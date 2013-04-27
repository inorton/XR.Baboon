using System;
using System.Collections.Generic;
using NUnit.Framework;

using XR.Mono.Cover;

namespace UnitTests
{
    [TestFixture()]
    public class TestFileSystemMap
    {
        [Test]
        [TestCase("foo/bar/baz/moose.cs", "bob/bar/baz/moose.cs", 
                  Result = "/bar/baz/moose.cs")]
        [TestCase("wobble/foo/bar/baz/moose.cs", "bob/bar/baz/moose.cs", 
                  Result = "/bar/baz/moose.cs")]
        [TestCase("zdsfsdfsd/dsfsdf/dsf/sdf/sdfsd/bar/baz/moose.cs", "bob/bar/baz/moose.cs", 
                  Result = "/bar/baz/moose.cs")]
        [TestCase("moose.cs", "bob/bar/baz/moose.cs", 
                  Result = "moose.cs")]
        [TestCase("dsfsdfsdfsdfsdfsdbar/baz/moose.cs", "bob/bar/baz/moose.cs",
                  Result = "bar/baz/moose.cs")]
        [TestCase(".cs", "bob/bar/baz/moose.cs",
                  Result = ".cs")]
        [TestCase("fsdbar/baz/moose.cs", "bob/bar/baz/moose.cs", 
                  Result = "bar/baz/moose.cs")]
        [TestCase("sdfsdbarbaz/moose.cs", "bob/bar/baz/moose.cs",
                  Result = "baz/moose.cs")]
        [TestCase("dfsdfsdfsd/dfsd/f/sdf/sdffdsfsd.cs", "gdfgdfg.txt",
                  Result = (string)null)]
        public string FindSubstrings (string a, string b)
        {
            var c = FilesystemMap.CommonPath( a, b );
            Console.Error.WriteLine(c);
            return c;
        }

        [Test]
        public void FindCommonParentPath()
        {
            var cr = new List<CodeRecord>();
            cr.Add( new CodeRecord() { Assembly = this.GetType().Assembly.FullName, 
                SourceFile = "/home/inb/things/sources/projects/asm1/file1.cs" } );
            cr.Add( new CodeRecord() { Assembly = this.GetType().Assembly.FullName, 
                SourceFile = "/home/inb/things/sources/projects/asm1/file2.cs" } );
            cr.Add( new CodeRecord() { Assembly = this.GetType().Assembly.FullName, 
                SourceFile = "/home/inb/things/sources/projects/asm1/file3.cs" } );
            cr.Add( new CodeRecord() { Assembly = this.GetType().Assembly.FullName, 
                SourceFile = "/home/inb/things/sources/projects/asm1/subdir/file4.cs" } );
            cr.Add( new CodeRecord() { Assembly = this.GetType().Assembly.FullName, 
                SourceFile = "/home/inb/things/sources/projects/asm1/subdir/subdir2/file5.cs" } );

            var x = new FilesystemMap();
            var parent = x.FindMainFolder( this.GetType().Assembly.FullName, cr );
            Assert.AreEqual( "/home/inb/things/sources/projects/asm1", parent );
        }
    }
}

