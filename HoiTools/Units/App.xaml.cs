using Common;
using System.Diagnostics;
using System.Windows;

namespace Units
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly StringTextListener _listener = new StringTextListener();

        public App()
        {
            Trace.Listeners.Add(_listener);
        }

        public StringTextListener Log { get { return _listener; } }
        static public App Instance { get { return Current as App; } }
    }
}
