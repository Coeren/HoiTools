using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace PersistentLayer
{
    enum TechEffects
    {

    }

    interface IAppliedTech
    {
    }

    interface ITechnology
    {

    }

    internal class Technology : ITechnology, IConsistencyChecker
    {
        public void CheckConsistency()
        {
            throw new NotImplementedException();
        }

        internal Technology()
        {

        }
    }
}
