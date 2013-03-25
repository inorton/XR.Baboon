using System;
using Mono.Data.Sqlite;

namespace XR.Mono.Cover
{
    public class CodeRecordData : IDisposable
    {
        bool checkedDb = false;

        SqliteConnection con = null;

        public CodeRecordData ()
        {

        }

        public void Open (string file)
        {
            con = new SqliteConnection (string.Format ("URI=file:{0}", file));
            con.Open ();
            Tables ();
        }

        void Tables ()
        {
            if (checkedDb)
                return;

            using (var cmd = new SqliteCommand( con )) {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS methods ( 
                    TEXT fullname PRIMARY KEY, TEXT sourcefile, TEXT classname, TEXT name, TEXT assembly )";
                cmd.ExecuteNonQuery ();
            }

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

