using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;

namespace Magic.Rendering
{
    public interface ISelectable : IHittable
    {
        bool IsSelected{ get; }
        void OnSelect();
        void OnDeselect();
    }
}
