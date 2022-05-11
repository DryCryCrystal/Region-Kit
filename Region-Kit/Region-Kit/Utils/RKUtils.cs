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
using static UnityEngine.Mathf;

using URand = UnityEngine.Random;

namespace RegionKit.Utils
{
    /// <summary>
    /// contains general purpose utility methods
    /// </summary>
    public static class RKUtils
    {
        public static int ClampedIntDeviation(int start, int mDev, int minRes = int.MinValue, int maxRes = int.MaxValue)
        {
            return IntClamp(UnityEngine.Random.Range(start - mDev, start + mDev), minRes, maxRes);
        }

        public static float ClampedFloatDeviation(float start, float mDev, float minRes = float.MinValue, float maxRes = float.MaxValue)
        {
            return Clamp(Lerp(start - mDev, start + mDev, URand.value), minRes, maxRes);
        }

        public static IntRect ConstructIR(IntVector2 p1, IntVector2 p2) => new(Min(p1.x, p2.x), Min(p1.y, p2.y), Max(p1.x, p2.x), Max(p1.y, p2.y));

        #region refl flag templates
        public const BindingFlags allContexts = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        public const BindingFlags allContextsInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        public const BindingFlags allContextsStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public const BindingFlags allContextsCtor = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
        #endregion

        #region refl helpers
        public static MethodInfo GetMethodAllContexts(this Type self, string name)
        {
            return self.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        }
        public static PropertyInfo GetPropertyAllContexts(this Type self, string name)
        {
            return self.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        }
        /// <summary>
        /// returns prop backing field name
        /// </summary>
        public static string pbfiname(string propname) => $"<{propname}>k__BackingField";
        /// <summary>
        /// takes methodinfo from T, defaults to <see cref="allContextsInstance"/>
        /// </summary>
        /// <typeparam name="T">target type</typeparam>
        /// <param name="mname">methodname</param>
        /// <param name="context">binding flags, default private+public+instance</param>
        /// <returns></returns>
        public static MethodInfo methodof<T>(string mname, BindingFlags context = allContextsInstance)
            => typeof(T).GetMethod(mname, context);
        /// <summary>
        /// takes methodinfo from t, defaults to <see cref="allContextsStatic"/>
        /// </summary>
        /// <param name="t">target type</param>
        /// <param name="mname">method name</param>
        /// <param name="context">binding flags, default private+public+static</param>
        /// <returns></returns>
        public static MethodInfo methodof(Type t, string mname, BindingFlags context = allContextsStatic)
            => t.GetMethod(mname, context);
        /// <summary>
        /// Mother method of a delegate
        /// </summary>
        /// <typeparam name="Tm"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        public static MethodInfo methodofdel<Tm>(Tm m) where Tm : Delegate => m.Method;
        /// <summary>
        /// gets constructorinfo from T. no cctors by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="pms"></param>
        /// <returns></returns>
        public static ConstructorInfo ctorof<T>(BindingFlags context = allContextsCtor, params Type[] pms)
            => typeof(T).GetConstructor(context, null, pms, null);
        /// <summary>
        /// gets constructorinfo from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pms"></param>
        /// <returns></returns>
        public static ConstructorInfo ctorof<T>(params Type[] pms)
            => typeof(T).GetConstructor(pms);

        /// <summary>
        /// takes fieldinfo from T, defaults to <see cref="allContextsInstance"/>
        /// </summary>
        /// <typeparam name="T">target type</typeparam>
        /// <param name="name">field name</param>
        /// <param name="context">context, default private+public+instance</param>
        /// <returns></returns>
        public static FieldInfo fieldof<T>(string name, BindingFlags context = allContextsInstance)
            => typeof(T).GetField(name, context);
        /// <summary>
        /// searches loaded asms by name
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> FindAssembliesByName(string n)
        {
            var lasms = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = lasms.Length - 1; i > -1; i--)
                if (lasms[i].FullName.Contains(n)) yield return lasms[i];
        }
        /// <summary>
        /// force clones an object instance
        /// </summary>
        /// <typeparam name="T">tar type</typeparam>
        /// <param name="from">source object</param>
        /// <param name="to">target object</param>
        /// <param name="context">specifies context of fields to be cloned</param>
        public static void CloneInstance<T>(T from, T to, BindingFlags context = allContextsInstance)
        {
            var tt = typeof(T);
            foreach (FieldInfo field in tt.GetFields(context))
            {
                if (field.IsStatic) continue;
                //field.SetValueDirect(__makeref(to), field.GetValue(from));
                field.SetValue(to, field.GetValue(from), context, null, System.Globalization.CultureInfo.CurrentCulture);
            }
        }
        #endregion


