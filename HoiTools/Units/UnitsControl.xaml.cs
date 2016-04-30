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
    public class UnitsMVVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public UnitsMVVM() {}

        public IReadOnlyCollection<UnitTypes> Types { get { return Core.Models.Types; } }

        public UnitTypes SelectedType
        {
            get { return _selectedType; }
            set { if (_selectedType != value) { _selectedType = value; OnPropertyChanged("Models"); } }
        }

        public IReadOnlyCollection<IModel> Models { get { return Core.Models.ModelType(_selectedType).Models; } }

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

//        public ReadOnlyDictionary<string, int> Specifications { get { return _selectedModel.Specifications; } }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private UnitTypes _selectedType;
        private IModel _selectedModel;
    }

    /// <summary>
    /// Interaction logic for UnitsControl.xaml
    /// </summary>
    public partial class UnitsControl : UserControl
    {
        private UnitsMVVM _mvvm = new UnitsMVVM();

        public UnitsControl()
        {
            InitializeComponent();

            DataContext = _mvvm;

            CreateContents(false);
        }

        internal void CreateContents(bool recreate)
        {
            //if (recreate)
            //{
            //    tabControl.Items.Clear();
            //}

            //foreach (UnitTypes type in Core.Models.Types)
            //{
            //    tabControl.Items.Add(new TabItem
            //    {
            //        Header = new TextBlock { Text = Enum.GetName(typeof(UnitTypes), type) },
            //        Content = ModelTypeContent(Core.Models.ModelType(type))
            //    });
            //}
        }

        //private object ModelTypeContent(IModelType modelType)
        //{
        //    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
        //    ListBox lb = new ListBox { Name = "lb" };
        //    Grid grid = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, RowDefinitions = { new RowDefinition() } };
        //    Label label = new Label { Content = "Name" };
        //    TextBox tb = new TextBox { IsEnabled = false };
        //    Binding bind = new Binding("SelectedValue.Tag.Name");

        //    foreach (int id in modelType.Ids)
        //    {
        //        lb.Items.Add(new Label { Content = modelType.Model(id).Name, Tag = modelType.Model(id) });
        //    }
        //    sp.Children.Add(lb);

        //    Grid.SetColumn(label, 0);
        //    Grid.SetRow(label, 0);

        //    bind.Source = lb;
        //    bind.Mode = BindingMode.OneWay;
        //    tb.SetBinding(TextBox.TextProperty, bind);
        //    Grid.SetColumn(tb, 1);
        //    Grid.SetRow(tb, 0);

        //    grid.Children.Add(label);
        //    grid.Children.Add(tb);

        //    sp.Children.Add(grid);

        //    return sp;
        //}
    }
}
