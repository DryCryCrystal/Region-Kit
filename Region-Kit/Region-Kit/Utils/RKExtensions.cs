using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RegionKit.Machinery;
using UnityEngine;
using RWCustom;

using static RWCustom.Custom;
using static RegionKit.Utils.RKUtils;
using static UnityEngine.Mathf;
using static RegionKit.Machinery.MachineryStatic;

namespace RegionKit.Utils
{
    public static class RKExtensions
    {
        public const BindingFlags allContexts = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        public static MethodInfo GetMethodAllContexts(this Type self, string name)
        {
            return self.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        }
        public static PropertyInfo GetPropertyAllContexts(this Type self, string name)
        {
            return self.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        }
        
        public static float GetGlobalPower(this Room self)
        {
            if (ManagersByRoom.TryGetValue(self.GetHashCode(), out var rpm)) return rpm.GetGlobalPower();
            return self.world?.rainCycle?.brokenAntiGrav?.CurrentLightsOn ?? 1f;
        }
        public static float GetPower(this Room self, Vector2 point)
        {
            if (ManagersByRoom.TryGetValue(self.GetHashCode(), out var rpm)) return rpm.GetPowerForPoint(point);
            return self.world?.rainCycle?.brokenAntiGrav?.CurrentLightsOn ?? 1f;
        }

        public static IntVector2 ToIV2(this Vector2 sv) => new IntVector2((int)sv.x, (int)sv.y);
        public static Vector2 ToV2(this IntVector2 sv) => new Vector2(sv.x, sv.y);
        
        public static List<IntVector2> ReturnTiles (this IntRect ir)
        {
            var res = new List<IntVector2>();
            for (int x = ir.left; x < ir.right; x++)
            {
                for (int y = ir.bottom; y < ir.top; y++)
                {
                    res.Add(new IntVector2(x, y));
                }
            }
            return res;
        }

        public static void ClampToNormal(this Color self)
        {
            self.r = Clamp01(self.r);
            self.g = Clamp01(self.g);
            self.b = Clamp01(self.b);
            self.a = Clamp01(self.a);
        }
        public static List<T> AddRangeReturnSelf<T>(this List<T> self, IEnumerable<T> range)
        {
            if (self == null) self = new List<T>();
            self.AddRange(range);
            return self;
        }
        public static Color Deviation (this Color self, Color dev)
        {
            var res = new Color();
            for (int i = 0; i < 4; i++)
            {
                res[i] = ClampedFloatDeviation(self[i], dev[i]);
            }
            return res;
        }
        public static T RandomOrDefault<T>(this T[] a)
        {
            if (a.Length == 0) return default;
            //var R = new System.Random(UnityEngine.Random);
            return a[UnityEngine.Random.Range(0, a.Length)];
        }
        public static T RandomOrDefault<T>(this List<T> l)
        {
            if (l.Count == 0) return default;
            //var R = new System.Random(l.GetHashCode());
            return l[UnityEngine.Random.Range(0, l.Count)];
        }

        public static FContainer ReturnFContainer(this RoomCamera rcam, ContainerCodes cc) 
            => rcam.ReturnFContainer(cc.ToString());
    }
}
