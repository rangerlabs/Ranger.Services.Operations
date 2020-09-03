using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chronicle;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusSubscriber;

namespace Ranger.Services.Operations
{
    public static class Subscriptions
    {
        private static readonly Assembly MessagesAssembly = typeof(Subscriptions).Assembly;

        public static IBusSubscriber SubscribeAllMessages(this IBusSubscriber subscriber) => subscriber.SubscribeAllCommands().SubscribeAllEvents();

        private static IBusSubscriber SubscribeAllCommands(this IBusSubscriber subscriber) => subscriber.SubscribeAllMessages<ICommand>(nameof(IBusSubscriber.SubscribeCommandWithHandler), new[] { typeof(Func<ICommand, RangerException, IRejectedEvent>) });

        private static IBusSubscriber SubscribeAllEvents(this IBusSubscriber subscriber) => subscriber.SubscribeAllMessages<IEvent>(nameof(IBusSubscriber.SubscribeEventWithHandler), new[] { typeof(Func<IEvent, RangerException, IRejectedEvent>) });

        public static IBusSubscriber SubscribeAllMessages<TMessage>(this IBusSubscriber subscriber, string subscribeMethod, Type[] signature)
            where TMessage : IMessage
        {

            var messageTypes = Subscriptions.SagaMessageTypes<TMessage>();

            messageTypes.ForEach(mt =>
            {
                subscriber.GetType()
                    .GetMethod(subscribeMethod)
                    .MakeGenericMethod(mt)
                    .Invoke(subscriber, new object[] { null });
            });

            return subscriber;
        }

        private static List<Type> SagaMessageTypes<TMessage>()
            where TMessage : IMessage
        {
            var MessagesAssembly = typeof(Subscriptions).Assembly;
            var typeFilter = new TypeFilter(ISagaActionFilter);
            var sagaMessages = MessagesAssembly.GetTypes()
                .Where(t => t.IsClass && (typeof(ISaga)).IsAssignableFrom(t))
                .SelectMany(t => t.FindInterfaces(typeFilter, t))
                .SelectMany(i => i.GenericTypeArguments)
                .Distinct()
                .ToList();
            return sagaMessages;
        }

        private static bool ISagaActionFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.IsGenericType)
            {
                var typeObjGeneric = typeObj.GetGenericTypeDefinition();
                var iSagaActionGeneric = typeof(ISagaAction<>).GetGenericTypeDefinition();
                if (typeObjGeneric.Equals(iSagaActionGeneric) || typeObjGeneric.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition().Equals(iSagaActionGeneric)))
                    return true;
                else
                    return false;
            }
            return false;
        }
    }
}