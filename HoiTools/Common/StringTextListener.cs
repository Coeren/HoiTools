using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Common
{
    public class StringTextListener : TraceListener
    {
        public delegate void TraceHandler(string trace);

        public event TraceHandler TraceAdded;
        protected virtual void OnTraceAdded(string trace)
        {
            TraceAdded?.Invoke(trace);
        }

        private readonly StringBuilder _builder = new StringBuilder();

        public string Trace => _builder.ToString();

        public override void Write(string message)
        {
            string trace = string.Format("[{0}] {1}", DateTime.Now, message);
            _builder.Append(trace);
            OnTraceAdded(trace);
        }

        public override void WriteLine(string message)
        {
            string trace = string.Format("[{0}] {1}\n", DateTime.Now, message);
            _builder.Append(trace);
            OnTraceAdded(trace);
        }
    }
}
