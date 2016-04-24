using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersistentLayer
{
    /// <summary>
    /// Interaction logic for CommonSettings.xaml
    /// </summary>
    public partial class CommonSettings : UserControl
    {
        public CommonSettings()
        {
            InitializeComponent();

            _rootPath.Path = Core.Instance.RootFolder;
        }

        public void Apply()
        {
            Core.Instance.RootFolder = _rootPath.Path;
        }
    }
}
