using PersistentLayer;
using System.Configuration;
using System.Windows;

namespace Units
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _commonSettings.Apply();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Core.Instance.Test();
        }
    }
}
