using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using task1.Models;

namespace task1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Stack<string> _stack = new();

    private bool CanPopItem => _stack.Length != 0;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PopItemCommand))]
    private string? _newItemContent;
    
    [ObservableProperty] 
    private string _tailElementString = "Tail: " + null;
    [ObservableProperty] 
    private string _currentLengthString = "0 elements";
    [ObservableProperty] 
    private string _isEmptyString = "Is empty: " + true;

    private void UpdateStrings()
    {
        TailElementString = "Tail: " + _stack.TailElement;
        CurrentLengthString = _stack.Length + " elements";
        IsEmptyString = "Is empty: " + _stack.IsEmpty;
    }
    
    [RelayCommand(CanExecute = nameof(CanPopItem))]
    private void PopItem()
    {
        NewItemContent = _stack.Pop(); 
        
        UpdateStrings();
    }
    
    [RelayCommand]
    private void AppendItem()
    {
        _stack.Append(NewItemContent);
        NewItemContent = "";
        
        UpdateStrings();
    }
    
    [RelayCommand]
    private void ClearItems()
    {
        _stack.Clear();
        NewItemContent = "";
        
        UpdateStrings();
    }
}