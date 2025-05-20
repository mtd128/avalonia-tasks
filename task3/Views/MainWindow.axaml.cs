using Avalonia.Controls;
using MainWindowViewModel = task3.ViewModels.MainWindowViewModel;

namespace task3.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                if (this.DataContext is MainWindowViewModel vm)
                {
                    vm.Cleanup();
                }
            };
        }
    }
}