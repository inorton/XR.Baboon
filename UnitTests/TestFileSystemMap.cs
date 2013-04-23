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
        [TestCase("foo/bar/baz/moose.cs", "bob/bar/baz/moose.cs", Result = "/bar/baz/moose.cs")]
        [TestCase("wobble/foo/bar/baz/moose.cs", "bob/bar/baz/moose.cs", Result = "/bar/baz/moose.cs")]
        [TestCase("zdsfsdfsd/dsfsdf/dsf/sdf/sdfsd/bar/baz/moose.cs", "bob/bar/baz/moose.cs", Result = "/bar/baz/moose.cs")]
        [TestCase("moose.cs", "bob/bar/baz/moose.cs", Result = "moose.cs")]
        [TestCase("dsfsdfsdfsdfsdfsdbar/baz/moose.cs", "bob/bar/baz/moose.cs", Result = "bar/baz/moose.cs")]
        [TestCase(".cs", "bob/bar/baz/moose.cs", Result = ".cs")]
        [TestCase("fsdbar/baz/moose.cs", "bob/bar/baz/moose.cs", Result = "bar/baz/moose.cs")]
        [TestCase("sdfsdbarbaz/moose.cs", "bob/bar/baz/moose.cs", Result = "baz/moose.cs")]
        [TestCase("dfsdfsdfsd/dfsd/f/sdf/sdffdsfsd.cs", "gdfgdfg.txt", Result = (string)null)]
        public string FindSubstrings (string a, string b)
        {
            var x = new FilesystemMap();
            var c = x.CommonPath( a, b );
            Console.Error.WriteLine(c);
            return c;
        }
    }
}

