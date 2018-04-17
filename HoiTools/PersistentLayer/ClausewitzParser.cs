using System;
using System.IO;

namespace PersistentLayer
{
    public class ClauzewitzSyntaxException : Exception
    {
        public ClauzewitzSyntaxException(string message) : base(message) { }
    }

    public class ClausewitzParser
    {
        public delegate void StringHandler(string name);
        public delegate void Handler();

        public ClausewitzParser(StringHandler beginBlock, Handler endBlock, StringHandler variable, StringHandler value)
        {
            _beginBlockHandler = beginBlock;
            _endBlockHandler = endBlock;
            _variableHandler = variable;
            _valueHandler = value;

            Reset();
        }

        public void Parse(string filename)
        {
            const string pattern = "Parse logic error";

            using (StreamReader sr = new StreamReader(filename))
            {
                string s, name = pattern;
                States state = States.Name;
                while ((s = sr.ReadLine()) != null)
                {
                    int p = s.IndexOf('#');
                    if (p != -1) s = s.Remove(p);
                    s = s.Trim();
                    if (s.Length == 0) continue;

                    s = s.Replace("=", " = ").Replace("{", " { ").Replace("}", " } ");

                    var a = s.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < a.Length; i++)
                    {
                        switch (state)
                        {
                            case States.Name:
                                if (a[i].StartsWith("}"))
                                {
                                    FinishBlock(s, filename);
                                    if (a[i].Length > 1)
                                    {
                                        a[i].Substring(1);
                                        i--;
                                    }
                                }
                                else
                                {
                                    name = a[i];
                                    state = States.Eq;
                                }
                                break;
                            case States.Eq:
                                if (a[i] != "=")
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of '='");

                                state = States.Val;
                                break;
                            case States.Val:
                                if (a[i].StartsWith("{"))
                                {
                                    StartBlock(name);
                                    name = pattern;
                                    state = States.Name;
                                    if (a[i].Length > 1)
                                    {
                                        a[i].Substring(1);
                                        i--;
                                    }
                                }
                                else
                                {
                                    _variableHandler(name);
                                    _valueHandler(a[i]);
                                    state = States.Name;
                                }
                                break;
                            default:
                                throw new ClauzewitzSyntaxException("Unknown state");
                        }
                    }
                }
            }
        }

        private enum States
        {
            Name,
            Eq,
            Val,
        }

        private void StartBlock(string name)
        {
            _depth++;
            _beginBlockHandler(name);
        }

        private void FinishBlock(string s, string filename)
        {
            if (--_depth < 0)
                throw new ClauzewitzSyntaxException("Unexpected '}' in '" + s + "' in '" + filename + "'");

            _endBlockHandler();
        }

        private void Reset()
        {
            _depth = 0;
        }

        private int _depth;
        private StringHandler _beginBlockHandler;
        private Handler _endBlockHandler;
        private StringHandler _variableHandler;
        private StringHandler _valueHandler;
    }
}
