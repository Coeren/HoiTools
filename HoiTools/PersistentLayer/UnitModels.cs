using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Common;

namespace PersistentLayer
{
    public enum UnitTypeName
    {
        Infantry = 0,
        Cavalry,
        Motorized,
        Mechanized,
        Panzer,
        Paratrooper,
        Marine,
        Bergsjaeger,
        Militia,
        Fighter,
        Strategic_Bomber,
        Tactical_Bomber,
        Dive_Bomber,
        Naval_Bomber,
        Torpedo_Plane,
        Transport_Plane,
        Flying_Bomb,
        Flying_Rocket,
        Battleship,
        Cruiser,
        Destroyer,
        Carrier,
        Submarine,
        Transport,
        // brigades
        AntiAir,
        AntiTank,
        Artillery,
        Engineer,
        // ???
        Night_Fighter
    }

    public interface IModel
    {
        string Name { get; }
        ReadOnlyDictionary<string, double> Specifications { get; }
    }

    public interface IUnitType
    {
        IReadOnlyCollection<IModel> Models { get; }
    }

    public interface IUnitTypes
    {
        IReadOnlyCollection<UnitTypeName> Types { get; }
        IUnitType UnitType(UnitTypeName type);
    }

    internal class Model : IModel, IConsistencyChecker
    {
        public override string ToString() { return Name; }

        public string Name
        {
            get
            {
                try
                {
                    string tag = Core.CurrentCountryTag;
                    return _namesByCountry.ContainsKey(tag) ? _namesByCountry[tag] : _namesByCountry[Constants.DefaultCountry];
                }
                catch (KeyNotFoundException)
                {
                    return "Not found";
                }
            }
        }

        public ReadOnlyDictionary<string, double> Specifications { get { return new ReadOnlyDictionary<string, double>(_specifications); } }

        internal Model(string country, string name) { SetName(country, name); }
        internal void SetName(string country, string name)
        {
            if (_namesByCountry.ContainsKey(country))
            {
                Trace.WriteLine("Duplicated models: '" + name + "' for country '" + country + "'");
            }
            else if (country != Constants.DefaultCountry && !_namesByCountry.ContainsKey(Constants.DefaultCountry))
            {
                Trace.WriteLine("National model w/o common: '" + name + "' for country '" + country + "'");
            }
            else
            {
                _namesByCountry.Add(country, name);
            }
        }
        internal void SetSpec(string key, double value)
        {
            _specifications[key] = value;
        }

        public void CheckConsistency()
        {
            foreach (var nbc in _namesByCountry)
                if (!Core.Countries.ContainsKey(nbc.Key)) throw new ConsistencyException(string.Format("Model for absent country found ({0})", nbc.Key));
        }

        private Dictionary<string, string> _namesByCountry = new Dictionary<string, string>();
        private Dictionary<string, double> _specifications = new Dictionary<string, double>();
    }

    internal class UnitType : IUnitType, IConsistencyChecker
    {
        public IReadOnlyCollection<IModel> Models { get { return _models.Values; } }

        internal UnitType(int id, string name, string country = Constants.DefaultCountry) { SetModel(id, name, country); }

        internal void SetModel(int id, string name, string country = Constants.DefaultCountry)
        {
            if (_models.ContainsKey(id))
            {
                _models[id].SetName(country, name);
            }
            else
            {
                _models.Add(id, new Model(country, name));
            }
        }
        internal void SetSpec(int id, string key, double value)
        {
            if (!_models.ContainsKey(id))
            {
                Trace.WriteLine("No model description for model id " + id);
                _models.Add(id, new Model(Constants.DefaultCountry, "Unknown"));
            }

            _models[id].SetSpec(key, value);
        }

        public void CheckConsistency()
        {
            foreach (var item in _models)
                item.Value.CheckConsistency();
        }

        private Dictionary<int, Model> _models = new Dictionary<int, Model>();
    }

    internal class UnitTypes : IUnitTypes, IConsistencyChecker
    {
        public IReadOnlyCollection<UnitTypeName> Types { get { return _unitTypes.Keys; } }

        public IUnitType UnitType(UnitTypeName type)
        {
            UnitType ret;
            if (!_unitTypes.TryGetValue(type, out ret))
                ret = new UnitType(0, "Unknown");

            return ret;
        }

        internal void AddUnitType(UnitTypeName type, int id, string name, string country = Constants.DefaultCountry)
        {
            if (_unitTypes.ContainsKey(type))
            {
                _unitTypes[type].SetModel(id, name, country);
            }
            else
            {
                _unitTypes.Add(type, new UnitType(id, name, country));
            }
        }
        internal void SetSpec(UnitTypeName type, int id, string key, double value)
        {
            if (!_unitTypes.ContainsKey(type))
            {
                Trace.WriteLine("No type description for type " + type);
                _unitTypes.Add(type, new UnitType(id, "Unknown", Constants.DefaultCountry));
            }

            _unitTypes[type].SetSpec(id, key, value);
        }

        public void CheckConsistency()
        {
            foreach (var item in _unitTypes)
                item.Value.CheckConsistency();
        }

        private Dictionary<UnitTypeName, UnitType> _unitTypes = new Dictionary<UnitTypeName, UnitType>();
    }
}