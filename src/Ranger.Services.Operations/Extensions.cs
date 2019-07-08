using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Chronicle;

namespace Ranger.RabbitMQ {
    internal static class Extensions {
        private static readonly Type[] SagaTypes = Assembly.GetExecutingAssembly ()
            .GetTypes ()
            .Where (t => t.IsAssignableTo<ISaga> ())
            .ToArray ();

        internal static bool BelongsToSaga<TMessage> (this TMessage _) where TMessage : IMessage => SagaTypes.Any (t => t.IsAssignableTo<ISagaAction<TMessage>> ());
    }
}