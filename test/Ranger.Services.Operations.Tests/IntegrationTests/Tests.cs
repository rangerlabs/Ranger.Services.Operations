using System.Threading.Tasks;
using Chronicle;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;
using Ranger.Services.Operations.Data;
using Shouldly;
using Xunit;

namespace Ranger.Services.Operations.Tests
{
    public class HandlerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IBusSubscriber busSubscriber;
        private readonly IOperationsRepository operationsRepository;
        private readonly CustomWebApplicationFactory _factory;

        public HandlerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
            this.operationsRepository = factory.Services.GetService(typeof(IOperationsRepository)) as IOperationsRepository;
        }

        [Fact]
        public void Operations_Starts()
        { }


        [Fact]
        public void ChronicleConfiguration_AllowConcurrentWrites_IsFalse()
        {
            var chronicleConfig = _factory.Services.GetService(typeof(IChronicleConfiguration)) as ChronicleConfiguration;
            chronicleConfig.AllowConcurrentWrites.ShouldBeFalse();
        }

    }
}