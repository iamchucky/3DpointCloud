using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common.DataStructures.Tree;
using Magic.Common;

namespace Magic.Rendering.Renderables
{
    public class Vector2TreeRenderer : IRender
    {
        #region IRender Members

        SimpleTree<Vector2> treeToDraw = new SimpleTree<Vector2>();

        public string GetName()
        {
            return "Vector2Tree";
        }

        public void UpdateTree(SimpleTree<Vector2> newTree)
        {
            treeToDraw = newTree;
        }

        public void Draw(Renderer cam)
        {
            DrawChildren(treeToDraw.Root);
        }

        private void DrawChildren(SimpleTreeNode<Vector2> node)
        {
            foreach (SimpleTreeNode<Vector2> child in node.Children)
            {
                Vector2[] treeLine = new Vector2[2];
                treeLine[0] = node.Value;
                treeLine[1] = child.Value;
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
