// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Microsoft.Azure.WebJobs.Host.Triggers
{
    /// <summary>
    /// Represents data returned after a trigger parameter value is bound.
    /// </summary>
    public class TriggerData : ITriggerData
    {
        private readonly IValueProvider _valueProvider;
        private readonly Func<IReadOnlyDictionary<string, object>> _bindingDataFactory;
        private IReadOnlyDictionary<string, object> _bindingData;

        /// <summary>
        /// Creates a new instance. No ValueProvider used here since the converter pipeline will take care of it. 
        /// </summary>
        /// <param name="bindingData"></param>
        public TriggerData(IReadOnlyDictionary<string, object> bindingData)
        {
            _bindingData = bindingData;
        }

        public TriggerData(Func<IReadOnlyDictionary<string, object>> bindingDataFactory)
        {
            _bindingDataFactory = bindingDataFactory;
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="valueProvider"></param>
        /// <param name="bindingData"></param>
        public TriggerData(IValueProvider valueProvider, IReadOnlyDictionary<string, object> bindingData)
        {
            _valueProvider = valueProvider;
            _bindingData = bindingData;
        }

        public TriggerData(IValueProvider valueProvider, Func<IReadOnlyDictionary<string, object>> bindingDataFactory)
        {
            _valueProvider = valueProvider;
            _bindingDataFactory = bindingDataFactory;
        }

        /// <inheritdoc/>
        public IValueProvider ValueProvider
        {
            get { return _valueProvider; }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> BindingData
        {
            get 
            { 
                if (_bindingDataFactory != null && _bindingData == null)
                {
                    _bindingData = _bindingDataFactory();
                }
                return _bindingData; 
            }
        }

        /// <summary>
        /// If non-null, then this trigger handles a return value. 
        /// The binding data contract should have a "$return" entry of byref type too. 
        /// </summary>
        public IValueBinder ReturnValueProvider { get; set; }
    }
}
