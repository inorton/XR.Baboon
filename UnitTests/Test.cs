using System;
using System.IO;
using NUnit.Framework;

using XR.Mono.Cover;

namespace UnitTests
{
    [TestFixture()]
    public class Test
    {
        [SetUp]
        public void Init ()
        {
            if (File.Exists ("testsubject.exe.covdb"))
                File.Delete ("testsubject.exe.covdb");
        }

        [Test()]
        public void Cover ()
        {

            var h = CoverHostFactory.CreateHost ("testsubject.exe.covdb", "testsubject.exe");
            h.Cover ("^testsubject");

        }
    }
}

