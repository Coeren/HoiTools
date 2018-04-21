using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace PersistentLayer
{
    public sealed class Core
    {
        public static event EventHandler<string> DataChanged;

        public static string RootFolder
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
        public static IUnitTypes UnitTypes { get => Instance._unitTypes; }
        public static IReadOnlyDictionary<string, string> Countries { get => Instance._countries; }
        public static string CurrentCountry
        {
            get { return Instance._countries[Instance._countryTag]; }
            set
            {
                Instance._countryTag = Instance._countryTags[value];
                DataChanged?.Invoke(null, "CurrentCountry");
            }
        }
        public static void Prepare()
        {
            string root = RootFolder;
            if (!string.IsNullOrEmpty(root))
                Instance.Init(root);
        }

        internal static string CurrentCountryTag { get => Instance._countryTag; }
        internal static Dictionary<string, string> Text { get => Instance._textData; }

        private Core()
        {
            _countries[Constants.DefaultCountry] = "Common";
            _countryTags["Common"] = Constants.DefaultCountry;
            _countryTag = Constants.DefaultCountry;
        }

        private void ParseError(string file, string message)
        {
            Trace.WriteLine(file + ": parse error on '" + message + "'");
        }
        private enum ModelSpecsState { File, Spec }

        private void Init(string root)
        {
            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("Root folder is not set");
            }

            var textData = _textData;
            _textData = new Dictionary<string, string>();
            var unitTypes = _unitTypes;
            _unitTypes = new UnitTypes();
            var countries = _countries;
            _countries = new Dictionary<string, string>();
            var countryTags = _countryTags;
            _countryTags = new Dictionary<string, string>();
            Dictionary<TechAreas, TechArea> techAreas;

            try
            {
                ParseTextData(root);
                ParseModelNames(root);
                ParseCountries(root);
                ParseModelSpecifications(root);

                techAreas = (new TechParser(root)).Parse();
            }
            catch (Exception)
            {
                _textData = textData;
                _unitTypes = unitTypes;
                _countries = countries;
                _countryTags = countryTags;

                throw;
            }

            _techAreas = techAreas;

            _countryTag = Constants.DefaultCountry;
            DataChanged?.Invoke(null, "All");
        }

        private void ParseModelSpecifications(string root)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            foreach (UnitTypeName type in _unitTypes.Types)
            {
                string filename = root + Constants.ModelSpecsPath + type + ".txt";
                if (!File.Exists(filename))
                {
                    Trace.WriteLine("Cannot find specs file for " + type);
                    continue;
                }

                ModelSpecsState state = ModelSpecsState.File;
                int modelId = -1;
                using (StreamReader sr = new StreamReader(filename))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        int p = s.IndexOf('#');
                        if (p != -1)
                        {
                            s = s.Remove(p);
                        }
                        s = s.Trim();

                        switch (state)
                        {
                            case ModelSpecsState.File:
                                if (s.StartsWith("model"))
                                {
                                    state = ModelSpecsState.Spec;
                                    modelId++;
                                    if (!s.EndsWith("{") || s.IndexOf('{') != s.LastIndexOf('{'))
                                    {
                                        throw new InvalidOperationException("Bad format in '" + filename + "': " + s);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(s))
                                {
                                    Trace.WriteLine("Bad format in '" + filename + "', possible loss of data:");
                                    Trace.Indent();
                                    Trace.WriteLine(s);
                                    Trace.Unindent();
                                }
                                break;
                            case ModelSpecsState.Spec:
                                if (s.StartsWith("}"))
                                {
                                    state = ModelSpecsState.File;
                                }
                                else if (s.IndexOf('}') != -1)
                                {
                                    throw new InvalidOperationException("Bad format in '" + filename + "': " + s);
                                }
                                else
                                {
                                    var a = s.Split('=');
                                    if (a.Count() != 2)
                                    {
                                        break;
                                    }

                                    string k = a[0].Trim();
                                    string kh;
                                    if (_textData.TryGetValue(k, out kh))
                                    {
                                        k = kh;
                                    }
                                    double v;
                                    if (!double.TryParse(a[1], NumberStyles.Float, culture, out v))
                                    {
                                        Trace.WriteLine("Bad format in '" + filename + "', possible loss of data:");
                                        Trace.Indent();
                                        Trace.WriteLine(s);
                                        Trace.Unindent();
                                    }
                                    else
                                    {
                                        _unitTypes.SetSpec(type, modelId, k, v);
                                    }
                                }
                                break;

                            default:
                                throw new InvalidOperationException("Unknown state while parsing '" + filename + "'");
                        }
                    }
                }
            }
        }

        private void ParseModelNames(string root)
        {
            try
            {
                using (StreamReader sr = new StreamReader(root + Constants.ModelsPath))
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
                            ParseError(Constants.ModelsPath, s);
                            continue;
                        }

                        var aa = a[0].Split('_');
                        if (aa.Count() != 3 && aa.Count() != 4)
                        {
                            ParseError(Constants.ModelsPath, s);
                            continue;
                        }

                        int shift = 0;
                        if (aa.Count() == 4)
                        {
                            shift = 1;
                        }

                        int type, id;
                        if (!int.TryParse(aa[1 + shift], out type) || !Enum.IsDefined(typeof(UnitTypeName), type) || !int.TryParse(aa[2 + shift], out id))
                        {
                            ParseError(Constants.ModelsPath, s);
                            continue;
                        }
                        string country = aa[0 + shift];

                        _unitTypes.AddUnitType((UnitTypeName) type, id, a[1], country);
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
                using (StreamReader sr = new StreamReader(root + Constants.TextPath))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.StartsWith("#EOF"))
                            break;

                        // comment or not decoded
                        if (s.StartsWith("#") || s.Contains(";;;;;;;;;;;"))
                            continue;

                        var a = s.Split(';');
                        if (a.Count() >= 2 && !string.IsNullOrWhiteSpace(a[0]) && !string.IsNullOrWhiteSpace(a[1]))
                        {
                            if (_textData.ContainsKey(a[0]))
                                ParseError(Constants.TextPath, "var '" + a[0] + "' overwritten (" + _textData[a[0]] + " to " + a[1] + ")");

                            _textData[a[0]] = a[1];
                        }
                        else
                        {
                            ParseError(Constants.TextPath, s);
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
                _countries[Constants.DefaultCountry] = "Common";
                _countryTags["Common"] = Constants.DefaultCountry;

                using (StreamReader sr = new StreamReader(root + Constants.CountriesPath))
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

                            // Skip not using countries
                            if (country == "User Defined") continue;

                            if (_countries.ContainsKey(a[0]))
                            {
                                Trace.WriteLine("Duplicated country: '" + a[0] + "' for country '" + country + "'");
                            }
                            else
                            {
                                _countries[a[0]] = country;
                                _countryTags[country] = a[0];
                            }
                        }
                        else
                        {
                            ParseError(Constants.CountriesPath, s);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("Error accessing text data", e);
            }

            _unitTypes.CheckConsistency();
        }

        private static readonly Lazy<Core> _lazy = new Lazy<Core>(() => new Core());
        private static Core Instance { get { return _lazy.Value; } }

        private string _countryTag = Constants.DefaultCountry;
        private Dictionary<string, string> _textData = new Dictionary<string, string>();
        private UnitTypes _unitTypes = new UnitTypes();
        private Dictionary<string, string> _countries = new Dictionary<string, string>();
        private Dictionary<string, string> _countryTags = new Dictionary<string, string>();
        private Dictionary<TechAreas, TechArea> _techAreas = new Dictionary<TechAreas, TechArea>();
    }
}