using FluentAssertions;
using NUnit.Framework;
using Remote;

namespace Tests
{
    [TestFixture]
    public class ServiceExplorerTests
    {
        private ServiceExplorer sut;

        [SetUp]
        public void SetUp()
        {
            sut = new ServiceExplorer();
        }

        [Test]
        public void GetServices_shouldReturnListOfServices()
        {
            var services = sut.GetServices("kubanb").Result;

            services.Should().NotBeEmpty();
        }
    }
}