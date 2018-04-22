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
        public delegate void Handler();
        public delegate void StringHandler(string name);
        public delegate void String2Handler(string name, string val);

        public ClausewitzParser(StringHandler beginBlock, Handler endBlock, String2Handler variable, StringHandler value)
        {
            _beginBlockHandler = beginBlock;
            _endBlockHandler = endBlock;
            _variable = variable;
            _value = value;

            Reset();
        }

        public void Parse(string filename)
        {
            Reset();
            using (StreamReader sr = new StreamReader(filename))
            {
                string s;

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
                        switch (_state)
                        {
                            case States.Block:
                                if (a[i] == "{")
                                    StartBlock();
                                else if (a[i] == "}")
                                    FinishBlock();
                                else if (a[i].IndexOfAny(_markup) != -1)
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a name/value");
                                else
                                {
                                    if (_buff != _pattern)
                                        throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "'");

                                    _buff = a[i];
                                    _state = States.Name;
                                }
                                break;
                            case States.Name:
                                if (a[i] == "=")
                                    _state = States.Eq;
                                else if (a[i] == "}")
                                    FinishBlock();
                                else if (a[i].IndexOfAny(_markup) != -1)
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a name/value");
                                else
                                    RegVal(a[i]);
                                break;

                            case States.Val:
                                if (a[i] == "}")
                                    FinishBlock();
                                else if (a[i].IndexOfAny(_markup) != -1)
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a name/value");
                                else
                                {
                                    if (_buff != _pattern)
                                        throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "'");

                                    _buff = a[i];
                                    _state = States.Name;
                                }
                                break;

                            case States.Eq:
                                if (a[i] == "{")
                                    StartBlock();
                                else if (a[i].IndexOfAny(_markup) != -1)
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a name/value");
                                else
                                {
                                    if (_buff == _pattern)
                                        throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "'");

                                    _variable(_buff, a[i]);
                                    _buff = _pattern;
//                                    _value(a[i]);
                                    _state = States.Val;
                                }
                                break;

                            case States.EndBlock:
                                if (a[i] == "}")
                                    FinishBlock();
                                else if (a[i].IndexOfAny(_markup) != -1)
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a name/value");
                                else
                                {
                                    if (_buff != _pattern)
                                        throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "'");

                                    _buff = a[i];
                                    _state = States.Var;
                                }
                                break;

                            case States.Var:
                                if (a[i] == "=")
                                    _state = States.Eq;
                                else
                                    throw new ClauzewitzSyntaxException("Unexpected '" + a[i] + "' in place of a '='");
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
            Block,
            Var,
            Eq,
            Val,
            EndBlock,
            Name // Var or Val
        }

        private void StartBlock()
        {
            if (_buff == _pattern)
                throw new ClauzewitzSyntaxException("Unexpected '{' without variable");

            _depth++;
            _beginBlockHandler(_buff);
            _buff = _pattern;
            _state = States.Block;
        }

        private void FinishBlock()
        {
            if (--_depth < 0)
                throw new ClauzewitzSyntaxException("Unexpected '}'");

            if (_buff != _pattern)
            {
                _value(_buff);
                _buff = _pattern;
            }

            _endBlockHandler();
            _state = States.EndBlock;
        }

        private void RegVal(string val)
        {
            if (_buff == _pattern)
                throw new ClauzewitzSyntaxException("Unexpected '" + val + "'");

            _value(_buff);
            _buff = _pattern;
            _value(val);
            _state = States.Val;
        }

        private void Reset()
        {
            _state = States.Block;
            _depth = 0;
            _buff = _pattern;
        }

        private int _depth = 0;
        private States _state = States.Block;
        private string _buff;

        private StringHandler _beginBlockHandler;
        private Handler _endBlockHandler;
        private String2Handler _variable;
        private StringHandler _value;

        private const string _pattern = "Parse logic error";
        private static char[] _markup = { '{', '}', '=' };
}
}
