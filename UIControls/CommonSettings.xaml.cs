using System.Windows.Controls;

using PersistentLayer;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for CommonSettings.xaml
    /// </summary>
    public partial class CommonSettings : UserControl
    {
        public CommonSettings()
        {
            InitializeComponent();

            _rootPath.Path = Core.RootFolder;
        }

        public void Apply()
        {
            Core.RootFolder = _rootPath.Path;
        }
    }
}
