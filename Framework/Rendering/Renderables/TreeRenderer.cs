using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common.DataStructures;
using Magic.Common.DataStructures.Tree;
using Magic.Common;

namespace Magic.Rendering.Renderables
{
    
    public class TreeRenderer<T> : IRender where T: IAsPoint 
    {
        #region IRender Members

        SimpleTreeNode<T> treeRoot = new SimpleTreeNode<T>();

        public string GetName()
        {
            return "TreeRenderer";
        }

        public void UpdateTree(SimpleTreeNode<T> newTree)
        {
            treeRoot = newTree;
        }

        public void Draw(Renderer cam)
        {
            DrawChildren(treeRoot);
        }

        private void DrawChildren(SimpleTreeNode<T> node)
        {
            foreach (SimpleTreeNode<T> child in node.Children)
            {
                Vector2[] treeLine = new Vector2[2];
                treeLine[0] = node.Value.Point;
                treeLine[1] = child.Value.Point;
                GLUtility.DrawLines(new GLPen(Color.Blue, 1.0f), treeLine);
                DrawChildren(child);
            }
        }

        public void ClearBuffer()
        {
        }

        public bool VehicleRelative
        {
            get { return false; }
        }

        public int? VehicleRelativeID
        {
            get { return null; }
        }

        #endregion
    }
}
