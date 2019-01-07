using System;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;
using System.Threading;


namespace XR.Mono.Cover
{
    public class CodeRecordData : IDisposable
    {
        bool checkedDb = false;

        SqliteConnection con = null;


        public void Open (string file)
        {
            con = new SqliteConnection (string.Format ("URI=file:{0}", file));
            con.Open ();
            Tables ();
            //NonQuery( @"DELETE FROM methods" );
            //NonQuery( @"DELETE FROM calls" );
            //NonQuery( @"DELETE FROM lines" );
        }

        public void Close()
        {
            con.Close();
        }

        public void NonQuery( string cmdstr )
        {
            using (var cmd = new SqliteCommand( con )) {
                cmd.CommandText = cmdstr;
                cmd.ExecuteNonQuery();
            }
        }

        public List<CodeRecord> Load()
        {
            var rv = new List<CodeRecord>();

            // load code points
            using ( var tx = con.BeginTransaction() )
            using ( var cmd = new SqliteCommand( con ) ){
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT assembly, sourcefile, classname, name FROM methods";
                using ( var sth = cmd.ExecuteReader() ) {
                    while ( sth.HasRows && sth.Read() ){
                        var rec = new CodeRecord();
                        rec.Assembly = Convert.ToString( sth["assembly"] );
                        rec.SourceFile = Convert.ToString( sth["sourcefile"] );
                        rec.ClassName = Convert.ToString( sth["classname"] );
                        rec.Name = Convert.ToString( sth["name"] );

                        // get call count
                        var calls = new SqliteCommand( con );
                        calls.CommandText = "SELECT hits FROM calls WHERE assembly = :ASSEMBLY AND classname = :CLASSNAME AND name = :NAME";
                        calls.Parameters.Add( new SqliteParameter( ":ASSEMBLY", rec.Assembly ) );
                        calls.Parameters.Add( new SqliteParameter( ":CLASSNAME", rec.ClassName ) );
                        calls.Parameters.Add( new SqliteParameter( ":NAME", rec.Name ) );
                        var ccount = Convert.ToInt32( calls.ExecuteScalar() );
                        rec.CallCount = ccount;

                        // get lines
                        var lines = new SqliteCommand( con );
                        lines.CommandText = "SELECT line, hits FROM lines WHERE assembly = :ASSEMBLY AND classname = :CLASSNAME AND name = :NAME";
                        lines.Parameters.Add( new SqliteParameter( ":ASSEMBLY", rec.Assembly ) );
                        lines.Parameters.Add( new SqliteParameter( ":CLASSNAME", rec.ClassName ) );
                        lines.Parameters.Add( new SqliteParameter( ":NAME", rec.Name ) );
                        using ( var lsth = lines.ExecuteReader() ){
                            while ( lsth.HasRows && lsth.Read() ) {
                                var l = Convert.ToInt32( lsth["line"] );
                                var hc = Convert.ToInt32( lsth["hits"] );
                                rec.AddLines(l);
                                rec.SetHits( l, hc );
                            }
                        }

                        rv.Add(rec);
                    }
                }
                tx.Commit();
            }

            return rv;
        }

        public void RegisterMethod(CodeRecord m ){
            RegisterMethods( new CodeRecord[] { m } );
        }

 

        public void RegisterMethods( IEnumerable<CodeRecord> methods )
        {
            using ( var tx = con.BeginTransaction() )
            using ( var cmd = new SqliteCommand( con ) ){
                cmd.Transaction = tx;
                cmd.CommandText = @"REPLACE INTO methods ( assembly, sourcefile, classname, name ) 
                    VALUES ( :ASSEMBLY, :SOURCEFILE, :CLASSNAME, :METHNAME )";
                cmd.Parameters.Add( new SqliteParameter(  ":ASSEMBLY" ) );
                cmd.Parameters.Add( new SqliteParameter(  ":SOURCEFILE" ) );
                cmd.Parameters.Add( new SqliteParameter(  ":CLASSNAME" ) );
                cmd.Parameters.Add( new SqliteParameter(  ":METHNAME" ) );

                foreach ( var newmethod in methods ) {
                    if ( newmethod.Saved ) continue;

                    cmd.Parameters[":ASSEMBLY"].Value =  newmethod.Assembly;
                    cmd.Parameters[":SOURCEFILE"].Value = newmethod.SourceFile;
                    cmd.Parameters[":CLASSNAME"].Value = newmethod.ClassName;
                    cmd.Parameters[":METHNAME"].Value = newmethod.Name;
                    cmd.ExecuteNonQuery();
                }
                    
                tx.Commit();
            }
        }

        static object dbLock = new object();

