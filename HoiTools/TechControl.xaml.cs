using Common;
using System;
using System.Collections.Generic;
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

using UIControls;

namespace HoiTools
{
    /// <summary>
    /// Interaction logic for TechControl.xaml
    /// </summary>
    public partial class TechControl : CustomControlBase<TechMVVM>
    {
        public TechControl()
        {
            InitializeComponent();
        }
    }
}
