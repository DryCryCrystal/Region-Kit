using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Utils
{
    internal static class RKEnv
    {
        internal const string RKENVKEY = "RW_RKRules";
        internal static string[] Rules
        { get { __c_rules ??= Environment.GetEnvironmentVariable(RKENVKEY)?.Split(';'); return __c_rules; } }
        internal static Dictionary<string, IEnumerable<string>> RulesDet { get
            {
                if (__c_detrules != null) goto cached;
                Dictionary<string, IEnumerable<string>> res = new();
                var kraw = Rules;
                if (kraw == null) return null;
                foreach (var rule in kraw)
                {
                    var spl = rule.Split('=');
                    if (spl.Length == 1) continue;
                    var prm = spl[1].Split(',');
                    res.Add(spl[0], prm);                    
                }
                __c_detrules ??= res;
            cached:
                return __c_detrules;
            } }

        private static string[] __c_rules;
        private static Dictionary<string, IEnumerable<string>> __c_detrules;
    }
}
