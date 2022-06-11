//CustomSpritesLoader by Henpemaz, adapted by Thalber

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Security;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using UnityEngine;
using System.IO;
using RegionKit.Utils;

namespace RegionKit.Sprites
{
    internal static class CSLCentral
    {
        internal const string csl_modid = "CSL";
        internal const string breakVer = "1.3";
        internal static string description => RKUtils.ResourceAsString("RegionKit.Resources.CSLDesc.txt") ?? "grug";

        internal static bool CRSOnlyMode;
        public static void Enable(bool CRSOnly = true)
        {
            CRSOnlyMode = CRSOnly;
            CheckMyFolders();

            //initialLoadLock = true;
            On.RainWorld.LoadResources += RainWorld_LoadResources_hk;
            //On.FAtlasManager.ActuallyUnloadAtlasOrImage += FAtlasManager_ActuallyUnloadAtlasOrImage;
            On.FAtlasManager.AddAtlas += FAtlasManager_AddAtlas_fix;

            On.FAtlasManager.LoadAtlasFromTexture_string_Texture += FAtlasManager_LoadAtlasFromTexture;
            On.FAtlasManager.LoadAtlasFromTexture_string_string_Texture += FAtlasManager_LoadAtlasFromTexture_1;
            On.FAtlasManager.LoadAtlas += FAtlasManager_LoadAtlas;
            On.FAtlasManager.LoadImage += FAtlasManager_LoadImage;
            On.FAtlasManager.ActuallyLoadAtlasOrImage += FAtlasManager_ActuallyLoadAtlasOrImage;

            // if menuilustration loads image with different dimentions, patch up menuilustration
            On.Menu.MenuIllustration.LoadFile_string += MenuIllustration_LoadFile_1;
        }

        public static void Disable()
        {

            //initialLoadLock = true;
            On.RainWorld.LoadResources -= RainWorld_LoadResources_hk;
            //On.FAtlasManager.ActuallyUnloadAtlasOrImage -= FAtlasManager_ActuallyUnloadAtlasOrImage;
            On.FAtlasManager.AddAtlas -= FAtlasManager_AddAtlas_fix;

            On.FAtlasManager.LoadAtlasFromTexture_string_Texture -= FAtlasManager_LoadAtlasFromTexture;
            On.FAtlasManager.LoadAtlasFromTexture_string_string_Texture -= FAtlasManager_LoadAtlasFromTexture_1;
            On.FAtlasManager.LoadAtlas -= FAtlasManager_LoadAtlas;
            On.FAtlasManager.LoadImage -= FAtlasManager_LoadImage;
            On.FAtlasManager.ActuallyLoadAtlasOrImage -= FAtlasManager_ActuallyLoadAtlasOrImage;

            // if menuilustration loads image with different dimentions, patch up menuilustration
            On.Menu.MenuIllustration.LoadFile_string -= MenuIllustration_LoadFile_1;
        }

        private static void MenuIllustration_LoadFile_1(On.Menu.MenuIllustration.orig_LoadFile_string orig, Menu.MenuIllustration self, string folder)
        {
            orig(self, folder);

            if (DoIHaveAReplacementForThis(self.fileName))
            {
                // self asign newly loaded texture, original was loaded through WWW and screw interfering with that
                var tex = Futile.atlasManager.GetAtlasWithName(self.fileName).texture;
                if (tex is Texture2D tex2d) self.texture = tex2d;
            }
        }

        private static FAtlas FAtlasManager_ActuallyLoadAtlasOrImage(On.FAtlasManager.orig_ActuallyLoadAtlasOrImage orig, FAtlasManager self, string name, string imagePath, string dataPath)
        {
            FAtlas replacement = TryLoadReplacement(name);
            if (replacement != null) return replacement;
            return orig(self, name, imagePath, dataPath);
        }

        private static FAtlas FAtlasManager_LoadImage(On.FAtlasManager.orig_LoadImage orig, FAtlasManager self, string imagePath)
        {
            FAtlas replacement = TryLoadReplacement(imagePath);
            if (replacement != null) return replacement;
            return orig(self, imagePath);
        }

