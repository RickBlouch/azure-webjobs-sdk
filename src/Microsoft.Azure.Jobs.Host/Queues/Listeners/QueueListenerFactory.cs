﻿using System;
using System.Diagnostics;
using Microsoft.Azure.Jobs.Host.Executors;
using Microsoft.Azure.Jobs.Host.Listeners;
using Microsoft.Azure.Jobs.Host.Triggers;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.Jobs.Host.Queues.Listeners
{
    internal class QueueListenerFactory : IListenerFactory
    {
        private static string poisonQueueSuffix = "-poison";

        private readonly CloudQueue _queue;
        private readonly CloudQueue _poisonQueue;
        private readonly ITriggeredFunctionInstanceFactory<CloudQueueMessage> _instanceFactory;

        public QueueListenerFactory(CloudQueue queue,
            ITriggeredFunctionInstanceFactory<CloudQueueMessage> instanceFactory)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            else if (instanceFactory == null)
            {
                throw new ArgumentNullException("instanceFactory");
            }

            _queue = queue;
            _poisonQueue = CreatePoisonQueueReference(queue.ServiceClient, queue.Name);
            _instanceFactory = instanceFactory;
        }

        public IListener Create(IFunctionExecutor executor, ListenerFactoryContext context)
        {
            QueueTriggerExecutor triggerExecutor = new QueueTriggerExecutor(_instanceFactory, executor);
            ICanFailCommand command = new PollQueueCommand(_queue, _poisonQueue, triggerExecutor);
            IntervalSeparationTimer timer = ExponentialBackoffTimerCommand.CreateTimer(command,
                QueuePollingIntervals.Minimum, QueuePollingIntervals.Maximum);
            return new TimerListener(timer);
        }

        private static CloudQueue CreatePoisonQueueReference(CloudQueueClient client, string name)
        {
            Debug.Assert(client != null);

            // Only use a corresponding poison queue if:
            // 1. The poison queue name would be valid (adding "-poison" doesn't make the name too long), and
            // 2. The queue itself isn't already a poison queue.

            if (name == null || name.EndsWith(poisonQueueSuffix, StringComparison.Ordinal))
            {
                return null;
            }

            string possiblePoisonQueueName = name + poisonQueueSuffix;

            if (!QueueClient.IsValidQueueName(possiblePoisonQueueName))
            {
                return null;
            }

            return client.GetQueueReference(possiblePoisonQueueName);
        }
    }
}
