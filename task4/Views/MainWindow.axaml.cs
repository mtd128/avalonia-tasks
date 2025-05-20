using Avalonia.Controls;
using Avalonia.Interactivity;
using task2.ViewModels;

namespace task2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnTypeSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectTypeCommand.Execute(null);
        }
    }

    private void OnMethodSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectMethodCommand.Execute(null);
        }
    }
}