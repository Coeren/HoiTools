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
            string r = ConfigurationManager.AppSettings["RootFolder"];
            string r2 = Core.Instance.RootFolder;

            _commonSettings.Apply();
        }
    }
}