        private static FAtlas FAtlasManager_LoadAtlas(On.FAtlasManager.orig_LoadAtlas orig, FAtlasManager self, string atlasPath)
        {
            FAtlas replacement = TryLoadReplacement(atlasPath);
            if (replacement != null) return replacement;
            return orig(self, atlasPath);
        }

        private static FAtlas FAtlasManager_LoadAtlasFromTexture_1(On.FAtlasManager.orig_LoadAtlasFromTexture_string_string_Texture orig, FAtlasManager self, string name, string dataPath, Texture texture)
        {
            FAtlas replacement = TryLoadReplacement(name);
            if (replacement != null)
            {
                CopyTextureSettingsToAtlas(texture, replacement);
                return replacement;
            }
            return orig(self, name, dataPath, texture);
        }

        private static FAtlas FAtlasManager_LoadAtlasFromTexture(On.FAtlasManager.orig_LoadAtlasFromTexture_string_Texture orig, FAtlasManager self, string name, Texture texture)
        {
            FAtlas replacement = TryLoadReplacement(name);
            if (replacement != null)
            {
                CopyTextureSettingsToAtlas(texture, replacement);
                return replacement;
            }
            return orig(self, name, texture);
        }

        private static void CopyTextureSettingsToAtlas(Texture from, FAtlas to)
        {
            if (from == null || to == null || to.texture == null) return;
            to._texture.wrapMode = from.wrapMode;
            to._texture.anisoLevel = from.anisoLevel;
            to._texture.filterMode = from.filterMode;
            // more ?
        }

        public static FAtlas TryLoadReplacement(string atlasname)
        {
            // if requested atlas already in memory, use instead of reading replacement from disk again
            var alreadyLoaded = Futile.atlasManager.GetAtlasWithName(atlasname);
            if (alreadyLoaded != null)
            {
                PetrifiedWood.WriteLine("texture : " + atlasname + "already loaded");
                return alreadyLoaded;
            }

            if (atlasname.StartsWith("Atlases/"))
            {
                atlasname = atlasname.Substring(8);
                if (!DoIHaveAReplacementForThis(atlasname)) return null;
                if (!ShouldAtlasBeLoadedWithPrefix(atlasname)) knownPrefixedAtlases.Add(atlasname);
            }
            else if (!DoIHaveAReplacementForThis(atlasname)) return null;
            try
            {
                PetrifiedWood.WriteLine("CSL: Loading replacement for " + atlasname);
                string actualatlasname = ShouldAtlasBeLoadedWithPrefix(atlasname) ? "Atlases/" + atlasname : atlasname;
                return CustomAtlasLoader.ReadAndLoadCustomAtlas(atlasname, new FileInfo(knownAtlasReplacements[atlasname]).DirectoryName, actualatlasname);
            }
            catch (Exception e)
            {
                PetrifiedWood.WriteLine("CSL: Error loading replacement atlas " + atlasname + ", skipping");
                PetrifiedWood.WriteLine(e);
                return null;
            }
        }

        public static bool DoIHaveAReplacementForThis(string atlasname)
        {
            //if (initialLoadLock) return false;
            if (knownAtlasReplacements.ContainsKey(atlasname)) return true;
            return false;
        }

        //List<string> knownAtlasReplacements = new List<string>();
        static Dictionary<string, string> knownAtlasReplacements = new Dictionary<string, string>();

        private static void CheckMyFolders()
        {
            if (CRSOnlyMode)
            {
                PetrifiedWood.WriteLine("CSL Running in CRS-only mode. Ignoring own dirs");
            }
            else
            {
                PetrifiedWood.WriteLine("CSL Running full folder scan.");
                Directory.CreateDirectory(CustomSpritesLoaderFolder);
                FileInfo readme = new FileInfo(Path.Combine(CustomSpritesLoaderFolder, "Readme.txt"));
                if (!readme.Exists || readme.Length != description.Length)
                {
                    StreamWriter readmeWriter = readme.CreateText();
                    readmeWriter.Write(description);
                    readmeWriter.Flush();
                    readmeWriter.Close();
                }
                Directory.CreateDirectory(LoadAtlasesFolder);
                File.Create(Path.Combine(LoadAtlasesFolder, "Place new atlases to be automatically loaded here"));
                Directory.CreateDirectory(ReplaceAtlasesFolder);
                File.Create(Path.Combine(ReplaceAtlasesFolder, "Place full atlas or image replacements here"));

                CheckFolder(ReplaceAtlasesFolder);
            }
            
        }

