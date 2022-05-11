using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Utils
{
    /// <summary>
    /// summon him to die instantly
    /// </summary>
    public class Joar : Exception
    {
        public Joar()
        {
            throw new Joar();
        }
    }
}
