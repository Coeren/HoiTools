using System.Linq;
using System.Collections.Generic;

using Common;

namespace PersistentLayer
{
    public enum TechAreas
    {
        air_doctrine,
        armor,
        artillery,
        electronics,
        heavy_aircraft,
        industry,
        infantry,
        land_doctrine,
        light_aircraft,
        naval_doctrine,
        naval,
        nuclear,
        rocket,
        submarine
    }

    public enum TechEffects
    {
        AA_batteries,
        activate_unit_type,
        air_attack,
        air_defense,
        air_detection,
        army_detection,
        blizzard_attack,
        blizzard_defense,
        blizzard_move,
        build_cost,
        build_time,
        coast_fort_eff,
        convoy_def_eff,
        deactivate,
        desert_attack,
        desert_defense,
        desert_move,
        double_nuke_prod,
        forest_attack,
        forest_defense,
        fort_attack,
        frozen_attack,
        frozen_defense,
        frozen_move,
        fuel_consumption,
        ground_def_eff,
        ground_defense,
        hard_attack,
        hill_attack,
        hill_defense,
        industrial_modifier,
        industrial_multiplier,
        intelligence,
        jungle_attack,
        jungle_defense,
        manpowerpool,
        max_organization,
        mountain_attack,
        mountain_defense,
        naval_attack,
        naval_defense,
        new_model,
        night_attack,
        night_defense,
        night_move,
        nuke_level,
        paradrop_attack,
        range,
        research_cost,
        research_time,
        river_attack,
        shore_attack,
        snow_attack,
        snow_defense,
        snow_move,
        soft_attack,
        speed,
        strategic_attack,
        sub_attack,
        sub_detection,
        supply_consumption,
        surface_detection,
        surprise,
        tactical_attack,
        urban_attack,
        urban_defense,
        visibility
    }

    public enum TechEffectsTargets
    {
        air,
        anti_air,
        anti_tank,
        application,
        armor,
        artillery,
        battleship,
        bergsjaeger,
        carrier,
        cavalry,
        coal_to_oil,
        cruiser,
        destroyer,
        dive_bomber,
        engineer,
        fighter,
        flying_bomb,
        flying_rocket,
        infantry,
        land,
        marine,
        mechanized,
        militia,
        motorized,
        naval_bomber,
        oil_to_rubber,
        paratrooper,
        strategic_bomber,
        submarine,
        supplies,
        tactical_bomber,
        them,
        theoretical,
        torpedo_plane,
        total,
        transport_plane,
        transports,
        us
    }

    public interface ITechEffect
    {
        TechEffectsTargets Applies { get; }
        TechEffects Type { get; }
        double Value { get; }
    }

    public interface ITechnology
    {
        int Id { get; }
        TechAreas Area { get; }
        string Name { get; }
        string Desc { get; }
        int Cost { get; }
        int Duration { get; }
        IReadOnlyCollection<ITechnology> Predecessors { get; }
        IReadOnlyCollection<ITechnology> Successors { get; }
    }

    public interface ITheoryTech : ITechnology
    {
        IReadOnlyCollection<ITechnology> Children { get; }
    }

    public interface IAppliedTech : ITechnology
    {
        ITheoryTech Parent { get; }
        IReadOnlyCollection<ITechEffect> Effects { get; }
    }

    public interface ITechArea
    {
        int Id { get; }
        TechAreas Area { get; }
        string Name { get; }
        string Desc { get; }
        ITheoryTech Root { get; }
    }

    internal class TechArea : ITechArea, IConsistencyChecker
    {
        public int Id { get; internal set; }
        public TechAreas Area { get; internal set; }
        public string Name { get; internal set; }
        public string Desc { get; internal set; }
        public ITheoryTech Root { get => _root; }

        public void CheckConsistency()
        {
            ChkCon();
            _root.CheckConsistency();
        }
        public void CheckConsistency<T>(T param)
        {
            ChkCon();
            _root.CheckConsistency(param);
        }

        internal void SetRoot(TheoryTech root) { _root = root; }
        private void ChkCon()
        {
            if (Id <= 0 || !Area.IsValid() || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Desc) || _root == null)
                throw new ConsistencyException("TechArea is not configured");
        }

