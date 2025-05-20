using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MainWindow = task3.Views.MainWindow;
using MainWindowViewModel = task3.ViewModels.MainWindowViewModel;

namespace task2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindowViewModel = new MainWindowViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}