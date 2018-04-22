using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Common;

namespace UIControls
{
    public abstract class Mvvm : INotifyPropertyChanged
    {
        abstract public void Cleanup();
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class CustomControlBase<TMvvm> : UserControl where TMvvm : Mvvm, new()
    {
        public CustomControlBase()
        {
            DataContext = _mvvm;
            if (!Utils.IsDesigning) // Workaround to prevent XAML designer gets broken
                Loaded += (s, e) => { Window.GetWindow(this).Closed += Cleanup; };
        }

        private void Cleanup(object sender, EventArgs e)
        {
            _mvvm.Cleanup();
            Window.GetWindow(this).Closed -= Cleanup;
        }

        protected TMvvm _mvvm = new TMvvm();
    }
}
