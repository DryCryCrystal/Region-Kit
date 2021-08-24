using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegionKit.TheMast.Localization;

//Made by Slime_Cubed and Doggo
namespace RegionKit.TheMast
{
    internal static class Translator
    {
        public static string GetString(string name, InGameTranslator.LanguageID language)
        {
            string langName;
            if (language == InGameTranslator.LanguageID.English) langName = "";
            else langName = LocalizationTranslator.LangShort(language) + "-";
            string ret = Strings.ResourceManager.GetString(langName + name);
            if(ret == null)
                ret = Strings.ResourceManager.GetString(name);
            return ret;
        }
    }
}
