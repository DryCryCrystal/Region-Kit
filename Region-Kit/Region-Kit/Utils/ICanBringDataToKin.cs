using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Utils
{
    /// <summary>
    /// Can be used for pretty much anything. Just a way of avoiding copypasting similar methods, really.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanBringDataToKin <T>
    {
        void BringToKin(T other);
    }
}
