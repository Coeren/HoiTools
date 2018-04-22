using System;
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

        [Option('e', "effects", SetName = "Tree, Block, Variable", HelpText = "Extract tech areas.")]
        public bool Effects { get; set; }

        [Option('t', "tree", SetName = "Effects, Block, Variable", HelpText = "Extract tech areas.")]
        public bool Tree { get; set; }

        [Option('b', HelpText = "Extract possible block content.")]
        public string Block { get; set; }

        [Option('v', HelpText = "Extract possible variable values (only with -b).")]
        public string Variable { get; set; }
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

                if (opts.Tree)
                {
                    ExctractTechTree(opts.Root + Constants.TechPath);
                    PrintOutTree("", 0);
                }
                else if (!string.IsNullOrEmpty(opts.Block))
                {
                    ExctractTechTree(opts.Root + Constants.TechPath);

                    if (!string.IsNullOrEmpty(opts.Variable))
                        foreach (string item in _blocks[opts.Block].Vars.ValueList(opts.Variable).OrderBy(s => s))
                            Console.WriteLine(item);
                    else
                        foreach (string item in _blocks[opts.Block].Vars.Keys.OrderBy(s => s))
                            Console.WriteLine(item);
                }
                else if (opts.Effects)
                {
                    ExtractEffects(opts.Root + Constants.TechPath);
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

        private class Block
        {
            internal Block()
            {
                Siblings = new HashSet<string>();
                Vars = new MultiMap<string, string>();
            }

            internal HashSet<string> Siblings { get; }
            internal MultiMap<string, string> Vars { get; }
        }

        private static void ExctractTechTree(string path)
        {
            _blocks.Clear();
            _blocks.Add("", new Block());
            Stack<Block> stack = new Stack<Block>();
            stack.Push(_blocks[""]);
            ClausewitzParser parser = new ClausewitzParser(
                name =>
                {
                    stack.Peek().Siblings.Add(name);
                    if (!_blocks.ContainsKey(name))
                        _blocks.Add(name, new Block());

                    stack.Push(_blocks[name]);
                },
                () =>
                {
                    stack.Pop();
                },
                (name, val) =>
                {
                    stack.Peek().Vars[name] = val;
                },
                val =>
                {
                });

            foreach (string filename in Directory.GetFiles(path))
            {
                if (filename.EndsWith(@"\old_nuclear_tech.txt"))
                    continue;

                parser.Parse(filename);
            }
        }

        private static void PrintOutTree(string blockName, int level)
        {
            Console.WriteLine(new string('\t', level) + "+ " + blockName);
            Block block = _blocks[blockName];

            foreach (string attr in block.Vars.Keys)
                Console.WriteLine(new string('\t', level + 1) + "  " + attr);

            foreach (string sib in block.Siblings)
                PrintOutTree(sib, level + 1);
        }

        private enum EffectStates
        {
            Effects,
            Command,
            Unknown
        }

        private static void ExtractEffects(string path)
        {
            EffectStates state = EffectStates.Unknown;
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
                (name, val) =>
                {
                    if (state != EffectStates.Command) return;

                    effects.Add(name, val);
                },
                val =>
                {
                });

            foreach (string filename in Directory.GetFiles(path))
            {
                if (filename.EndsWith(@"\old_nuclear_tech.txt"))
                    continue;

                parser.Parse(filename);
            }

            Console.WriteLine("Found the following effects:");
            foreach (string key in effects.Keys)
            {
                Console.WriteLine(key + ":");
                foreach (string val in effects.ValueList(key))
                    Console.WriteLine("\t" + val);
            }
        }

        private static Dictionary<string, Block> _blocks = new Dictionary<string, Block>();
    }
}
