// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : Node.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="Node.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     The definition of a singly-linked node
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

namespace LockFree.Net.Objects
{
    public class Node<T>
    {
        public Node<T> Next;

        public T Value;

        public Node()
        {
            Next = null;
            Value = default;
        }

        public Node(T value)
        {
            Next = null;
            Value = value;
        }

        public Node(Node<T> node, NodeCreationSettings nodeCreationSettings = NodeCreationSettings.PointToNull)
        {
            Value = node.Value;

            switch (nodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Next = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Next = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Next = node.Next;
                    node.Next = this;
                    break;
            }
        }

        public Node(T value, Node<T> node, NodeCreationSettings nodeCreationSettings = NodeCreationSettings.PointToNull)
        {
            Value = value;

            switch (nodeCreationSettings)
            {
                case NodeCreationSettings.PointToNull:
                    Next = null;
                    break;
                case NodeCreationSettings.PointNewNodeToOldNode:
                    Next = node;
                    break;
                case NodeCreationSettings.PointOldNodeToNewNode:
                    Next = node.Next;
                    node.Next = this;
                    break;
            }
        }
    }
}
