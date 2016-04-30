using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Units
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            try
            {
                _commonSettings.Apply();

                DialogResult = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                Cursor = null;
            }
        }
    }
}
