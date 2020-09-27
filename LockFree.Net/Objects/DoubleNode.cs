// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : DoubleNode.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="DoubleNode.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     The definition of a doubly-linked node
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

namespace LockFree.Net.Objects
{
    public class DoubleNode<T>
    {
        public DoubleNode<T> Next;

        public DoubleNode<T> Previous;

        public T Value;

        public DoubleNode()
        {
            Next = null;
            Previous = null;
            Value = default;
        }

        public DoubleNode(T value)
        {
            Next = null;
            Previous = null;
            Value = value;
        }

        public DoubleNode(DoubleNode<T> node, NodeCreationSettings nextNodeCreationSettings = NodeCreationSettings.PointToNull, NodeCreationSettings previousNodeCreationSettings = NodeCreationSettings.PointToNull)
        {
            Value = node.Value;

            switch (nextNodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Next = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Next = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Next = node.Next;
                    node.Next = null;
                    break;
            }

            switch (previousNodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Previous = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Previous = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Previous = node.Previous;
                    node.Previous = null;
                    break;
            }
        }

        public DoubleNode(T value, DoubleNode<T> node, NodeCreationSettings nextNodeCreationSettings = NodeCreationSettings.PointToNull, NodeCreationSettings previousNodeCreationSettings = NodeCreationSettings.PointToNull)
        {
            Value = value;

            switch (nextNodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Next = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Next = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Next = node.Next;
                    node.Next = null;
                    break;
            }

            switch (previousNodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Previous = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Previous = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Previous = node.Previous;
                    node.Previous = null;
                    break;
            }
        }
    }
}
