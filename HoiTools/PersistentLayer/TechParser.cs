using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Common;

namespace PersistentLayer
{
    internal class TechParser
    {
        public TechParser(string root)
        {
            _root = root;
        }

        internal Dictionary<TechAreas, TechArea> Parse()
        {
            _areas = new Dictionary<TechAreas, TechArea>();
            _reqs = new MultiMap<int, int>();
            _mapping = new Dictionary<int, Technology>();
            _state = States.file;

            ClausewitzParser clausewitzParser = new ClausewitzParser(BeginBlock, EndBlock, Variable, Value);
            foreach (string filename in Directory.GetFiles(_root + Constants.TechPath))
            {
                if (filename.EndsWith(@"\old_nuclear_tech.txt"))
                    continue;

                clausewitzParser.Parse(filename);
            }

            foreach (int tech in _reqs.Keys)
                foreach (int req in _reqs.ValueList(tech))
                {
                    _mapping[tech].Preds.Add(_mapping[req]);
                    _mapping[req].Succs.Add(_mapping[tech]);
                }

            foreach (var item in _areas)
                item.Value.CheckConsistency();

            return _areas;
        }

        private void BeginBlock(string name)
        {
            switch (_state)
            {
                case States.file:
                    if (name == "technology")
                    {
                        _area = new TechArea();
                        _state = States.area;
                        return;
                    }
                    break;
                case States.area:
                    if (name == "level")
                    {
                        _theo = new TheoryTech();
                        _state = States.theory;
                        return;
                    }
                    break;
                case States.theory:
                    if (name == "application")
                    {
                        _app = new AppliedTech();
                        _state = States.applied;
                        return;
                    }
                    break;
                case States.applied:
                    if (name == "required")
                    {
                        _state = States.reqs;
                        return;
                    }
                    else if (name == "effects")
                    {
                        _state = States.effect;
                        return;
                    }
                    break;
                case States.effect:
                    if (name == "command")
                    {
                        _eff = new TechEffect();
                        _state = States.command;
                        return;
                    }
                    break;
            }
            throw new ClauzewitzSyntaxException("Unexpected block '" + name + "' in state " + _state.ToString());
        }

        private void EndBlock()
        {
            switch (_state)
            {
                case States.file:
                    throw new ClauzewitzSyntaxException("Closing virtual block");

                case States.area:
                    if (_areas.ContainsKey(_area.Area))
                        throw new ClauzewitzSyntaxException("Duplicated areas: " + _area.Area.ToString());

                    _areas[_area.Area] = _area;
                    _state = States.file;
                    break;

                case States.theory:
                    if (_area.Root == null)
                        _area.SetRoot(_theo);
                    else
                        _reqs[_theo.Id] = _lastTheo.Id;

                    _lastTheo = _theo;
                    _mapping[_theo.Id] = _theo;
                    _state = States.area;
                    break;

                case States.applied:
                    _reqs[_app.Id] = _theo.Id;
                    _app.Parent = _theo;
                    _theo.Brood.Add(_app);
                    _mapping[_app.Id] = _app;
                    _state = States.theory;
                    break;

                case States.reqs:
                    _state = States.applied;
                    break;

                case States.effect:
                    _state = States.applied;
                    break;

                case States.command:
                    _state = States.effect;
                    _app.Eff.Add(_eff);
                    break;

                default:
                    throw new ClauzewitzSyntaxException("Unknown state");
            }
        }

        private void Variable(string name, string value)
        {
            switch (_state)
            {
                case States.area:
                    switch (name)
                    {
                        case "id":
                            _area.Id = int.Parse(value);
                            return;
                        case "category":
                            _area.Area = (TechAreas) Enum.Parse(typeof(TechAreas), value);
                            return;
                        case "name":
                            _area.Name = Core.Text.GetValue(value, value);
                            return;
                        case "desc":
                            _area.Desc = Core.Text.GetValue(value, value);
                            return;
                    }
                    break;

                case States.theory:
                    switch (name)
                    {
                        case "id":
                            _theo.Id = int.Parse(value);
                            return;
                        case "name":
                            _theo.Name = Core.Text.GetValue(value, value);
                            return;
                        case "desc":
                            _theo.Desc = Core.Text.GetValue(value, value);
                            return;
                        case "cost":
                            _theo.Cost = int.Parse(value);
                            return;
                        case "time":
                            _theo.Duration = int.Parse(value);
                            return;
                        case "number":
                        case "neg_offset":
                        case "pos_offset":
                            return;
                    }
                    break;

                case States.applied:
                    switch (name)
                    {
                        case "id":
                            _app.Id = int.Parse(value);
                            return;
                        case "name":
                            _app.Name = Core.Text.GetValue(value, value);
                            return;
                        case "desc":
                            _app.Desc = Core.Text.GetValue(value, value);
                            return;
                        case "cost":
                            _app.Cost = int.Parse(value);
                            return;
                        case "time":
                            _app.Duration = int.Parse(value);
                            return;
                        case "chance":
                        case "neg_offset":
                        case "pos_offset":
                            return;
                    }
                    break;

                case States.command:
                    switch (name)
                    {
                        case "type":
                            _eff.Type = (TechEffects) Enum.Parse(typeof(TechEffects), value);
                            return;
                        case "which":
                            TechEffectsTargets target;
                            if (Enum.TryParse(value, out target))
                                _eff.Applies = target;
                            return;
                        case "value":
                            _eff.Value = double.Parse(value, _culture);
                            return;
                        case "when":
                            return;
                    }
                    break;
            }
            throw new ClauzewitzSyntaxException("Unexpected variable in state " + _state.ToString());
        }

        private void Value(string value)
        {
            switch (_state)
            {
                case States.reqs:
                    _reqs[_app.Id] = int.Parse(value);
                    break;

                default:
                    throw new ClauzewitzSyntaxException("Unexpected value in state " + _state.ToString());
            }
        }

        private enum States
        {
            file,
            area,
            theory,
            applied,
            reqs,
            effect,
            command
        }

        private string _root;
        private CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");
        private Dictionary<TechAreas, TechArea> _areas;
        private MultiMap<int, int> _reqs;
        private Dictionary<int, Technology> _mapping;
        private States _state = States.file;
        private TechArea _area;
        private TheoryTech _theo;
        private TheoryTech _lastTheo;
        private AppliedTech _app;
        private TechEffect _eff;
    }
}