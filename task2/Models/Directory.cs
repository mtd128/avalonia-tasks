using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace task2.Models;

public class Directory : FileSystemNode
{
    internal readonly List<FileSystemNode> MutableChildren;

    public ReadOnlyCollection<FileSystemNode> Children => MutableChildren.AsReadOnly();

    public override NodeType Type => NodeType.Directory;

    public override long Size
    {
        get
        {
            var size = 0L;
            var stack = new Stack<FileSystemNode>();
            
            MutableChildren.ForEach(child => stack.Push(child));

            while (stack.Count != 0)
            {
                var item = stack.Pop();

                switch (item.Type)
                {
                    case NodeType.Directory:
                    {
                        (item as Directory)?.MutableChildren.ForEach(child => stack.Push(child));
                        break;
                    }
                    case NodeType.File:
                    {
                        size += (item as File)?.Size ?? 0;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return size;
        }
    }

    public Directory(string name, List<FileSystemNode> children, Directory? parent = null) : base(name, parent)
    {
        MutableChildren = children.Select(child =>
        {
            child.Parent = this;
            return child;
        }).ToList();
    }

    public void Add(FileSystemNode item)
    {
        item.Parent = this;
        MutableChildren.Add(item);
    }
    
    public void Remove(FileSystemNode item)
    {
        MutableChildren.Remove(item);
    }
}