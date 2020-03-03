// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Host.Loggers
{
    internal class FunctionInstanceLogger : IFunctionInstanceLogger
    {
        private readonly ILoggerFactory _loggerFactory;

        public FunctionInstanceLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task<string> LogFunctionStartedAsync(FunctionStartedMessage message, CancellationToken cancellationToken)
        {
            string name = message.Function.ShortName;
            string reason = message.ReasonDetails ?? message.FormatReason();
            var id = message.FunctionInstanceId;

            Log(LogLevel.Information, message.Function, (s, e) => $"Executing '{name}' (Reason='{reason}', Id={id})");

            if (message.TriggerDetails != null && message.TriggerDetails.Count != 0)
            {
                LogTemplatizedTriggerDetails(message);
            }

            return Task.FromResult<string>(null);
        }

        private void LogTemplatizedTriggerDetails(FunctionStartedMessage message)
        {
            var templateKeys = message.TriggerDetails.Select(entry => $"{entry.Key}: {{{entry.Key}}}");
            string messageTemplate = "Trigger Details: " + string.Join(", ", templateKeys);
            string[] templateValues = message.TriggerDetails.Values.ToArray();

            Log(LogLevel.Information, message.Function, messageTemplate, templateValues);
        }

        private void Log(LogLevel level, FunctionDescriptor descriptor, string message, params object[] args)
        {
            ILogger logger = _loggerFactory?.CreateLogger(LogCategories.CreateFunctionCategory(descriptor.LogName));
            logger?.Log(level, 0, message, args);
        }

        public Task LogFunctionCompletedAsync(FunctionCompletedMessage message, CancellationToken cancellationToken)
        {
            string name = message.Function.ShortName;
            var id = message.FunctionInstanceId;

            if (message.Succeeded)
            {
                Log(LogLevel.Information, message.Function, (s, e) => $"Executed '{name}' (Succeeded, Id={id})");
            }
            else
            {
                Log(LogLevel.Error, message.Function, (s, e) => $"Executed '{name}' (Failed, Id={id})", message.Failure.Exception);
            }

            return Task.CompletedTask;
        }

        private void Log(LogLevel level, FunctionDescriptor descriptor, Func<string, Exception, string> formatter, Exception exception = null)
        {
            ILogger logger = _loggerFactory?.CreateLogger(LogCategories.CreateFunctionCategory(descriptor.LogName));
            logger?.Log(level, 0, null, exception, formatter);
        }

        public Task DeleteLogFunctionStartedAsync(string startedMessageId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
