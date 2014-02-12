﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Jobs.Runners
{
    interface IInvokerFactory
    {
        IInvoker Create(Guid hostId);
    }
}