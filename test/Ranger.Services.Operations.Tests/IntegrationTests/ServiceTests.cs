using Chronicle;
using Shouldly;
using Xunit;

namespace Ranger.Services.Operations.Tests
{
    public class ServiceTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ServiceTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

        }

        [Fact]
        public void ChronicleConfiguration_AllowConcurrentWrites_IsFalse()
        {
            var chronicleConfig = _factory.Services.GetService(typeof(IChronicleConfiguration)) as ChronicleConfiguration;
            chronicleConfig.AllowConcurrentWrites.ShouldBeFalse();
        }
    }
}