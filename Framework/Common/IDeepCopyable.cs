using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
    public interface IDeepCopyable
    {
        object DeepCopy();
    }
}
