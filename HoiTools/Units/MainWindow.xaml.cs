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

            App.Instance.Log.PropertyChanged += Log_PropertyChanged;

            Trace.WriteLine("!!!!!!!!!!!!!! 1111 !!!!!!!!!!!!!!!");
        }

        private void Log_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Trace")
            {
                Log = (sender as StringTextListener).Trace;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _commonSettings.Apply();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("!!!!!!!!!!!!!! 2222 !!!!!!!!!!!!!!!");

            try
            {
                Core.Instance.Test();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            Trace.WriteLine("!!!!!!!!!!!!!! 3333 !!!!!!!!!!!!!!!");
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBox.ScrollToEnd();
        }
    }
}
