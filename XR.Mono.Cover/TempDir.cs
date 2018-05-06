using System;
using System.IO;

namespace XR.Mono.Cover
{
    public class TempDir : IDisposable
    {
        string path;

        public string TempPath {
            get {
                return path;
            }
        }

        public TempDir()
        {
            path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
        }

        public void Dispose ()
        {
            Directory.Delete (this.path, true);
        }
    }
}
