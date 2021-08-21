using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

using static UnityEngine.Mathf;

namespace RegionKit.Utils
{
    public static class RKUtils
    {
        public static int ClampedIntDeviation(int start, int mDev, int minRes = int.MinValue, int maxRes = int.MaxValue)
        {
            return (Custom.IntClamp(UnityEngine.Random.Range(start - mDev, start + mDev), minRes, maxRes));
        }

        public static float ClampedFloatDeviation(float start, float mDev, float minRes = float.MinValue, float maxRes = float.MaxValue)
        {
            return Mathf.Clamp(Mathf.Lerp(start - mDev, start + mDev, UnityEngine.Random.value), minRes, maxRes);
        }

        public static IntRect ConstructIR(IntVector2 p1, IntVector2 p2) => new IntRect(Min(p1.x, p2.x), Min(p1.y, p2.y), Max(p1.x, p2.x), Max(p1.y, p2.y));
    }
}
