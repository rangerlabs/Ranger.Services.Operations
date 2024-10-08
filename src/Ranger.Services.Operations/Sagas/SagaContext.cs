using System;
using System.Collections.Generic;
using Chronicle;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{

    public class SagaContext : ISagaContext
    {
        public SagaId SagaId { get; }
        public string Originator { get; }
        public IReadOnlyCollection<ISagaContextMetadata> Metadata { get; }

        private SagaContext(SagaId sagaId, string originator)
        {
            SagaId = sagaId;
            Originator = originator;
        }
        public static ISagaContext FromCorrelationContext(ICorrelationContext context) => new SagaContext(context.CorrelationContextId.ToString(), context.Resource);

        public static ISagaContext Empty
            => new SagaContext(Guid.Empty.ToString(), string.Empty);

        public SagaContextError SagaContextError { get; set; }

        public ISagaContextMetadata GetMetadata(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetMetadata(string key, out ISagaContextMetadata metadata)
        {
            throw new NotImplementedException();
        }
    }
}