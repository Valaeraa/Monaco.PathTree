﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Monaco.PathTree
{
    public interface IPathTreeNode<T> 
    {
        IPathTreeNode<T> Parent { get; set; }
        T Value { get; set; }
        string Name { get; }
        string PathKey { get; }

        void AddChild(string name, T value);
        void RemoveChild(string name);
        bool ContainsChild(string name);
        bool TryGetChild(string name, out IPathTreeNode<T> node);
        void AttachChild(IPathTreeNode<T> node);
        IPathTreeNode<T> DetachChild(string name);

        IEnumerable<IPathTreeNode<T>> Children();
        IEnumerable<IPathTreeNode<T>> Ancestors();
        IEnumerable<IPathTreeNode<T>> SelfAndDescendantsDepthFirst();
        IEnumerable<IPathTreeNode<T>> SelfAndDescendantsBreadthFirst();
        IEnumerable<IPathTreeNode<T>> DescendantsDepthFirst();
        IEnumerable<IPathTreeNode<T>> DescendantsBreadthFirst();
    }
}