using System;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;


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
            using ( var cmd = new SqliteCommand( con ) ){
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
                                rec.Lines.Add(l);
                                for ( int i = 0; i < hc; i++ ){
                                    rec.LineHits.Add(l);
                                }
                            }
                        }

                        rv.Add(rec);
                    }
                }
            }

            return rv;
        }

        public void RegisterMethod( CodeRecord newmethod )
        {
            using ( var cmd = new SqliteCommand( con ) ){
                cmd.CommandText = @"REPLACE INTO methods ( fullname, assembly, sourcefile, classname, name ) 
                    VALUES ( :FULLNAME, :ASSEMBLY, :SOURCEFILE, :CLASSNAME, :METHNAME )";
                cmd.Parameters.Add( new SqliteParameter(  ":FULLNAME", newmethod.FullMethodName ) );
                cmd.Parameters.Add( new SqliteParameter(  ":ASSEMBLY", newmethod.Assembly ) );
                cmd.Parameters.Add( new SqliteParameter(  ":SOURCEFILE", newmethod.SourceFile ) );
                cmd.Parameters.Add( new SqliteParameter(  ":CLASSNAME", newmethod.ClassName ) );
                cmd.Parameters.Add( new SqliteParameter(  ":METHNAME", newmethod.Name ) );
                cmd.ExecuteNonQuery();
            }
        }

        public void RegisterCalls( CodeRecord method )
        {
            using ( var cmd = new SqliteCommand( con ) ){
                cmd.CommandText = @"REPLACE INTO calls ( fullname, assembly, hits ) 
                    VALUES ( :FULLNAME, :ASSEMBLY, :HITS )";
                cmd.Parameters.Add( new SqliteParameter( ":FULLNAME", method.FullMethodName ) );
                cmd.Parameters.Add( new SqliteParameter( ":ASSEMBLY", method.Assembly ) );
                cmd.Parameters.Add( new SqliteParameter( ":HITS", method.CallCount ) );
                cmd.ExecuteNonQuery();
            }
        }

        public void RegisterHits( CodeRecord method )
        {
            foreach ( var line in method.Lines.Distinct() ) {
            using ( var cmd = new SqliteCommand( con ) ){
                    cmd.CommandText = @"REPLACE INTO lines ( fullname, assembly, line, hits ) 
                        VALUES ( :FULLNAME, :ASSEMBLY, :LINE, :HITS )";
                    cmd.Parameters.Add( new SqliteParameter( ":FULLNAME", method.FullMethodName ) );
                    cmd.Parameters.Add( new SqliteParameter( ":ASSEMBLY", method.Assembly ) );
                    cmd.Parameters.Add( new SqliteParameter( ":LINE", line ) );
                    cmd.Parameters.Add( new SqliteParameter( ":HITS", method.GetHits( line) ) );
                    cmd.ExecuteNonQuery();
                }
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

