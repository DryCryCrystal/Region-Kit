using System;
using System.Collections.Generic;

namespace RegionKit.EchoExtender {
    public static class ExtensionMethods {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
            try {
                dict.Add(key, value);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static string[] SplitAndREE(this string target, string seperator) => target.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);

        public static void AddMultiple<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, params TKey[] keys) {
            dict.AddMultiple(value, ieKeys: keys);
        }

        public static void AddMultiple<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, IEnumerable<TKey> ieKeys) {
            foreach (TKey key in ieKeys) {
                dict.TryAdd(key, value);
            }
        }
    }
}