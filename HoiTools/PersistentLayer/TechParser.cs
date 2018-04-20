using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
            Dictionary<TechAreas, TechArea> areas = new Dictionary<TechAreas, TechArea>();

            ClausewitzParser clausewitzParser = new ClausewitzParser(BeginBlock, EndBlock, Variable, Value);
            foreach (string filename in Directory.GetFiles(_root + Constants.TechPath))
            {
                if (filename.EndsWith(@"\old_nuclear_tech.txt"))
                    continue;

                _area = new TechArea();
                clausewitzParser.Parse(filename);

                if (areas.ContainsKey(_area.Area))
                    throw new ClauzewitzSyntaxException("Duplicated areas: " + _area.Area.ToString());

                areas[_area.Area] = _area;
            }

            return areas;
        }

        private void BeginBlock(string name)
        {

        }

        private void EndBlock()
        {

        }

        private void Variable(string name, string value)
        {

        }

        private void Value(string value)
        {

        }

        private string _root;
        CultureInfo _culture = CultureInfo.CreateSpecificCulture("en-US");
        TechArea _area;
    }
}