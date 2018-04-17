using Common;
using PersistentLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Units
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class MVVM : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Log { get { return App.Log.Trace; } }
            public IEnumerable<string> Countries { get { return Core.Countries.Values; } }
            public string CurrentCountry
            {
                get { return Core.CurrentCountry; }
                set { Core.CurrentCountry = value; }
            }
            public bool DelayTrace
            {
                set
                {
                    if (_delayTrace != value)
                    {
                        _delayTrace = value;
                        if (!_delayTrace)
                        {
                            OnPropertyChanged("Log");
                        }
                    }
                }
            }

            internal MVVM()
            {
                Core.DataChanged += Core_DataChanged;
                App.Log.TraceAdded += OnTraceAdded;
                OnPropertyChanged("Log");
            }

            internal void Cleanup()
            {
                Core.DataChanged -= Core_DataChanged;
                App.Log.TraceAdded -= OnTraceAdded;
            }

            private void Core_DataChanged(object sender, string e)
            {
                OnPropertyChanged(e);
            }

            private void OnTraceAdded(object sender, string trace)
            {
                if (!_delayTrace)
                {
                    OnPropertyChanged("Log");
                }
            }

            private void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            private bool _delayTrace = false;
        }

        private MVVM _mvvm = new MVVM();

        public MainWindow()
        {
            InitializeComponent();
            Core.Prepare();

            DataContext = _mvvm;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _mvvm.Cleanup();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings dlg = new Settings();
            dlg.Owner = this;

            _mvvm.DelayTrace = true;
            dlg.ShowDialog();
            _mvvm.DelayTrace = false;
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBox.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            unitsControl.Compare();
        }
    }
}