        public void RegisterHits( IEnumerable<CodeRecord> methods, bool zerohits )
        {
            lock (dbLock)
            using ( var tx = con.BeginTransaction() )
            using ( var chits = new SqliteCommand( con ) )
            using ( var ccalls = new SqliteCommand( con ) ){
                chits.Transaction = tx;
                chits.CommandText = @"REPLACE INTO lines ( assembly, classname, name, line, hits ) 
                        VALUES ( :ASSEMBLY, :CLASSNAME, :NAME, :LINE, :HITS )";
                chits.Parameters.Add( new SqliteParameter( ":ASSEMBLY" ) );
                chits.Parameters.Add( new SqliteParameter( ":CLASSNAME" ) );
                chits.Parameters.Add( new SqliteParameter( ":NAME" ) );
                chits.Parameters.Add( new SqliteParameter( ":LINE" ) );
                chits.Parameters.Add( new SqliteParameter( ":HITS" ) );


                ccalls.Transaction = tx;
                ccalls.CommandText = @"REPLACE INTO calls ( assembly, classname, name, hits ) 
                    VALUES ( :ASSEMBLY, :CLASSNAME, :NAME, :HITS )";
                ccalls.Parameters.Add( new SqliteParameter( ":ASSEMBLY" ) );
                ccalls.Parameters.Add( new SqliteParameter( ":CLASSNAME" ) );
                ccalls.Parameters.Add( new SqliteParameter( ":NAME" ) );
                ccalls.Parameters.Add( new SqliteParameter( ":HITS" ) );
                
                foreach ( var method in methods )
                {
                    if ( method.Saved ) continue;
                    CoverHost.Singleton.Log("saving {0}:{1}", method.ClassName, method.Name );
                    ccalls.Parameters[":ASSEMBLY"].Value =  method.Assembly;
                    ccalls.Parameters[":CLASSNAME"].Value = method.ClassName;
                    ccalls.Parameters[":NAME"].Value = method.Name;
                    ccalls.Parameters[":HITS"].Value = method.CallCount;
                    ccalls.ExecuteNonQuery();

                    foreach ( var line in method.GetLines() )
                    {
                        var hits = method.GetHits(line);
                        
                        if ( hits > 0 || zerohits ){
                            chits.Parameters[":ASSEMBLY"].Value = method.Assembly;
                            chits.Parameters[":CLASSNAME"].Value = method.ClassName;
                            chits.Parameters[":NAME"].Value = method.Name;
                            chits.Parameters[":LINE"].Value = line;
                            chits.Parameters[":HITS"].Value = hits;
                            chits.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();
                CoverHost.Singleton.Log("saved");
            }
        }

        public void SaveMeta( string key, string val )
        {
            lock (dbLock ){
                using ( var cmd = new SqliteCommand( con ) ) {
                    cmd.CommandText = "REPLACE INTO meta ( item, val ) VALUES ( :ITEM, :VAL )";
                    cmd.Parameters.Add( new SqliteParameter( ":ITEM",  key ) );
                    cmd.Parameters.Add( new SqliteParameter( ":VAL",  val ) );
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Dictionary<string,string> LoadMeta()
        {
            var rv = new Dictionary<string,string>();
            lock ( dbLock ){
                using ( var cmd = new SqliteCommand( con ) )
                {
                    cmd.CommandText = @"SELECT item, val FROM meta";
                    using ( var sth = cmd.ExecuteReader() ) {
                        while ( sth.HasRows && sth.Read() ){
                            var k = Convert.ToString( sth["item"] );
                            var v = Convert.ToString( sth["val"] );
                            if ( !string.IsNullOrEmpty(k) ){
                                rv[k] = v;
                            }
                        }
                    }
                }
            }
            return rv;
        }

        void Tables ()
        {
            if (checkedDb)
                return;

            NonQuery( @"CREATE TABLE IF NOT EXISTS methods ( sourcefile TEXT, classname TEXT, name TEXT, assembly TEXT )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS methods_idx ON methods ( assembly, classname, name )" );

            NonQuery( @"CREATE TABLE IF NOT EXISTS calls ( classname TEXT, name TEXT, assembly TEXT, hits INTEGER )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS calls_idx ON calls ( assembly, classname, name )" );

            NonQuery( @"CREATE TABLE IF NOT EXISTS lines ( classname TEXT, name TEXT, assembly TEXT, line INTEGER, hits INTEGER )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS lines_idx ON lines ( assembly, classname, name, line )" );

            NonQuery( @"CREATE TABLE IF NOT EXISTS meta ( item TEXT, val TEXT )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS meta_idx ON meta ( item )" );


            checkedDb = true;


        }

        public void Dispose ()
        {
            if (con != null) {
                try {

                } catch {
                }
            }
        }
    }
}

