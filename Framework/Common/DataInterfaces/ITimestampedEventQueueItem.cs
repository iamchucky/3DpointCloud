﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.DataInterfaces
{
    public interface ITimestampedEventQueueItem : ITimeComparable
    {
        string DataType { get;}
    }
}
