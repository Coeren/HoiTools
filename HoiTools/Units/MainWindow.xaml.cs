using Common;
using PersistentLayer;
using System;
using System.Diagnostics;
using System.Windows;

namespace Units
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(string), typeof(MainWindow));
        public string Log
        {
            get { return (string) GetValue(LogProperty); }
            private set { SetValue(LogProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            App.Log.TraceAdded += OnTraceAdded;
            Log = App.Log.Trace;
        }

        private void OnTraceAdded(string trace)
        {
            Log += trace;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Settings dlg = new Settings();
            dlg.Owner = this;
            dlg.ShowDialog();

            unitsControl.CreateContents(true);
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBox.ScrollToEnd();
        }
    }
}
