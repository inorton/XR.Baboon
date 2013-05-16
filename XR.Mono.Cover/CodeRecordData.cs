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
                cmd.CommandText = @"SELECT fullname, assembly, sourcefile, classname, name FROM methods";
                using ( var sth = cmd.ExecuteReader() ) {
                    while ( sth.HasRows && sth.Read() ){
                        var rec = new CodeRecord();
                        rec.FullMethodName = Convert.ToString( sth["fullname"] );
                        rec.Assembly = Convert.ToString( sth["assembly"] );
                        rec.SourceFile = Convert.ToString( sth["sourcefile"] );
                        rec.ClassName = Convert.ToString( sth["classname"] );
                        rec.Name = Convert.ToString( sth["name"] );

                        // get call count
                        var calls = new SqliteCommand( con );
                        calls.CommandText = "SELECT hits FROM calls WHERE assembly = :ASSEMBLY AND fullname = :FULLNAME";
                        calls.Parameters.Add( new SqliteParameter( ":ASSEMBLY", rec.Assembly ) );
                        calls.Parameters.Add( new SqliteParameter( ":FULLNAME", rec.FullMethodName ) );
                        var ccount = Convert.ToInt32( calls.ExecuteScalar() );
                        rec.CallCount = ccount;

                        // get lines
                        var lines = new SqliteCommand( con );
                        lines.CommandText = "SELECT line, hits FROM lines WHERE assembly = :ASSEMBLY AND fullname = :FULLNAME";
                        lines.Parameters.Add( new SqliteParameter( ":ASSEMBLY", rec.Assembly ) );
                        lines.Parameters.Add( new SqliteParameter( ":FULLNAME", rec.FullMethodName ) );
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

        object reglock = new object();
        int regcount = 0;

        public void RegisterMethods( IEnumerable<CodeRecord> methodslist )
        {
            lock (reglock) regcount++;

            while ( regcount > 2 ) Thread.Sleep(40);

            ThreadPool.QueueUserWorkItem( (x) => {
                var methods = x as IEnumerable<CodeRecord>;
                using ( var tx = con.BeginTransaction() )
                using ( var cmd = new SqliteCommand( con ) ){
                    cmd.Transaction = tx;
                    cmd.CommandText = @"REPLACE INTO methods ( fullname, assembly, sourcefile, classname, name ) 
                    VALUES ( :FULLNAME, :ASSEMBLY, :SOURCEFILE, :CLASSNAME, :METHNAME )";
                    cmd.Parameters.Add( new SqliteParameter(  ":FULLNAME" ) );
                    cmd.Parameters.Add( new SqliteParameter(  ":ASSEMBLY" ) );
                    cmd.Parameters.Add( new SqliteParameter(  ":SOURCEFILE" ) );
                    cmd.Parameters.Add( new SqliteParameter(  ":CLASSNAME" ) );
                    cmd.Parameters.Add( new SqliteParameter(  ":METHNAME" ) );

                    foreach ( var newmethod in methods ) {
                        if ( newmethod.Saved ) continue;

                        cmd.Parameters[":FULLNAME"].Value = newmethod.FullMethodName;
                        cmd.Parameters[":ASSEMBLY"].Value =  newmethod.Assembly;
                        cmd.Parameters[":SOURCEFILE"].Value = newmethod.SourceFile;
                        cmd.Parameters[":CLASSNAME"].Value = newmethod.ClassName;
                        cmd.Parameters[":METHNAME"].Value = newmethod.Name;
                        cmd.ExecuteNonQuery();
                    }
                    //tx.Commit();
                }
                RegisterHits( methods, true);
                lock (reglock)
                    regcount--;

            }, methodslist );

        }

        static object dbLock = new object();

        public void RegisterHits( IEnumerable<CodeRecord> methods, bool zerohits )
        {
            lock (dbLock)
            using ( var tx = con.BeginTransaction() )
            using ( var chits = new SqliteCommand( con ) )
            using ( var ccalls = new SqliteCommand( con ) ){
                chits.Transaction = tx;
                chits.CommandText = @"REPLACE INTO lines ( fullname, assembly, line, hits ) 
                        VALUES ( :FULLNAME, :ASSEMBLY, :LINE, :HITS )";
                chits.Parameters.Add( new SqliteParameter( ":FULLNAME" ) );
                chits.Parameters.Add( new SqliteParameter( ":ASSEMBLY" ) );
                chits.Parameters.Add( new SqliteParameter( ":LINE" ) );
                chits.Parameters.Add( new SqliteParameter( ":HITS" ) );


                ccalls.Transaction = tx;
                ccalls.CommandText = @"REPLACE INTO calls ( fullname, assembly, hits ) 
                    VALUES ( :FULLNAME, :ASSEMBLY, :HITS )";
                ccalls.Parameters.Add( new SqliteParameter( ":FULLNAME" ) );
                ccalls.Parameters.Add( new SqliteParameter( ":ASSEMBLY" ) );
                ccalls.Parameters.Add( new SqliteParameter( ":HITS" ) );
                
                foreach ( var method in methods )
                {
                    if ( method.Saved ) continue;
                    CoverHost.Singleton.Log("saving {0}", method.FullMethodName );
                    ccalls.Parameters[":FULLNAME"].Value = method.FullMethodName;
                    ccalls.Parameters[":ASSEMBLY"].Value =  method.Assembly;
                    ccalls.Parameters[":HITS"].Value = method.CallCount;
                    ccalls.ExecuteNonQuery();

                    foreach ( var line in method.GetLines() )
                    {
                        var hits = method.GetHits(line);
                        
                        if ( hits > 0 || zerohits ){
                            chits.Parameters[":FULLNAME"].Value = method.FullMethodName;
                            chits.Parameters[":ASSEMBLY"].Value = method.Assembly;
                            chits.Parameters[":LINE"].Value = line;
                            chits.Parameters[":HITS"].Value = hits;
                            chits.ExecuteNonQuery();
                        }
                    }
                }
            
                //tx.Commit();
            }
        }

        void Tables ()
        {
            if (checkedDb)
                return;

            NonQuery( @"CREATE TABLE IF NOT EXISTS methods ( fullname TEXT, sourcefile TEXT, classname TEXT, name TEXT, assembly TEXT )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS methods_idx ON methods ( fullname, assembly )" );

            NonQuery( @"CREATE TABLE IF NOT EXISTS calls ( fullname TEXT, assembly TEXT, hits INTEGER )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS calls_idx ON calls ( fullname, assembly )" );

            NonQuery( @"CREATE TABLE IF NOT EXISTS lines ( fullname TEXT, assembly TEXT, line INTEGER, hits INTEGER )" );
            NonQuery( @"CREATE UNIQUE INDEX IF NOT EXISTS lines_idx ON lines ( fullname, assembly, line )" );


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

