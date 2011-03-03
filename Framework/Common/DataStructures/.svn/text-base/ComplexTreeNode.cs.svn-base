// This collection of non-binary tree data structures created by Dan Vanderboom.
// Critical Development blog: http://dvanderboom.wordpress.com
// Original Tree<T> blog article: http://dvanderboom.wordpress.com/2008/03/15/treet-implementing-a-non-binary-tree-in-c/
// Linked-in: http://www.linkedin.com/profile?viewProfile=&key=13009616&trk=tab_pro

using System;
using System.Text;
using System.Collections.Generic;

namespace Magic.Common.DataStructures.Tree
{
    /// <summary>
    /// Represents a node in a Tree structure, with a parent node and zero or more child nodes.
    /// </summary>
    public class ComplexTreeNode<T> : IDisposable where T : ComplexTreeNode<T>
    {
        private T _Parent;
        public T Parent
        {
            get { return _Parent; }
            set
            {
                if (value == _Parent)
                {
                    return;
                }

                if (_Parent != null)
                {
                    _Parent.Children.Remove(this);
                }

                if (value != null && !value.Children.Contains(this))
                {
                    value.Children.Add(this);
                }

                _Parent = value;
            }
        }

        public T Root
        {
            get
            {
                //return (Parent == null) ? this : Parent.Root;

                ComplexTreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }
                return (T)node;
            }
        }

        private ComplexTreeNodeList<T> _Children;
        public virtual ComplexTreeNodeList<T> Children
        {
            get { return _Children; }
            private set { _Children = value; }
        }

        private TreeTraversalDirection _DisposeTraversal = TreeTraversalDirection.BottomUp;
        /// <summary>
        /// Specifies the pattern for traversing the Tree for disposing of resources. Default is BottomUp.
        /// </summary>
        public TreeTraversalDirection DisposeTraversal
        {
            get { return _DisposeTraversal; }
            set { _DisposeTraversal = value; }
        }

        public ComplexTreeNode()
        {
            Parent = null;
            Children = new ComplexTreeNodeList<T>(this);
        }

        public ComplexTreeNode(T Parent)
        {
            this.Parent = Parent;
            Children = new ComplexTreeNodeList<T>(this);
        }

        public ComplexTreeNode(ComplexTreeNodeList<T> Children)
        {
            Parent = null;
            this.Children = Children;
            Children.Parent = (T)this;
        }

        public ComplexTreeNode(T Parent, ComplexTreeNodeList<T> Children)
        {
            this.Parent = Parent;
            this.Children = Children;
            Children.Parent = (T)this;
        }

        /// <summary>
        /// Reports a depth of nesting in the tree, starting at 0 for the root.
        /// </summary>
        public int Depth
        {
            get
            {
                //return (Parent == null ? -1 : Parent.Depth) + 1;

                int depth = 0;
                ComplexTreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    depth++;
                }
                return depth;
            }
        }

        public override string ToString()
        {
            string Description = "Depth=" + Depth.ToString() + ", Children=" + Children.Count.ToString();
            if (this == Root)
            {
                Description += " (Root)";
            }
            return Description;
        }

        #region IDisposable

        private bool _IsDisposed;
        public bool IsDisposed
        {
            get { return _IsDisposed; }
        }

        public virtual void Dispose()
        {
            CheckDisposed();

            // clean up contained objects (in Value property)
            if (DisposeTraversal == TreeTraversalDirection.BottomUp)
            {
                foreach (ComplexTreeNode<T> node in Children)
                {
                    node.Dispose();
                }
            }

            OnDisposing();

            if (DisposeTraversal == TreeTraversalDirection.TopDown)
            {
                foreach (ComplexTreeNode<T> node in Children)
                {
                    node.Dispose();
                }
            }

            // TODO: clean up the tree itself

            _IsDisposed = true;
        }

        public event EventHandler Disposing;

        protected void OnDisposing()
        {
            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }

        protected void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion
    }
}