using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

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
    }
}
