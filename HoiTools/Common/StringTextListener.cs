using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Common
{
    public class StringTextListener : TraceListener, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private readonly StringBuilder _builder = new StringBuilder();

        public string Trace => _builder.ToString();

        public override void Write(string message)
        {
            _builder.AppendFormat("[{0}] {1}", DateTime.Now, message);
            OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        public override void WriteLine(string message)
        {
            _builder.AppendFormat("[{0}] {1}", DateTime.Now, message).AppendLine();
            OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }
    }
}
