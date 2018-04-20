using System;
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

    public interface ITechEffect
    {
        int Applies { get; }
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
            if (Id <= 0 || !Enum.IsDefined(typeof(TechAreas), Area) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Desc) || _root == null)
                throw new ConsistencyException("TechArea is not configured");
        }

        internal void SetRoot(TheoryTech root) { _root = root; }

        private TheoryTech _root;
    }

    internal class TechEffect : ITechEffect
    {
        public int Applies { get; }
        public TechEffects Type { get; }
        public double Value { get; }

        internal TechEffect(int applies, TechEffects type, double value)
        {
            Applies = applies;
            Type = type;
            Value = value;
        }
    }

    internal class Technology : ITechnology, IConsistencyChecker
    {
        public int Id { get; }
        public TechAreas Area { get; }
        public string Name { get; }
        public string Desc { get; }
        public int Cost { get; }
        public int Duration { get; }
        public List<ITechnology> Predecessors { get; }
        public List<ITechnology> Successors { get; }

        public void CheckConsistency()
        {
            throw new NotImplementedException();
        }

        internal Technology(int id, TechAreas area, string name, string desc, int cost, int duration)
        {
            Id = id;
            Area = area;
            Name = name;
            Desc = desc;
            Cost = cost;
            Duration = duration;
            Predecessors = new List<ITechnology>();
            Successors = new List<ITechnology>();
    }
}

    internal class TheoryTech : Technology, ITheoryTech
    {
        public List<ITechnology> Children { get; }

        internal TheoryTech(int id, TechAreas area, string name, string desc, int cost, int duration) : base(id, area, name, desc, cost, duration)
        {
            Children = new List<ITechnology>();
        }
    }

    internal class AppliedTech : Technology, IAppliedTech
    {
        public List<ITechEffect> Effects { get; }
        public ITheoryTech Parent { get; }

        internal AppliedTech(int id, TechAreas area, string name, string desc, int cost, int duration, ITheoryTech parent) : base(id, area, name, desc, cost, duration)
        {
            Parent = parent;
            Effects = new List<ITechEffect>();
        }
    }
}
