using PersistentLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Units
{
    /// <summary>
    /// Interaction logic for UnitsControl.xaml
    /// </summary>
    public partial class UnitsControl : UserControl
    {
        public class UnitsMVVM : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public UnitsMVVM()
            {
                Core.DataChanged += Core_DataChanged;
            }

            public IReadOnlyCollection<UnitTypes> Types { get { return Core.Models.Types; } }

            public UnitTypes SelectedType
            {
                get { return _selectedType; }
                set { if (_selectedType != value) { _selectedType = value; OnPropertyChanged("Models"); } }
            }

            public ObservableCollection<IModel> Models { get { return new ObservableCollection<IModel>(Core.Models.ModelType(_selectedType).Models); } }

            public IModel SelectedModel
            {
                get { return _selectedModel; }
                set
                {
                    if (_selectedModel != value)
                    {
                        _selectedModel = value;
                        OnPropertyChanged("SelectedModel");
                    }
                }
            }

            internal void Cleanup()
            {
                Core.DataChanged -= Core_DataChanged;
            }

            private void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            private void Core_DataChanged(object sender, string e)
            {
                if (e == "CurrentCountry")
                {
                    OnPropertyChanged("Models");
                }
            }

            private UnitTypes _selectedType;
            private IModel _selectedModel;
        }

        private void Cleanup(object sender, EventArgs e)
        {
            _mvvm.Cleanup();
            Window.GetWindow(this).Closed -= Cleanup;
        }

        private UnitsMVVM _mvvm = new UnitsMVVM();

        public UnitsControl()
        {
            InitializeComponent();

            DataContext = _mvvm;
            Loaded += (s, e) => { Window.GetWindow(this).Closed += Cleanup; };
        }
    }
}
