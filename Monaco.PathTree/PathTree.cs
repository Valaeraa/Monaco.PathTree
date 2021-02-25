﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monaco.PathTree
{
    public class PathTree<T> : IPathTree<T>
    {
        private IPathTreeNode<T> _root;
        public IPathTreeNode<T> Root
        {
            get => _root;
            set
            {
                _root = value;
                if (_root is object)
                    _root.Parent = null;
            }
        }

        public char[] PathSeparators { get; set; } = new char[] { '\\', '/' };

        public PathTree() { }

        public PathTree(IPathTreeNode<T> root)
        {
            Root = root;
        }

        public PathTree(string rootName, T root)
        {
            var rootNode = new PathTreeNode<T>(rootName, root);
            Root = rootNode;
        }

        /// <summary>
        /// Adds the item as the specified path if the parent exists
        /// </summary>
        /// <param name="path">The full path associated with the item</param>
        /// <param name="item">The item to be added</param>
        public void AddAsPath(string path, T item)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(AddAsPath)}: parameter '{nameof(path)}' was null or empty");

            var parent = ResolveParent(path);

            if (parent is null)
                throw new KeyNotFoundException($"{nameof(TryGetValue)}: could not find {nameof(path)} '{path}'");

            var nodeName = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries).Last();
            parent.AddChild(nodeName, item);
        }

        /// <summary>
        /// Tries to get the value stored at the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns>True if successful, false if failed</returns>
        public bool TryGetValue(string path, out T value)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(TryGetValue)}: parameter '{nameof(path)}' was null or empty");

            var node = ResolveNode(path);
            
            if (node is object)
            {
                value = node.Value;
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Tries to get the value of a specific type stored at the specified path
        /// </summary>
        /// <typeparam name="U">Type of the value</typeparam>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns>True if successful, false if failed</returns>
        public bool TryGetValue<U>(string path, out U value) where U : T
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(TryGetValue)}: parameter '{nameof(path)}' was null or empty");

            var node = ResolveNode(path);

            if (node is object)
            {
                value = (U)node.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get the node contained at the specified location
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool TryGetNode(string path, out IPathTreeNode<T> node)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(TryGetNode)}: parameter '{nameof(path)}' was null or empty");

            node = ResolveNode(path);

            return node is object;
        }

        public bool TryGetNode<TDerived>(string path, out IPathTreeNode<TDerived> node) where TDerived : T
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(TryGetNode)}: parameter '{nameof(path)}' was null or empty");

            var resolvedNode = ResolveNode(path);

            if (resolvedNode is object)
            {
                node = (IPathTreeNode<TDerived>) resolvedNode;
                return true;
            }

            node = default;
            return false;
        }

        /// <summary>
        /// Removes the node at the specified location
        /// </summary>
        /// <param name="path"></param>
        public void RemoveNode(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
                throw new ArgumentException();

            var removeNode = ResolveNode(path);

            if (removeNode is null)
                throw new KeyNotFoundException($"RemoveNode was unable to locate {nameof(path)}");

            if (removeNode.Parent is null)
                throw new NullReferenceException();

            removeNode.Parent.RemoveChild(removeNode.Name);
        }

        private IPathTreeNode<T> ResolveNode(string absolutePath)
        {
            var nodeNames = absolutePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            return Resolve(nodeNames);
        }

        private IPathTreeNode<T> ResolveParent(string absolutePath)
        {
            var nodeNames = absolutePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            var parentNodeNames = nodeNames.Take(nodeNames.Length - 1).ToList();

            return Resolve(parentNodeNames);
        }

        private IPathTreeNode<T> Resolve(IList<string> nodeNames)
        {
            if (nodeNames.Count == 0)
                return null;

            if (nodeNames.First() != Root.Name)
                return null;

            IPathTreeNode<T> nodeVisitor = Root;
            IPathTreeNode<T> nextNode;

            foreach (var name in nodeNames.Skip(1))
            {
                if (!nodeVisitor.TryGetChild(name, out nextNode))
                {
                    return null;
                }
                nodeVisitor = nextNode;
            }

            return nodeVisitor;
        }

        /// <summary>
        /// Traverses the tree to count the number of items contained within
        /// </summary>
        /// <returns>Number of items</returns>
        public int Count()
        {
            int nodeCount = 0;
            var nodeStack = new Stack<IPathTreeNode<T>>();

            nodeStack.Push(Root);

            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();
                nodeCount++;

                foreach (var child in node.Children)
                    nodeStack.Push(child);
            }

            return nodeCount;
        }
    }
}
