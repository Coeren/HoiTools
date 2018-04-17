using PersistentLayer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Units
{
    public class UnitsMVVM : INotifyPropertyChanged
    {
        public class CompRow
        {
            private string spec;
            private string first;
            private string second;
            private bool highlight;

            public string Spec { get => spec; }
            public string First { get => first; }
            public string Second { get => second; }
            public bool Highlight { get => highlight; }

            public CompRow(string key, string v1, string v2, bool h)
            {
                this.spec = key;
                this.first = v1;
                this.second = v2;
                this.highlight = h;
            }
        }

        public UnitsMVVM()
        {
            Core.DataChanged += Core_DataChanged;
        }

        public IReadOnlyCollection<UnitTypeName> Types { get => Core.UnitTypes.Types; }// new ObservableCollection<UnitTypeName>(Core.UnitTypes.Types); } }

        public UnitTypeName SelectedType
        {
            get { return _selectedType; }
            set { if (_selectedType != value) { _selectedType = value; OnPropertyChanged("Models"); } }
        }

        public ObservableCollection<IModel> Models { get { return new ObservableCollection<IModel>(Core.UnitTypes.UnitType(_selectedType).Models); } }

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
            if (e == "All")
            {
                SelectedModel = null;
                OnPropertyChanged("Types");
                OnPropertyChanged("Models");
            }
            else if (e == "CurrentCountry")
            {
                OnPropertyChanged("Models");
            }
        }

        public ObservableCollection<IModel> ComparingModels { get => _comparingModels; }
        public bool CompareEnabled { get => _comparingModels.Count == 2 && !_comparingMode; }
        public bool ComparingMode { get => _comparingMode; }

        public void AddToComparison(IModel model)
        {
            bool btnUpdate = _comparingModels.Count == 1;

            if (_comparingModels.Count == 2) _comparingModels.RemoveAt(0);
            _comparingModels.Add(model);

            OnPropertyChanged("ComparingModels");
            if (btnUpdate) OnPropertyChanged("CompareEnabled");
        }

        public ObservableCollection<CompRow> ComparisonTable { get => new ObservableCollection<CompRow>(_comparison.Values); }
        public string FirstComparingModel { get => _comparingModels.Count >= 1 ? _comparingModels[0].Name : ""; }
        public string SecondComparingModel { get => _comparingModels.Count >= 1 ? _comparingModels[1].Name : ""; }

        internal void Compare()
        {
            IModel modelOne = _comparingModels[0], modelTwo = _comparingModels[1];

            foreach (var spec in modelOne.Specifications)
            {
                if (modelTwo.Specifications.ContainsKey(spec.Key))
                    _comparison.Add(spec.Key, new CompRow(spec.Key, spec.Value.ToString(), modelTwo.Specifications[spec.Key].ToString(), spec.Value != modelTwo.Specifications[spec.Key]));
                else
                    _comparison.Add(spec.Key, new CompRow(spec.Key, spec.Value.ToString(), "-", true));
            }

            foreach (var spec in modelTwo.Specifications)
                if (!_comparison.ContainsKey(spec.Key)) _comparison.Add(spec.Key, new CompRow(spec.Key, "-", spec.Value.ToString(), true));

            _comparingMode = true;
            OnPropertyChanged("CompareEnabled");
            OnPropertyChanged("ComparingMode");
            OnPropertyChanged("ComparisonTable");
            OnPropertyChanged("FirstComparingModel");
            OnPropertyChanged("SecondComparingModel");
        }

        internal void CloseCompare()
        {
            _comparison.Clear();

            _comparingMode = false;
            OnPropertyChanged("ComparingMode");
            OnPropertyChanged("CompareEnabled");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private UnitTypeName _selectedType;
        private IModel _selectedModel;
        private ObservableCollection<IModel> _comparingModels = new ObservableCollection<IModel>();
        private bool _comparingMode = false;
        private Dictionary<string, CompRow> _comparison = new Dictionary<string, CompRow>();
    }
}
