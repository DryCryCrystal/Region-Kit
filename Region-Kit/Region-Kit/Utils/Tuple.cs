using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Utils
{
    public class Tuple<t1, t2>
    {
        public Tuple(t1 tin1, t2 tin2)
        {
            Item1 = tin1;
            Item2 = tin2;
        }
        public t1 Item1;
        public t2 Item2;
    }

    public class Tuple<t1, t2, t3>
    {
        public Tuple(t1 tin1, t2 tin2, t3 tin3)
        {
            Item1 = tin1;
            Item2 = tin2;
            Item3 = tin3;
        }
        public t1 Item1;
        public t2 Item2;
        public t3 Item3;
    }

    public class Tuple<t1, t2, t3, t4>
    {
        public Tuple(t1 tin1, t2 tin2, t3 tin3, t4 tin4)
        {
            item1 = tin1;
            item2 = tin2;
            item3 = tin3;
            item4 = tin4;
        }
        public t1 item1;
        public t2 item2;
        public t3 item3;
        public t4 item4;
    }
}
