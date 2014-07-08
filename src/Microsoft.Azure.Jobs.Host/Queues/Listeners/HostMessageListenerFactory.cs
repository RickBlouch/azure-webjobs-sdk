﻿using Microsoft.Azure.Jobs.Host.Bindings;
using Microsoft.Azure.Jobs.Host.Executors;
using Microsoft.Azure.Jobs.Host.Indexers;
using Microsoft.Azure.Jobs.Host.Listeners;
using Microsoft.Azure.Jobs.Host.Loggers;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.Jobs.Host.Queues.Listeners
{
    internal static class HostMessageListener
    {
        public static IListener Create(CloudQueue queue, IExecuteFunction executor, IFunctionIndexLookup functionLookup,
            IFunctionInstanceLogger functionInstanceLogger, HostBindingContext context)
        {
            ITriggerExecutor<CloudQueueMessage> triggerExecutor = new HostMessageExecutor(executor, functionLookup,
                functionInstanceLogger, context);
            ICanFailCommand command = new PollQueueCommand(queue, poisonQueue: null, triggerExecutor: triggerExecutor);
            IntervalSeparationTimer timer = ExponentialBackoffTimerCommand.CreateTimer(command,
                QueuePollingIntervals.Minimum, QueuePollingIntervals.Maximum);
            return new TimerListener(timer);
        }
    }
}
