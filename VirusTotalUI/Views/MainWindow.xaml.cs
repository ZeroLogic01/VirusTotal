using Prism.Ioc;
using Prism.Regions;
using System.Windows;
using VirusTotalUI.ViewModels;

namespace VirusTotalUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IContainerExtension containerExtension)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, containerExtension);

        }
    }
}
