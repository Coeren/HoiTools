using PersistentLayer;
using System;
using System.Collections.Generic;
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
        public UnitsControl()
        {
            InitializeComponent();

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

        private object ModelTypeContent(IModelType modelType)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            ListBox lb = new ListBox { Name = "lb" };
            Grid grid = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, RowDefinitions = { new RowDefinition() } };
            Label label = new Label { Content = "Name" };
            TextBox tb = new TextBox { IsEnabled = false };
            Binding bind = new Binding("SelectedValue.Tag.Name");

            foreach (int id in modelType.Ids)
            {
                lb.Items.Add(new Label { Content = modelType.Model(id).Name, Tag = modelType.Model(id) });
            }
            sp.Children.Add(lb);

            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 0);

            bind.Source = lb;
            bind.Mode = BindingMode.OneWay;
            tb.SetBinding(TextBox.TextProperty, bind);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, 0);

            grid.Children.Add(label);
            grid.Children.Add(tb);

            sp.Children.Add(grid);

            return sp;
        }
    }
}
