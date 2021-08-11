using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RegionKit.Machinery;
using UnityEngine;
using RWCustom;

using static RegionKit.Machinery.MachineryStatic;

namespace RegionKit.Utils
{
    internal static class RKExtensions
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
    }
}
