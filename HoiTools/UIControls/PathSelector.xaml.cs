using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for PathSelector.xaml
    /// </summary>
    public partial class PathSelector : UserControl
    {
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(PathSelector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty SelectFolderProperty = DependencyProperty.Register("SelectFolder", typeof(bool), typeof(PathSelector));
        public static readonly DependencyProperty HintProperty = DependencyProperty.Register("Hint", typeof(string), typeof(PathSelector));

        public bool SelectFolder
        {
            get { return (bool)GetValue(SelectFolderProperty); }
            set { SetValue(SelectFolderProperty, value); }
        }
        public string Path
        {
            get { return (string) GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public PathSelector()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Hint == null || Hint == "")
            {
                Hint = "Enter path or select on file tree";
            }
            Path = Hint;
        }

        private void path_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Path == Hint)
            {
                Path = "";
            }
        }

        private void path_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox)?.Text == "")
            {
                Path = Hint;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = SelectFolder;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dlg.FileName;
            }
        }
    }
}
