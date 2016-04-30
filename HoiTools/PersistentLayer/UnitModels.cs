using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PersistentLayer
{
    public enum UnitTypes
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

    public interface IModelType
    {
        IReadOnlyCollection<IModel> Models { get; }
    }

    public interface IModels
    {
        IReadOnlyCollection<UnitTypes> Types { get; }
        IModelType ModelType(UnitTypes type);
    }

    internal class Model : IModel
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

        private Dictionary<string, string> _namesByCountry = new Dictionary<string, string>();
        private Dictionary<string, double> _specifications = new Dictionary<string, double>();
    }

    internal class ModelType : IModelType
    {
        public IReadOnlyCollection<IModel> Models { get { return _modelNames.Values; } }

        internal ModelType(int id, string name, string country = Constants.DefaultCountry) { SetModel(id, name, country); }

        internal void SetModel(int id, string name, string country = Constants.DefaultCountry)
        {
            if (_modelNames.ContainsKey(id))
            {
                _modelNames[id].SetName(country, name);
            }
            else
            {
                _modelNames.Add(id, new Model(country, name));
            }
        }
        internal void SetSpec(int id, string key, double value)
        {
            if (!_modelNames.ContainsKey(id))
            {
                Trace.WriteLine("No model description for model id " + id);
                _modelNames.Add(id, new Model(Constants.DefaultCountry, "Unknown"));
            }

            _modelNames[id].SetSpec(key, value);
        }

        private Dictionary<int, Model> _modelNames = new Dictionary<int, Model>();
    }

    internal class Models : IModels
    {
        public IReadOnlyCollection<UnitTypes> Types { get { return _modelTypes.Keys; } }

        public IModelType ModelType(UnitTypes type)
        {
            return _modelTypes[type];
        }

        internal void AddModel(UnitTypes type, int id, string name, string country = Constants.DefaultCountry)
        {
            if (_modelTypes.ContainsKey(type))
            {
                _modelTypes[type].SetModel(id, name, country);
            }
            else
            {
                _modelTypes.Add(type, new ModelType(id, name, country));
            }
        }
        internal void SetSpec(UnitTypes type, int id, string key, double value)
        {
            if (!_modelTypes.ContainsKey(type))
            {
                Trace.WriteLine("No type description for type " + type);
                _modelTypes.Add(type, new ModelType(id, "Unknown", Constants.DefaultCountry));
            }

            _modelTypes[type].SetSpec(id, key, value);
        }

        private Dictionary<UnitTypes, ModelType> _modelTypes = new Dictionary<UnitTypes, ModelType>();
    }
}