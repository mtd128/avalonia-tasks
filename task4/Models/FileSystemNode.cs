using System;
using System.Linq;

namespace task2.Models;

public abstract class FileSystemNode(string name, Directory? parent)
{
    public enum NodeType
    {
        File,
        Directory
    }
    
    private const string Separator = "/";

    public string Name { get; private set; } = name;
    public Directory? Parent { get; internal set; } = parent;
    public abstract NodeType Type { get; }
    public abstract long Size { get; }

    public string Path
    {
        get
        {
            var path = Separator + Name;
            var parent = Parent;

            while (parent != null)
            {
                path = Separator + parent.Name + path;
                parent = parent.Parent;
            }

            return path;
        }
    }

    private static bool IsChildOf(Directory parent, Directory target)
    {
        return parent.Children.Contains(target)
               || parent.Children.Any(child => child is Directory dir && IsChildOf(dir, target));
    }

    private static bool DirectoryParentCheck(FileSystemNode what, Directory? to)
    {
        if (what is not Directory node) return true;
        if (to is null) return true;
        if (what == to) return false;
        return !IsChildOf(node, to);
    }
    
    public static FileSystemNode? Copy(FileSystemNode node, Directory? to)
    {
        if (!DirectoryParentCheck(node, to)) return null;
        
        switch (node.Type)
        {
            case NodeType.File:
            {
                var file = new File(node.Name, node.Size, to);
                
                while (to?.MutableChildren.Any(child => child.Name == file.Name) ?? false)
                {
                    file.Name += " - copy";
                }
                
                to?.MutableChildren.Add(file);
                
                return file;
            }
            case NodeType.Directory:
            {
                var directory = new Directory(node.Name, [], to);
                
                while (to?.MutableChildren.Any(child => child.Name == directory.Name) ?? false)
                {
                    directory.Name += " - copy";
                }
                
                to?.MutableChildren.Add(directory);
                
                (node as Directory)?.MutableChildren.ForEach(child => Copy(child, directory));
                
                return directory;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void Move(FileSystemNode node, Directory? to)
    {
        if (!DirectoryParentCheck(node, to)) return;
        
        node.Parent?.MutableChildren.Remove(node);
        node.Parent = to;

        while (to?.MutableChildren.Any(child => child.Name == node.Name) ?? false)
        {
            node.Name += " - copy";
        }
        
        to?.MutableChildren.Add(node);
    }
}