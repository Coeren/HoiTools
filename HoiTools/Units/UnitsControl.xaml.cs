using PersistentLayer;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Units
{
    /// <summary>
    /// Interaction logic for UnitsControl.xaml
    /// </summary>
    public partial class UnitsControl : UserControl
    {
        private UnitsMVVM _mvvm = new UnitsMVVM();

        public UnitsMVVM Mvvm { get => _mvvm; }

        public UnitsControl()
        {
            InitializeComponent();

            DataContext = _mvvm;
            Loaded += (s, e) => { Window.GetWindow(this).Closed += Cleanup; };
        }

        private void Cleanup(object sender, EventArgs e)
        {
            _mvvm.Cleanup();
            Window.GetWindow(this).Closed -= Cleanup;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mvvm.AddToComparison(((Button) sender).Tag as IModel);
        }

        internal void Compare()
        {
            _mvvm.Compare();
        }

        private void Button_CloseCompare(object sender, RoutedEventArgs e)
        {
            _mvvm.CloseCompare();
        }
    }
}
