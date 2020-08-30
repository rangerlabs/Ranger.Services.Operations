using System.Threading.Tasks;
using Ranger.RabbitMQ;
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
        public Task OperationsService_Starts()
        {
            return Task.CompletedTask;
        }
    }
}