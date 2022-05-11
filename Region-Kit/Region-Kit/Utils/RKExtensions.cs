using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RegionKit.Machinery;
using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.IO;

using static RWCustom.Custom;
using static RegionKit.Utils.RKUtils;
using static UnityEngine.Mathf;
using static RegionKit.Machinery.MachineryStatic;

using URand = UnityEngine.Random;

namespace RegionKit.Utils
{
    public static class RKExtensions
    {
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

        public static IntVector2 ToIV2(this Vector2 sv) => new((int)sv.x, (int)sv.y);
        public static Vector2 ToV2(this IntVector2 sv) => new(sv.x, sv.y);
        
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
        
        public static FContainer ReturnFContainer(this RoomCamera rcam, ContainerCodes cc) 
            => rcam.ReturnFContainer(cc.ToString());
        public static string[] SplitAndREE(this string target, string seperator) => target.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);

        #region refl extensions
        /// <summary>
        /// cleans up all non valuetype fields in a type. for realm cleanups
        /// </summary>
        /// <param name="t"></param>
        internal static void CleanUpStatic(this Type t)
        {
            foreach (var fld in t.GetFields(allContextsStatic))
            {
                try
                {
                    if (fld.FieldType.IsValueType || fld.IsLiteral) continue;
                    fld.SetValue(null, default);
                }
                catch { }

            }
            foreach (var child in t.GetNestedTypes()) child.CleanUpStatic();
        }

        /// <summary>
        /// dumps an IL context into a specified file. Watch out for invalidchars
        /// </summary>
        /// <param name="il">context to be dumped</param>
        /// <param name="rf">folder to dump into</param>
        /// <param name="nameOverride">replaces filename if specified</param>
        internal static void dump(this ILContext il, string rf, string nameOverride = default)
        {
            var oname = il.Method.FullName.SkipWhile(c => Path.GetInvalidPathChars().Contains(c));
            var sb = new StringBuilder();
            foreach (var c in oname) sb.Append(c);
            File.WriteAllText(Path.Combine(rf, nameOverride ?? sb.ToString()), il.ToString());
        }

        #endregion
        #region collection extensions
        /// <summary>
        /// adds or updates a keypair
        /// </summary>
        /// <typeparam name="tKey"></typeparam>
        /// <typeparam name="tValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        internal static bool SetKey<tKey, tValue>(this IDictionary<tKey, tValue> dict, tKey key, tValue val)
        {
            if (dict == null) throw new ArgumentNullException();
            try
            {
                if (!dict.ContainsKey(key)) dict.Add(key, val);
                else dict[key] = val;
                return true;
            }
            catch
            {
                return false;
            }
            
        }
        /// <summary>
        /// removes a keypair if key present
        /// </summary>
        /// <typeparam name="tKey"></typeparam>
        /// <typeparam name="tVal"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        internal static bool TryRemoveKey<tKey, tVal>(this IDictionary<tKey, tVal> dict, tKey key)
        {
            if (dict.ContainsKey(key)) { dict.Remove(key); return true; }
            else return false;
        }
        internal static bool IndexInRange(this object[] arr, int index) => index > -1 && index < arr.Length;
        internal static T RandomOrDefault<T>(this T[] arr)
        {
            var res = default(T);
            if (arr.Length > 0) return arr[URand.Range(0, arr.Length)];
            return res;
        }
        public static T RandomOrDefault<T>(this List<T> l)
        {
            if (l.Count == 0) return default;
            //var R = new System.Random(l.GetHashCode());
            return l[URand.Range(0, l.Count)];
        }

        public static void AddMultiple<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, params TKey[] keys)
        {
            dict.AddMultiple(value, ieKeys: keys);
        }

        public static void AddMultiple<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, IEnumerable<TKey> ieKeys)
        {
            foreach (TKey key in ieKeys)
            {
                dict.SetKey(key, value);
            }
        }
        //public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        //{
        //    try
        //    {
        //        dict.Add(key, value);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        #endregion
    }
}
