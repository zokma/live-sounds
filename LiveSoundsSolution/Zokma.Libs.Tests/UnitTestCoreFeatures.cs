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
            var info1 = Pathfinder.FindPathInfo("sub/test.dat");
            var info2 = Pathfinder.FindPathInfo("sub\\test.dat");

            var name1 = Pathfinder.FindPathName("sub/test.dat");
            var name2 = Pathfinder.FindPathName("sub\\test.dat");

            output.WriteLine("Path1: {0}", info1.FullName);
            output.WriteLine("Path2: {0}", info2.FullName);
            output.WriteLine("Path3: {0}", name1);
            output.WriteLine("Path4: {0}", name2);

            Assert.Equal(info1.FullName, info2.FullName);
            Assert.Equal(name1, name2);
            Assert.Equal(info1.FullName, name1);
            Assert.Equal(info2.FullName, name2);

            Assert.StartsWith(Pathfinder.ApplicationDirectory, info1.FullName);
        }

        [Theory]
        [InlineData("./data/test.dat")]
        [InlineData("../data/test.dat")]
        [InlineData("/data/test.dat")]
        [InlineData("\\data\\test.dat")]
        [InlineData("C:\\data\\test.dat")]
        [InlineData("")]
        [InlineData(null)]
        public void TestPathfinderFindException(string path)
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var info = Pathfinder.FindPathInfo(path);
                });

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var info = Pathfinder.FindPathName(path);
                });
        }
    }
}
