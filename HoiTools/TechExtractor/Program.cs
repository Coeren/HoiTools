using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
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

                if (opts.Effects)
                {
                    foreach (string filename in Directory.GetFiles(opts.Root + Constants.TechPath))
                        ExctractEffectsFromFile(filename);
                }
                else if (opts.Areas)
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

        private static void ExctractEffectsFromFile(string filename)
        {
        }
    }
}
