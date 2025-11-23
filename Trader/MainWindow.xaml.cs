using System.Windows;
using System.Windows.Forms;
using Trader.Controls;

namespace Trader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = (WindowState)FormWindowState.Maximized;
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainControl mainControl = new MainControl();
            this.Content = mainControl;
        }
    }
}
