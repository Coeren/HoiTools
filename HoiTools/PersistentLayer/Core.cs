using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PersistentLayer
{
    public class Core
    {
        public Core()
        {
            if (ConfigurationManager.AppSettings["Texts"] != null)
                ;
            if (ConfigurationManager.AppSettings["Tech"] != null)
                ;
        }
    }
    internal class TextData
    {
    }
}
