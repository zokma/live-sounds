using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Zokma.Libs.Tests
{
    public class UnitTestCoreFeatures
    {
        private readonly ITestOutputHelper output;

        public UnitTestCoreFeatures(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestPathfinderApplicationPath()
        {
            output.WriteLine("ApplicationPath: {0}", Pathfinder.ApplicationDirectory);

            Assert.NotNull(Pathfinder.ApplicationDirectory);
            Assert.Equal(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Pathfinder.ApplicationDirectory);
        }

        [Fact]
        public void TestPathfinderFind()
        {
            var info1 = Pathfinder.ApplicationRoot.FindPathInfo("sub/test.dat");
            var info2 = Pathfinder.ApplicationRoot.FindPathInfo("sub\\test.dat");

            var name1 = Pathfinder.ApplicationRoot.FindPathName("sub/test.dat");
            var name2 = Pathfinder.ApplicationRoot.FindPathName("sub\\test.dat");

            var pf = new Pathfinder("sub");

            var info3 = pf.FindPathInfo("test.dat");
            var name3 = pf.FindPathName("test.dat");

            output.WriteLine("Path1: {0}", info1.FullName);
            output.WriteLine("Path2: {0}", info2.FullName);
            output.WriteLine("Path3: {0}", info3.FullName);
            output.WriteLine("Path1: {0}", name1);
            output.WriteLine("Path2: {0}", name2);
            output.WriteLine("Path3: {0}", name2);

            Assert.Equal(info1.FullName, info2.FullName);
            Assert.Equal(name1, name2);
            Assert.Equal(info1.FullName, name1);
            Assert.Equal(info2.FullName, name2);
            Assert.Equal(info1.FullName, info3.FullName);
            Assert.Equal(name1, name3);

            Assert.StartsWith(Pathfinder.ApplicationDirectory, info1.FullName);
        }

        [Fact]
        public void TestPathfinderFindSubDir()
        {
            var pf1 = new Pathfinder("sub1", "sub2", "sub3");
            var pf2 = new Pathfinder("sub1").GetSubPathfinder("sub2").GetSubPathfinder("sub3");
            var pf3 = new Pathfinder("sub1").GetSubPathfinder("sub2", "sub3");

            output.WriteLine("Base P1: {0}", pf1.BaseDirectory);
            output.WriteLine("Base P2: {0}", pf2.BaseDirectory);
            output.WriteLine("Base P3: {0}", pf3.BaseDirectory);

            Assert.Equal(pf1.BaseDirectory, pf2.BaseDirectory);
            Assert.Equal(pf2.BaseDirectory, pf3.BaseDirectory);

            Assert.Equal(pf1.FindPathName("test.dat"), pf2.FindPathName("test.dat"));
            Assert.Equal(pf2.FindPathName("test.dat"), pf3.FindPathName("test.dat"));

            Assert.StartsWith(Pathfinder.ApplicationDirectory, pf1.BaseDirectory);
        }

        [Fact]
        public void TestPathfinderFindSpecialPath()
        {
            var info1 = new Pathfinder(PathKind.ApplicationRoot,      "sub1", "sub2");
            var info2 = new Pathfinder(PathKind.ApplicationData,      "sub1", "sub2");
            var info3 = new Pathfinder(PathKind.LocalApplicationData, "sub1", "sub2");
            var info4 = new Pathfinder(PathKind.Personal,             "sub1", "sub2");

            output.WriteLine("Path1: {0}", info1.FindPathName("test.data"));
            output.WriteLine("Path2: {0}", info2.FindPathName("test.data"));
            output.WriteLine("Path3: {0}", info3.FindPathName("test.data"));
            output.WriteLine("Path4: {0}", info4.FindPathName("test.data"));

            Assert.StartsWith(Pathfinder.ApplicationDirectory, info1.FindPathName("test.dat"));
            Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), info2.FindPathName("test.dat"));
            Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), info3.FindPathName("test.dat"));
            Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Personal), info4.FindPathName("test.dat"));
        }


        [Theory]
        [InlineData("../data/test.dat")]
        [InlineData("/data/test.dat")]
        [InlineData("\\data\\test.dat")]
        [InlineData("C:\\data\\test.dat")]
        [InlineData("data/../../test.dat")]
        [InlineData("\\data\\..\\..\\test.dat")]
        [InlineData(null)]
        public void TestPathfinderFindException(string path)
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var info = Pathfinder.ApplicationRoot.FindPathInfo(path);
                });

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var info = Pathfinder.ApplicationRoot.FindPathName(path);
                });
        }
    }
}
