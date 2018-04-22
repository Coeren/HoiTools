using Common;
using PersistentLayer;
using System;
using System.Windows;
using System.Windows.Controls;
using UIControls;

namespace HoiTools
{
    /// <summary>
    /// Interaction logic for UnitsControl.xaml
    /// </summary>
    public partial class UnitsControl : CustomControlBase<UnitsMVVM>
    {
        public UnitsControl()
        {
            InitializeComponent();
        }

        public UnitsMVVM Mvvm { get => _mvvm; }

        internal void Compare()
        {
            _mvvm.Compare();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mvvm.AddToComparison(((Button) sender).Tag as IModel);
        }
        private void Button_CloseCompare(object sender, RoutedEventArgs e)
        {
            _mvvm.CloseCompare();
        }
    }
}
