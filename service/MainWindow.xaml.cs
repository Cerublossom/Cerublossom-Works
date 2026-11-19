using System.Windows;
using service.ViewModels;

namespace service
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(new MainViewModel())
        {
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
