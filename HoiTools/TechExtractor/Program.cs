﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Common;
using PersistentLayer;

namespace TechExtractor
{
    class CmdLineOpt
    {
        [Option('d', "root", Required = true, HelpText = "Root folder for hoi/mod.")]
        public string Root { get; set; }

        [Option('a', "areas", SetName = "Areas", HelpText = "Extract tech effects.")]
        public bool Effects { get; set; }

        [Option('e', "effects", SetName = "Effects", HelpText = "Extract tech areas.")]
        public bool Areas { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var res = Parser.Default.ParseArguments<CmdLineOpt>(args);

            return res.MapResult(opts => Run(opts), _ => -1);
        }

        private static int Run(CmdLineOpt opts)
        {
            try
            {
                if (!opts.Root.EndsWith(@"\")) opts.Root += @"\";

                // Inversion?.. FFS, what an asshole designed this library...
                if (!opts.Effects)
                {
                    ExtractEffectsFromFile(opts.Root + Constants.TechPath);
                }
                else if (!opts.Areas)
                {
                    Console.WriteLine("Extracting areas from " + opts.Root);
                }
                else
                {
                    throw new Exception("No operation selected");
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private enum EffectStates
        {
            Effects,
            Command,
            Unknown
        }

        private static void ExtractEffectsFromFile(string path)
        {
            EffectStates state = EffectStates.Unknown;
            string buff = null;
            MultiMap<string, string> effects = new MultiMap<string, string>();
            ClausewitzParser parser = new ClausewitzParser(
                name =>
                {
                    if (name == "effects")
                    {
                        if (state != EffectStates.Unknown) throw new ClauzewitzSyntaxException("effects block inside " + state.ToString());
                        state = EffectStates.Effects;
                    }
                    else if (name == "command")
                    {
                        if (state != EffectStates.Effects) throw new ClauzewitzSyntaxException("command block inside " + state.ToString());
                        state = EffectStates.Command;
                    }
                },
                () =>
                {
                    if (state == EffectStates.Command) state = EffectStates.Effects;
                    else if (state == EffectStates.Effects) state = EffectStates.Unknown;
                },
                var =>
                {
                    if (state != EffectStates.Command) return;
                    if (buff != null) throw new ClauzewitzSyntaxException("second variable name in a row (" + buff + ", " + var + ")");
                    buff = var;
                },
                val =>
                {
                    if (state != EffectStates.Command) return;
                    if (buff == null) throw new ClauzewitzSyntaxException("value without variable name (" + val + ")");
                    effects.Add(buff, val);
                    buff = null;
                });

            foreach (string filename in Directory.GetFiles(path))
            {
                if (filename.EndsWith(@"\old_nuclear_tech.txt"))
                    continue;

                parser.Parse(filename);

                if (buff != null)
                    throw new ClauzewitzSyntaxException("variable without a value (" + buff + ")");
            }

            Console.WriteLine("Found the following effects:");
            foreach (string key in effects.Keys)
            {
                Console.WriteLine(key + ":");
                foreach (string val in effects.ValueList(key))
                    Console.WriteLine("\t" + val);
            }
        }
    }
}