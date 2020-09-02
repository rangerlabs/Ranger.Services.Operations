using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            var messageTypes = MessagesAssembly
                .GetTypes()
                .Where(t => t.IsClass && typeof(TMessage).IsAssignableFrom(t))
                .ToList();

            messageTypes.ForEach(mt =>
            {
                subscriber.GetType()
                    .GetMethod(subscribeMethod)
                    .MakeGenericMethod(mt)
                    .Invoke(subscriber, new object[] { null });
            });

            return subscriber;
        }
    }
}