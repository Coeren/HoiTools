using PersistentLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIControls;

namespace HoiTools
{
    public class Tech
    {
        public string Name { get => _tech.Name; }
        public string Desc { get => _tech.Desc; }
        public string Cost { get => _tech.Cost.ToString(); }
        public string Duration { get => _tech.Duration.ToString(); }
        public string Effects { get => _effects; }
        public IReadOnlyCollection<Tech> Children { get => _children; }

        internal Tech(ITheoryTech t)
        {
            _tech = t;
            _effects = "-";
        }
        internal Tech(IAppliedTech t)
        {
            _tech = t;
            foreach (ITechEffect effect in t.Effects)
                _effects += effect.Type + " applies on " + effect.Applies + " for " + effect.Value + "\n";
        }
        internal void AddChild(Tech t)
        {
            _children.Add(t);
        }

        readonly private ITechnology _tech;
        readonly private string _effects = "";
        private List<Tech> _children = new List<Tech>();
    }

    public class TechMVVM : Mvvm
    {
        public TechMVVM()
        {
            Core.DataChanged += Core_DataChanged;
        }

        public override void Cleanup()
        {
            Core.DataChanged -= Core_DataChanged;
        }

        public IReadOnlyCollection<ITechArea> Areas { get => (IReadOnlyCollection<ITechArea>) Core.Tech.Values; }
        public ITechArea SelectedArea
        {
            get { return _selectedArea; }
            set { if (_selectedArea != value) { _selectedArea = value; OnPropertyChanged("Techs"); } }
        }
        public IReadOnlyCollection<Tech> Techs
        {
            get
            {
                List<Tech> tree = new List<Tech>();
                if (_selectedArea != null)
                {
                    ITheoryTech theo = _selectedArea.Root;
                    while (theo != null)
                    {
                        Tech theoPres = new Tech(theo);
                        tree.Add(theoPres);
                        foreach (IAppliedTech item in theo.Children)
                            if (item.Area == theo.Area) theoPres.AddChild(new Tech(item));

                        theo = theo.Next;
                    }
                }

                return tree;
            }
        }

        private void Core_DataChanged(object sender, string e)
        {
            if (e == "All") OnPropertyChanged("Areas");
        }

        private ITechArea _selectedArea;
    }
}
