using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using task2.Models;

namespace task2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private long _counter;

    [ObservableProperty]
    private ObservableCollection<FileSystemViewModel> _root = [new(new Directory("root", []), null)];

    [ObservableProperty]
    private FileSystemViewModel? _selectedNode;
    
    [ObservableProperty]
    private FileSystemViewModel? _selectedNodeToCopyMove;

    [ObservableProperty]
    private bool _moveCopyNotStarted = true;

    [ObservableProperty]
    private bool _moveCopyStarted;
    
    [RelayCommand]
    private void AddFile()
    {
        SelectedNode?.Add(new File("file" + _counter, _counter));
        
        if (SelectedNode is not null)
        {
            ++_counter;
        }
    }
    
    [RelayCommand]
    private void AddDirectory()
    {
        SelectedNode?.Add(new Directory("directory" + _counter, []));
        
        if (SelectedNode is not null)
        {
            ++_counter;
        }
    }

    [RelayCommand]
    private void DeleteNode()
    {
        SelectedNode?.Parent?.Remove(SelectedNode);
    }

    [RelayCommand]
    private void StartCopyMove()
    {        
        if (SelectedNode is null) return;
        
        SelectedNodeToCopyMove = SelectedNode;
        MoveCopyNotStarted = SelectedNodeToCopyMove is null;
        MoveCopyStarted = SelectedNodeToCopyMove is not null;
    }

    [RelayCommand]
    private void CancelCopyMove()
    {
        if (MoveCopyNotStarted) return;
        ClearSelectedNodeToCopyMove();
    }
    
    [RelayCommand]
    private void CopyTo()
    {
        if (SelectedNode is null) return;
        SelectedNodeToCopyMove?.Copy(SelectedNode);
        ClearSelectedNodeToCopyMove();
    }
    
    [RelayCommand]
    private void MoveTo()
    {
        if (SelectedNode is null) return;
        SelectedNodeToCopyMove?.Move(SelectedNode);
        ClearSelectedNodeToCopyMove();
    }

    private void ClearSelectedNodeToCopyMove()
    {
        SelectedNodeToCopyMove = null;
        MoveCopyNotStarted = SelectedNodeToCopyMove is null;
        MoveCopyStarted = SelectedNodeToCopyMove is not null;
    }
}