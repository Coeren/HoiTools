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
        public static readonly DependencyProperty PromptProperty = DependencyProperty.Register("Prompt", typeof(string), typeof(PathSelector));

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

        public string Prompt
        {
            get { return (string)GetValue(PromptProperty); }
            set { SetValue(PromptProperty, value); }
        }

        public PathSelector()
        {
            InitializeComponent();
        }

        private string _hint = "Enter path or select on file tree";

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Path == null || Path == "")
            {
                Path = _hint;
            }
        }

        private void path_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Path == _hint)
            {
                Path = "";
            }
        }

        private void path_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox)?.Text == "")
            {
                Path = _hint;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = SelectFolder;
            if (Path != "")
            {
                dlg.InitialDirectory = Path;
            }
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dlg.FileName;
            }
        }
    }
}
