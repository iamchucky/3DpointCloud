// This collection of non-binary tree data structures created by Dan Vanderboom.
// Critical Development blog: http://dvanderboom.wordpress.com
// Original Tree<T> blog article: http://dvanderboom.wordpress.com/2008/03/15/treet-implementing-a-non-binary-tree-in-c/
// Linked-in: http://www.linkedin.com/profile?viewProfile=&key=13009616&trk=tab_pro

using System;
using System.Text;
using System.Collections.Generic;

namespace Magic.Common.DataStructures.Tree
{
    public enum TreeTraversalType
    {
        DepthFirst,
        BreadthFirst
    }

    public enum TreeTraversalDirection
    {
        TopDown,
        BottomUp
    }
}