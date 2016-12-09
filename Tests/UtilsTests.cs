using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Remote;
using Remote.Utils;

namespace Tests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void TestGetLastLines()
        {
            var path = @"c:\remote.txt";
            bool readWholeFile;
            IEnumerable<string> lines = IOUtils.ReadTail(path, 3, out readWholeFile);

            var allLines = System.IO.File.ReadAllLines(path);
            var expectedLines = allLines.Skip(allLines.Length - 3).ToList();
            lines.Should().BeEquivalentTo(expectedLines);
        }

        [Test]
        public void TestConfig()
        {
            Config config = (Config)ConfigurationManager.GetSection("RemoteConfig");

            config.SharedLogFile.Should().NotBeNullOrEmpty();
            config.Machines.Should().NotBeEmpty();
        }
    }
}