using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.DataInterfaces
{
    public interface ITimeComparable
    {
        double TimeStamp { get; set; }
        int CompareTo(ITimeComparable obj);
    }
}