        private TheoryTech _root;
    }

    internal class TechEffect : ITechEffect, IConsistencyChecker
    {
        public TechEffectsTargets Applies { get; internal set; }
        public TechEffects Type { get; internal set; }
        public double Value { get; internal set; }

        public void CheckConsistency()
        {
            if (!Applies.IsValid() || !Type.IsValid())
                throw new ConsistencyException("Invalid effect");
        }
        public void CheckConsistency<T>(T param)
        {
            CheckConsistency();
        }

        internal TechEffect() {}
        internal TechEffect(TechEffectsTargets applies, TechEffects type, double value)
        {
            Applies = applies;
            Type = type;
            Value = value;
        }
    }

    internal class Technology : ITechnology, IConsistencyChecker
    {
        public int Id { get; internal set; }
        public TechAreas Area { get; internal set; }
        public string Name { get; internal set; }
        public string Desc { get; internal set; }
        public int Cost { get; internal set; }
        public int Duration { get; internal set; }
        public IReadOnlyCollection<ITechnology> Predecessors { get => _predecessors; }
        public IReadOnlyCollection<ITechnology> Successors { get => _successors; }

        public void CheckConsistency()
        {
            if (Id <= 0 || !Area.IsValid() || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Desc) || Cost <= 0 || Duration <= 0)
                throw new ConsistencyException("Invalid tech '" + Name + "'");

            if (_predecessors.Count > 0)
            {
                bool found = false;
                foreach (var item in _predecessors)
                    if (item._successors.Contains(this))
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    throw new ConsistencyException("Pred/succ inconsistency in tech '" + Name + "'");
            }
            else if (this.GetType() != typeof(TheoryTech))
                throw new ConsistencyException("Pred/succ inconsistency in tech '" + Name + "'");


            foreach (var item in _successors)
                item.CheckConsistency();
        }
        public void CheckConsistency<T>(T param)
        {
            Dictionary<TechAreas, ITechArea> areas = param as Dictionary<TechAreas, ITechArea>;

            if (Id <= 0 || !Area.IsValid() || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Desc) || Cost <= 0 || Duration <= 0)
                throw new ConsistencyException("Invalid tech '" + Name + "'");

            if (_predecessors.Count > 0)
            {
                bool found = false;
                foreach (var item in _predecessors)
                    if (item._successors.Contains(this))
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    throw new ConsistencyException("Pred/succ inconsistency in tech '" + Name + "'");
            }
            else if (this.GetType() != typeof(TheoryTech) || areas != null && !areas.Values.Any(a => a.Root == this))
                throw new ConsistencyException("Pred/succ inconsistency in tech '" + Name + "'");

            foreach (var item in _successors)
                item.CheckConsistency(param);
        }

        internal Technology() {}
        internal Technology(int id, TechAreas area, string name, string desc, int cost, int duration)
        {
            Id = id;
            Area = area;
            Name = name;
            Desc = desc;
            Cost = cost;
            Duration = duration;
        }

        internal List<Technology> Preds => _predecessors;
        internal List<Technology> Succs => _successors;

        List<Technology> _predecessors = new List<Technology>();
        List<Technology> _successors = new List<Technology>();
    }

    internal class TheoryTech : Technology, ITheoryTech
    {
        public IReadOnlyCollection<ITechnology> Children { get => _children; }

        public new void CheckConsistency()
        {
            if (_children.Count == 0)
                throw new ConsistencyException("No siblings in theory tech '" + Name + "'");

            base.CheckConsistency();
        }
        public new void CheckConsistency<T>(T param)
        {
            if (_children.Count == 0)
                throw new ConsistencyException("No siblings in theory tech '" + Name + "'");

            base.CheckConsistency(param);
        }

        internal TheoryTech() {}
        internal TheoryTech(int id, TechAreas area, string name, string desc, int cost, int duration) : base(id, area, name, desc, cost, duration) {}

        internal List<ITechnology> Brood => _children;

        private List<ITechnology> _children = new List<ITechnology>();
    }

    internal class AppliedTech : Technology, IAppliedTech
    {
        public IReadOnlyCollection<ITechEffect> Effects { get => _effects; }
        public ITheoryTech Parent { get; internal set; }

        public new void CheckConsistency()
        {
            if (_effects.Count == 0) throw new ConsistencyException("No effects in applied tech '" + Name + "'");
            if (Parent == null) throw new ConsistencyException("No parent in applied tech '" + Name + "'");
            foreach (var item in _effects)
                item.CheckConsistency();

            base.CheckConsistency();
        }
        public new void CheckConsistency<T>(T param)
        {
            if (_effects.Count == 0) throw new ConsistencyException("No effects in applied tech '" + Name + "'");
            if (Parent == null) throw new ConsistencyException("No parent in applied tech '" + Name + "'");
            foreach (var item in _effects)
                item.CheckConsistency(param);

            base.CheckConsistency(param);
        }

        internal AppliedTech() {}
        internal AppliedTech(int id, TechAreas area, string name, string desc, int cost, int duration, ITheoryTech parent) : base(id, area, name, desc, cost, duration)
        {
            Parent = parent;
        }

        internal List<TechEffect> Eff => _effects;

        private List<TechEffect> _effects = new List<TechEffect>();
    }
}
