using Avalonia.Controls;
using ConveyorViewModel = task3.ViewModels.ConveyorViewModel;

namespace task3.Views
{
    public partial class ConveyorView : UserControl
    {
        public ConveyorView()
        {
            InitializeComponent();
        }

        public ConveyorView(string ConveyorName, double CanvasX, double CanvasY) : this()
        {
            DataContext = new ConveyorViewModel(null, null, null, 0, 0, _ => {});
        }
    }
}