        private static void CheckFolder(string folderToCheck)
        {
            PetrifiedWood.WriteLine("CSL: Scanning for atlas replacements");
            DirectoryInfo atlasesFolder = new DirectoryInfo(folderToCheck);
            FileInfo[] atlasFiles = atlasesFolder.GetFiles("*.png", SearchOption.AllDirectories);
            foreach (FileInfo atlasFile in atlasFiles)
            {
                if (IsDirectoryDisabled(atlasFile, atlasesFolder)) continue;
                if (!atlasFile.Name.EndsWith(".png")) continue; // fake results ffs
                string basename = atlasFile.Name.Substring(0, atlasFile.Name.Length - 4); // remove .png
                knownAtlasReplacements.Add(basename, atlasFile.FullName);
                PetrifiedWood.WriteLine("CSL: Atlas replacement " + basename + " registered");
            }
            PetrifiedWood.WriteLine("CSL: Done scanning");
        }

        private static bool IsDirectoryDisabled(FileInfo file, DirectoryInfo root)
        {
            DirectoryInfo parentDir = file.Directory;
            while (String.Compare(parentDir.FullName.TrimEnd('\\'), root.FullName.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (parentDir.Name.StartsWith("_") || File.Exists(Path.Combine(parentDir.FullName, "disabled.txt")) || File.Exists(Path.Combine(parentDir.FullName, "disabled.txt.txt")) || File.Exists(Path.Combine(parentDir.FullName, "disabled"))) return true;
                parentDir = parentDir.Parent;
            }
            return false;
        }

        private static void FAtlasManager_AddAtlas_fix(On.FAtlasManager.orig_AddAtlas orig, FAtlasManager self, FAtlas atlas)
        {
            // Prevent elements being overwritten.
            List<KeyValuePair<string, FAtlasElement>> duplicates = new List<KeyValuePair<string, FAtlasElement>>();
            foreach (KeyValuePair<string, FAtlasElement> entry in atlas._elementsByName)
            {
                if (self._allElementsByName.ContainsKey(entry.Key)) // clash
                {
                    duplicates.Add(entry);
                }
            }
            foreach (KeyValuePair<string, FAtlasElement> dupe in duplicates)
            {
                PetrifiedWood.WriteLine("CSL: Preventing duplicate element '" + dupe.Key + "' from being loaded");
                atlas._elements.Remove(dupe.Value);
                atlas._elementsByName.Remove(dupe.Key);
            }

            orig(self, atlas);
        }

        //private void FAtlasManager_ActuallyUnloadAtlasOrImage(On.FAtlasManager.orig_ActuallyUnloadAtlasOrImage orig, FAtlasManager self, string name)
        //{
        //    if (loadedCustomAtlases.Contains(name)) return; // Prevent unloading of custom loaded assets :/
        //    orig(self, name);
        //}

        private static bool ShouldAtlasBeLoadedWithPrefix(string atlasname)
        {
            if (knownPrefixedAtlases.Contains(atlasname)) return true;
            return false;
        }

        /// <summary>
        /// Atlases that get the "Atlases/" prefix
        /// </summary>
        public static List<string> knownPrefixedAtlases = new List<string> {
            "outPostSkulls",
            "rainWorld",
            "fontAtlas",
            "uiSprites",
            "shelterGate",
            "regionGate",
            "waterSprites"
        };

        private static string _customSpritesLoaderFolder;
        public static string CustomSpritesLoaderFolder => _customSpritesLoaderFolder != null ? _customSpritesLoaderFolder : _customSpritesLoaderFolder = string.Concat(new object[]
        {
            RWCustom.Custom.RootFolderDirectory(),
            "ModConfigs",
            Path.DirectorySeparatorChar,
            csl_modid
        });

        private static string _loadAtlasesFolder;
        public static string LoadAtlasesFolder => _loadAtlasesFolder != null ? _loadAtlasesFolder : _loadAtlasesFolder = string.Concat(new object[]
        {
            CustomSpritesLoaderFolder,
            Path.DirectorySeparatorChar,
            "Load"
        });
        private static string _replaceAtlasesFolder;
        public static string ReplaceAtlasesFolder => _replaceAtlasesFolder != null ? _replaceAtlasesFolder : _replaceAtlasesFolder = string.Concat(new object[]
        {
            CustomSpritesLoaderFolder,
            Path.DirectorySeparatorChar,
            "Replace"
        });

        private static void RainWorld_LoadResources_hk(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            try
            {
                TryCheckCRSFolders();
            }
            catch { }

            orig(self);
            if (!CRSOnlyMode) LoadCustomAtlases(LoadAtlasesFolder);

            try
            {
                // try load from CRS packs
                TryLoadCRSAtlases();
            }
            catch { }
        }

        private static void TryCheckCRSFolders()
        {
            try
            {
                foreach (KeyValuePair<string, string> keyValues in CustomRegions.Mod.CustomWorldMod.activatedPacks)
                {
                    PetrifiedWood.WriteLine("CSL: Checking pack " + keyValues.Key);
                    var dir = new DirectoryInfo(CustomRegions.Mod.CRExtras.BuildPath(keyValues.Value, CustomRegions.Mod.CRExtras.CustomFolder.Assets, folder: "Replace"));
                    if (dir.Exists)
                    {
                        CheckFolder(dir.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                PetrifiedWood.WriteLine("CSL: Error checking CRS folders!" + e);
            }
        }

        private static void TryLoadCRSAtlases()
        {
            try
            {
                foreach (KeyValuePair<string, string> keyValues in CustomRegions.Mod.CustomWorldMod.activatedPacks)
                {
                    PetrifiedWood.WriteLine("CSL: Checking pack " + keyValues.Key);
                    var dir = new DirectoryInfo(CustomRegions.Mod.CRExtras.BuildPath(keyValues.Value, CustomRegions.Mod.CRExtras.CustomFolder.Assets, folder: "Load"));
                    if (dir.Exists)
                    {
                        LoadCustomAtlases(dir.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                PetrifiedWood.WriteLine("CSL: Error loading CRS atlases! " + e);
            }
        }

        public static KeyValuePair<string, string> MetaEntryToKeyVal(string input)
        {
            if (string.IsNullOrEmpty(input)) return new KeyValuePair<string, string>("", "");
            string[] pieces = input.Split(new char[] { ':' }, 2); // No trim option in framework 3.5
            if (pieces.Length == 0) return new KeyValuePair<string, string>("", "");
            if (pieces.Length == 1) return new KeyValuePair<string, string>(pieces[0].Trim(), "");
            return new KeyValuePair<string, string>(pieces[0].Trim(), pieces[1].Trim());
        }

        private static void LoadCustomAtlases(string folderToLoadFrom)
        {
            PetrifiedWood.WriteLine("CSL: LoadCustomAtlases from folder " + folderToLoadFrom);
            DirectoryInfo atlasesFolder = new DirectoryInfo(folderToLoadFrom);
            FileInfo[] atlasFiles = atlasesFolder.GetFiles("*.png", SearchOption.AllDirectories);
            foreach (FileInfo atlasFile in atlasFiles)
            {
                if (IsDirectoryDisabled(atlasFile, atlasesFolder)) continue;
                if (!atlasFile.Name.EndsWith(".png")) continue; // fake results ffs
                try
                {
                    string basename = atlasFile.Name.Substring(0, atlasFile.Name.Length - 4); // remove .png
                    string atlasname = ShouldAtlasBeLoadedWithPrefix(basename) ? "Atlases/" + basename : basename;
                    CustomAtlasLoader.ReadAndLoadCustomAtlas(basename, atlasFile.Directory.FullName, atlasname);
                }
                catch (Exception e)
                {
                    PetrifiedWood.WriteLine("CSL: Error loading custom atlas data for file " + atlasFile.Name + ", skipping");
                    PetrifiedWood.WriteLine(e);
                }
            }
            //initialLoadLock = false;
        }
    }
}
