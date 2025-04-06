using System.Collections.ObjectModel;
using task2.Models;

namespace task2.ViewModels;

public class FileSystemViewModel(FileSystemNode node, FileSystemViewModel? parent) : ViewModelBase
{
    private readonly FileSystemNode _node = node;
    
    public bool IsDirectory => _node.Type == FileSystemNode.NodeType.Directory;
    public bool IsFile => _node.Type == FileSystemNode.NodeType.File;
    
    public FileSystemViewModel? Parent = parent;
    
    public ObservableCollection<FileSystemViewModel>? Children
    {
        get
        {
            if (_node is not Directory directory) return null;

            var children = new ObservableCollection<FileSystemViewModel>();

            foreach (var child in directory.Children)
            {
                children.Add(new FileSystemViewModel(child, this));
            }

            return children;
        }
    }

    public string Name => _node.Name;
    public long Size => _node.Size;

    private void Update()
    {
        OnPropertyChanged(nameof(Children));
        SizeUpdate();
    }

    private void SizeUpdate()
    {
        OnPropertyChanged(nameof(Size));
        Parent?.OnPropertyChanged(nameof(Size));
    }

    public void Add(FileSystemNode item)
    {
        (_node as Directory)?.Add(item);
        Update();
    }

    public void Remove(FileSystemViewModel selectedNode)
    {
        (_node as Directory)?.Remove(selectedNode._node);
        Update();
    }

    public void Move(FileSystemViewModel selectedNodeToCopyMove)
    {
        if (selectedNodeToCopyMove._node is not Directory directory) return;
        
        FileSystemNode.Move(_node, directory);
        selectedNodeToCopyMove.Update();
        
        Parent?.Update();
        Parent = selectedNodeToCopyMove;
    }

    public void Copy(FileSystemViewModel selectedNodeToCopyMove)
    {
        if (selectedNodeToCopyMove._node is not Directory directory) return;
        
        FileSystemNode.Copy(_node, directory);
        selectedNodeToCopyMove.Update();
    }
}