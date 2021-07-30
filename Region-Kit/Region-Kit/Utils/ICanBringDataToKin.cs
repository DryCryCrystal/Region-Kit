using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Utils
{
    public interface ICanBringDataToKin <T>
    {
        void BringToKin(T other);
    }
}
