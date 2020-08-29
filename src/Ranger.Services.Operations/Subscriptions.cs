using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    public static class Subscriptions
    {
        private static readonly Assembly MessagesAssembly = typeof(Subscriptions).Assembly;

        public static IBusSubscriber SubscribeAllMessages(this IBusSubscriber subscriber) => subscriber.SubscribeAllCommands().SubscribeAllEvents();

        private static IBusSubscriber SubscribeAllCommands(this IBusSubscriber subscriber) => subscriber.SubscribeAllMessages<ICommand>(nameof(IBusSubscriber.SubscribeCommand), new[] { typeof(Func<ICommand, RangerException, IRejectedEvent>) });

        private static IBusSubscriber SubscribeAllEvents(this IBusSubscriber subscriber) => subscriber.SubscribeAllMessages<IEvent>(nameof(IBusSubscriber.SubscribeEvent), new[] { typeof(Func<IEvent, RangerException, IRejectedEvent>) });

        private static IBusSubscriber SubscribeAllMessages<TMessage>(this IBusSubscriber subscriber, string subscribeMethod, Type[] signature)
            where TMessage : IMessage
        {

            var messageTypes = MessagesAssembly
                .GetTypes()
                .Where(t => t.IsClass && typeof(TMessage).IsAssignableFrom(t))
                .ToList();

            messageTypes.ForEach(mt =>
                subscriber.GetType()
               .GetMethod(subscribeMethod, signature)
               .MakeGenericMethod(mt)
               .Invoke(subscriber,
                   new object[] { null }));

            return subscriber;
        }
    }
}