using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PersistentLayer
{
    public sealed class Core
    {
        private static readonly Lazy<Core> _lazy = new Lazy<Core>(() => new Core());
        public static Core Instance { get { return _lazy.Value; } }

        public string RootFolder
        {
            get { return ConfigurationManager.AppSettings["RootFolder"]; }
            set
            {
                //ConfigurationManager.AppSettings["RootFolder"] = value;
                var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = cfg.AppSettings.Settings;
                if (settings["RootFolder"] == null)
                {
                    settings.Add("RootFolder", value);
                }
                else
                {
                    settings["RootFolder"].Value = value;
                }
                cfg.Save();
                ConfigurationManager.RefreshSection(cfg.AppSettings.SectionInformation.Name);
            }
        }

        private Core()
        {
        }
    }
    internal class TextData
    {
    }
}
