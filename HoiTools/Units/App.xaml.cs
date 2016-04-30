using Common;
using PersistentLayer;
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

        static public StringTextListener Log { get { return (Current as App)._listener; } }

        private App()
        {
            Trace.Listeners.Add(_listener);
        }
    }
}
