using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PersistentLayer
{
    public enum UnitTypes
    {
        Infantry = 0,
        Cavalry,
        Motorised,
        Mechanized,
        Panzer,
        Paratrooper,
        Marine,
        Bergsjager,
        Militia,
        Fighter,
        Strategic_Bomber,
        Tactical_Bomber,
        Dive_Bomber,
        Naval_Bomber,
        Torpedo_Plane,
        Transport_Plane,
        FlyingBomb,
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
    }

    public interface IModelType
    {
        IReadOnlyCollection<int> Ids { get; }
        IModel Model(int id);
    }

    public interface IModels
    {
        IReadOnlyCollection<UnitTypes> Types { get; }
        IModelType ModelType(UnitTypes type);
    }

    internal class Model : IModel
    {
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

        private Dictionary<string, string> _namesByCountry = new Dictionary<string, string>();
    }

    internal class ModelType : IModelType
    {
        public IReadOnlyCollection<int> Ids { get { return _modelNames.Keys; } }
        public IModel Model(int id) { return _modelNames[id]; }

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

        private Dictionary<UnitTypes, ModelType> _modelTypes = new Dictionary<UnitTypes, ModelType>();
    }
}