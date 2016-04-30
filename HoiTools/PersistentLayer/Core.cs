using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace PersistentLayer
{
    public sealed class Core
    {
        private static readonly Lazy<Core> _lazy = new Lazy<Core>(() => new Core());
        private static Core Instance { get { return _lazy.Value; } }

        static public string RootFolder
        {
            get { return ConfigurationManager.AppSettings["RootFolder"]; }
            set
            {
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

                Instance.Init(value);
            }
        }
        static public IModels Models { get { return Instance._models; } }
        static public IReadOnlyCollection<string> Countries { get { return Instance._countries.Values; } }
        static internal string CurrentCountryTag { get { return Instance._countryTag; } }
        static public string CurrentCountry
        {
            get { return Instance._countries[Instance._countryTag]; }
            set { Instance._countryTag = Instance._countryTags[value]; }
        }


        private Core()
        {
            foreach (UnitTypes type in Enum.GetValues(typeof(UnitTypes)))
            {
                _models.AddModel(type, 0, "");
            }

            _countries[Constants.DefaultCountry] = "Common";
            _countryTags["Common"] = Constants.DefaultCountry;
            _countryTag = Constants.DefaultCountry;
        }

        private void ParseError(string file, string message)
        {
            Trace.WriteLine(file + ": parse error on '" + message + "'");
        }

        private void Init(string root)
        {
            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("Root folder is not set");
            }

            var textData = _textData;
            _textData = new Dictionary<string, string>();
            var models = _models;
            _models = new Models();
            var countries = _countries;
            _countries = new Dictionary<string, string>();
            var countryTags = _countryTags;
            _countryTags = new Dictionary<string, string>();

            try
            {
                ParseTextData(root);
                ParseModelNames(root);
                ParseCountries(root);
            }
            catch (Exception)
            {
                _textData = textData;
                _models = models;
                _countries = countries;
                _countryTags = countryTags;

                throw;
            }

            CountriesChanged?.Invoke(null, EventArgs.Empty);
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
                        if (s.StartsWith("#EOF"))
                        {
                            break;
                        }

                        var a = s.Split(';');
                        if (a.Count() < 2 || string.IsNullOrWhiteSpace(a[0]) || string.IsNullOrWhiteSpace(a[1]))
                        {
                            ParseError(InternalConstants.ModelsPath, s);
                            continue;
                        }

                        var aa = a[0].Split('_');
                        if (aa.Count() != 3 && aa.Count() != 4)
                        {
                            ParseError(InternalConstants.ModelsPath, s);
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
                            ParseError(InternalConstants.ModelsPath, s);
                            continue;
                        }
                        string country = aa[0 + shift];

                        _models.AddModel((UnitTypes) type, id, a[1], country);
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("Error accessing models data", e);
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
                        if (s.StartsWith("#EOF"))
                        {
                            break;
                        }

                        var a = s.Split(';');
                        if (a.Count() >= 2 && !string.IsNullOrWhiteSpace(a[0]) && !string.IsNullOrWhiteSpace(a[1]))
                        {
                            _textData[a[0]] = a[1];
                        }
                        else
                        {
                            ParseError(InternalConstants.TextPath, s);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("Error accessing text data", e);
            }
        }

        private void ParseCountries(string root)
        {
            try
            {
                using (StreamReader sr = new StreamReader(root + InternalConstants.CountriesPath))
                {
                    string s = sr.ReadLine(); // skip header
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.StartsWith("END"))
                        {
                            break;
                        }

                        var a = s.Split(';');
                        if (a.Count() == 5 && !string.IsNullOrWhiteSpace(a[0]))
                        {
                            string country = _textData[a[0]];
                            _countries[a[0]] = country;
                            _countryTags[country] = a[0];
                        }
                        else
                        {
                            ParseError(InternalConstants.CountriesPath, s);
                        }
                    }
                }

                _countries[Constants.DefaultCountry] = "Common";
                _countryTags["Common"] = Constants.DefaultCountry;
            }
            catch (Exception e)
            {
                throw new IOException("Error accessing text data", e);
            }
        }

        private Dictionary<string, string> _textData = new Dictionary<string, string>();
        private Models _models = new Models();
        private Dictionary<string, string> _countries = new Dictionary<string, string>();
        private Dictionary<string, string> _countryTags = new Dictionary<string, string>();
        private string _countryTag = Constants.DefaultCountry;

        static public event EventHandler CountriesChanged;
    }
}