using System.Windows;
using Trader.Controls;

namespace Trader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainControl mainControl = new MainControl();
            this.Content = mainControl;
        }
    }
}