        #region randomization extensions
        public static float RandSign() => URand.value > 0.5f ? -1f : 1f;
        public static Vector2 V2RandLerp(Vector2 a, Vector2 b) => Vector2.Lerp(a, b, URand.value);
        public static float NextFloat01(this System.Random r) => (float)(r.NextDouble() / double.MaxValue);
        public static Color Clamped(this Color bcol) => new(Clamp01(bcol.r), Clamp01(bcol.g), Clamp01(bcol.b));
        public static Color RandDev(this Color bcol, Color dbound, bool clamped = true)
        {
            Color res = default;
            for (int i = 0; i < 3; i++) res[i] = bcol[i] + dbound[i] * URand.Range(-1f, 1f);
            return clamped ? res.Clamped() : res;
        }
        #endregion

        #region misc bs
        public static string combinePath(params string[] parts) => parts.Aggregate(Path.Combine);
        public static RainWorld CRW => UnityEngine.Object.FindObjectOfType<RainWorld>();
        public static CreatureTemplate GetCreatureTemplate(CreatureTemplate.Type t) => StaticWorld.creatureTemplates[(int)t];
        public static Vector2 MiddleOfRoom(this Room rm) => new((float)rm.PixelWidth * 0.5f, (float)rm.PixelHeight * 0.5f);
        /// <summary>
        /// Gets bytes from ER of an assembly.
        /// </summary>
        /// <param name="resname">name of the resource</param>
        /// <param name="casm">target assembly. If unspecified, RK asm</param>
        /// <returns>resulting byte array</returns>
        public static byte[] ResourceBytes(string resname, Assembly casm = null)
        {
            if (resname is null) throw new ArgumentNullException("can not get with a null name");
            casm ??= Assembly.GetExecutingAssembly();
            var str = casm.GetManifestResourceStream(resname);
            byte[] bf = (str is null) ? null : new byte[str.Length];
            str?.Read(bf, 0, (int)str.Length);
            return bf;
        }
        /// <summary>
        /// Gets an ER of an assembly and returns it as string. Default encoding is UTF-8
        /// </summary>
        /// <param name="resname">Name of ER</param>
        /// <param name="enc">Encoding. If none is specified, UTF-8</param>
        /// <param name="casm">assembly to get resource from. If unspecified, RK asm.</param> 
        /// <returns>Resulting string. If none is found, <c>null</c> </returns>
        public static string ResourceAsString(string resname, Encoding enc = null, Assembly casm = null)
        {
            enc ??= Encoding.UTF8;
            casm ??= Assembly.GetExecutingAssembly();
            try
            {
                var bf = ResourceBytes(resname, casm);
                return (bf is null) ? null : enc.GetString(bf);
            }
            catch (Exception ee) { PetrifiedWood.WriteLine($"Error getting ER: {ee}"); return null; }
        }
        #endregion

        #region weakdicts
        //moved from M4rbleL1ne's ConditionalEffects
        public static void RemoveWeak<T>(Dictionary<WeakReference, T> dict, RoomSettings.RoomEffect key)
        {
            WeakReference weakKey = null;
            foreach (var pair in dict)
                if (pair.Key.IsAlive && pair.Key.Target == key)
                {
                    weakKey = pair.Key;
                    break;
                }
            if (weakKey != null)
                dict.Remove(weakKey);
        }
        public static bool TryGetWeak<T>(Dictionary<WeakReference, T> dict, RoomSettings.RoomEffect key, out T value)
        {
            foreach (var pair in dict)
                if (pair.Key.IsAlive && pair.Key.Target == key)
                {
                    value = pair.Value;
                    return true;
                }
            value = default;
            return false;
        }
        public static T GetWeak<T>(Dictionary<WeakReference, T> dict, RoomSettings.RoomEffect key)
        {
            foreach (var pair in dict)
                if (pair.Key.IsAlive && pair.Key.Target == key)
                    return pair.Value;
            throw new KeyNotFoundException("Could not get from weak list");
        }
        public static void SetWeak<T>(Dictionary<WeakReference, T> dict, RoomSettings.RoomEffect key, T value)
        {
            foreach (var pair in dict)
                if (pair.Key.IsAlive && pair.Key.Target == key)
                {
                    dict[pair.Key] = value;
                    return;
                }
            dict[new WeakReference(key)] = value;
        }
        #endregion

    }
}
