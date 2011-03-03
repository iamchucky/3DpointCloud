using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Magic.Rendering
{
    public interface IProvideContextMenu : IHittable
    {
        ICollection<MenuItem> GetMenuItems();
        void OnMenuOpening();
    }
}
