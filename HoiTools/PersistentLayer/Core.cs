using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace PersistentLayer
{
    public interface ICoreConfigurator
    {
        string RootFolder { get; set; }
    }

    public interface ICore
    {
        void Test();
    }

    public sealed class Core : ICoreConfigurator, ICore
    {
        private static readonly Lazy<Core> _lazy = new Lazy<Core>(() => new Core());
        public static ICoreConfigurator Configurator { get { return _lazy.Value; } }
        public static ICore Instance
        {
            get
            {
                Core instance = _lazy.Value;
                if (!instance._inited)
                {
                    instance.Init((instance as ICoreConfigurator).RootFolder);
                }
                return instance;
            }
        }

        string ICoreConfigurator.RootFolder
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

                _inited = false;
                Init(value);
            }
        }

        private Core()
        {
        }

        private void Init(string root)
        {
            if (_inited)
            {
                throw new InvalidOperationException("Already inited");
            }

            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("Root folder is not set");
            }

            ParseTextData(root);
            ParseModelNames(root);
        }

        private void ParseModelNames(string root)
        {
            try
            {
                using (StreamReader sr = new StreamReader(root + InternalConstants.ModelsPath))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        var a = s.Split(';');
                        if (a.Count() < 2 || string.IsNullOrWhiteSpace(a[0]) || string.IsNullOrWhiteSpace(a[1]))
                        {
                            continue;
                        }

                        var aa = a[0].Split('_');
                        if (aa.Count() != 3 && aa.Count() != 4)
                        {
                            continue;
                        }

                        int shift = 0;
                        if (aa.Count() == 4)
                        {
                            shift = 1;
                        }

                        int type, id;
                        if (!int.TryParse(aa[1 + shift], out type) || !Enum.IsDefined(typeof(UnitTypes), type) || !int.TryParse(aa[2 + shift], out id))
                        {
                            continue;
                        }
                        string country = aa[0 + shift];

                        _models.AddModel((UnitTypes) type, id, a[1], country);
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("Cannot parse models data", e);
            }
        }

        private void ParseTextData(string root)
        {
            try
            {
                using (StreamReader sr = new StreamReader(root + InternalConstants.TextPath))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        var a = s.Split(';');
                        if (a.Count() >= 2 && !string.IsNullOrWhiteSpace(a[0]) && !string.IsNullOrWhiteSpace(a[1]))
                        {
                            _textData[a[0]] = a[1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("Cannot parse text data", e);
            }
        }

        void ICore.Test()
        {
        }

        private bool _inited = false;

        private Dictionary<string, string> _textData = new Dictionary<string, string>();
        private Models _models = new Models();
    }
}
