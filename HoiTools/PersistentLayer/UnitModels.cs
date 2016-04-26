using System;
using System.Collections.Generic;

namespace PersistentLayer
{
    public enum UnitTypes
    {
        Infantry = 0,
        Cavalry,
        Motorised,
        Mechanized,
        Armor,
        Paratrooper,
        Marines,
        Mountain,
        Militia,
        Fighter,
        StrategicBomber,
        MediumBomber,
        CloseSupport,
        NavalBomber,
        CAG,
        AirTransport,
        FlyingBomb,
        FlyingRocket,
        Battleship,
        Cruiser,
        Destroyer,
        Carrier,
        Submarine,
        Transport
    }

    public interface IModel
    {
        string Name(string country = Constants.DefaultCountry);
    }

    public interface IModelType
    {
        IModel Model(int id);
    }

    public interface IModels
    {
        IModelType ModelType(UnitTypes type);
    }

    internal class Model : IModel
    {
        public string Name(string country = Constants.DefaultCountry)
        {
            return _namesByCountry.ContainsKey(country) ? _namesByCountry[country] : _namesByCountry[Constants.DefaultCountry];
        }

        internal Model(string country, string name) { SetName(country, name); }
        internal void SetName(string country, string name) { _namesByCountry.Add(country, name); }

        private Dictionary<string, string> _namesByCountry = new Dictionary<string, string>();
    }

    internal class ModelType : IModelType
    {
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