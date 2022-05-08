using System;
using System.Collections.Generic;
using System.Security;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;
using RegionKit.Utils;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RegionKit.Sprites
{
    public static class CustomAtlasLoader
    {
        /// <summary>
        /// Helper for parsing the unity .meta file format
        /// </summary>
        /// <param name="input">a line in .meta format</param>
        /// <returns>string:string pairs for building a dictionary</returns>
        public static KeyValuePair<string, string> MetaEntryToKeyVal(string input)
        {
            if (string.IsNullOrEmpty(input)) return new KeyValuePair<string, string>("", "");
            string[] pieces = input.Split(new char[] { ':' }, 2); // No trim option in framework 3.5
            if (pieces.Length == 0) return new KeyValuePair<string, string>("", "");
            if (pieces.Length == 1) return new KeyValuePair<string, string>(pieces[0].Trim(), "");
            return new KeyValuePair<string, string>(pieces[0].Trim(), pieces[1].Trim());
        }

        /// <summary>
        /// Reads and loads a custom atlas from the file system. Looks for a .png, .txt and .png.meta, but just the png is required.
        /// If only a png is found, it'll load as single-image element of same name. If .txt slicing data is found, will load as atlas with elements, if there's metadata, it'll be applied to the texture settings.
        /// </summary>
        /// <param name="basename">name of the atlas to look for with no extention</param>
        /// <param name="folder">folder to look for the atlas files</param>
        /// <param name="atlasName">optional alternate name for the atlas once loaded</param>
        /// <returns>A reference to the loaded atlas, which is available through futile</returns>
        public static FAtlas ReadAndLoadCustomAtlas(string basename, string folder, string atlasName = null)
        {
            PetrifiedWood.WriteLine("CSL_CAL: Loading atlas " + basename + " from " + folder);
            Texture2D imageData = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            imageData.LoadImage(File.ReadAllBytes(Path.Combine(folder, basename + ".png")));

            Dictionary<string, object> slicerData = null;
            if (File.Exists(Path.Combine(folder, basename + ".txt")))
            {
                PetrifiedWood.WriteLine("CSL_CAL: found slicer data");
                slicerData = File.ReadAllText(Path.Combine(folder, basename + ".txt")).dictionaryFromJson();
            }
            Dictionary<string, string> metaData = null;
            if (File.Exists(Path.Combine(folder, basename + ".png.meta")))
            {
                PetrifiedWood.WriteLine("CSL_CAL: found metadata");
                metaData = File.ReadAllLines(Path.Combine(folder, basename + ".png.meta")).ToList().ConvertAll(MetaEntryToKeyVal).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            atlasName = atlasName ?? basename;
            return CustomAtlasLoader.LoadCustomAtlas(atlasName, imageData, slicerData, metaData);
        }

        /// <summary>
        /// Loads an atlas or single-image into futile
        /// If only a png stream is provided, it'll load as single-image element of same name. If slicing data is provided, will load as atlas with elements, if there's metadata, it'll be applied to the texture settings.
        /// An atlas loaded through this method can overwrite other loaded atlases based on their name, or overwrite single elements from other atlases. If the name colides but it doesn't replace all the elements from the conflicting atlas, the name of the resulting atlas is salted.
        /// </summary>
        /// <param name="atlasName">Name of the atlas</param>
        /// <param name="textureStream">png file/memory stream, required</param>
        /// <param name="slicerStream">atlas slicer data file/memory stream, optional</param>
        /// <param name="metaStream">unity texture metadata file/memory stream, optional</param>
        /// <returns>A reference to the loaded atlas, which is available through futile</returns>
        public static FAtlas LoadCustomAtlas(string atlasName, System.IO.Stream textureStream, System.IO.Stream slicerStream = null, System.IO.Stream metaStream = null)
        {
            try
            {
                Texture2D imageData = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                byte[] bytes = new byte[textureStream.Length];
                textureStream.Read(bytes, 0, (int)textureStream.Length);
                imageData.LoadImage(bytes);
                Dictionary<string, object> slicerData = null;
                if (slicerStream != null)
                {
                    StreamReader sr = new StreamReader(slicerStream, Encoding.UTF8);
                    slicerData = sr.ReadToEnd().dictionaryFromJson();
                }
                Dictionary<string, string> metaData = null;
                if (metaStream != null)
                {
                    StreamReader sr = new StreamReader(metaStream, Encoding.UTF8);
                    metaData = new Dictionary<string, string>(); // Boooooo no linq and no splitlines, shame on you c#
                    for (string fullLine = sr.ReadLine(); fullLine != null; fullLine = sr.ReadLine())
                    {
                        (metaData as IDictionary<string, string>).Add(MetaEntryToKeyVal(fullLine));
                    }
                }

                return LoadCustomAtlas(atlasName, imageData, slicerData, metaData);
            }
            finally
            {
                textureStream.Close();
                slicerStream?.Close();
                metaStream?.Close();
            }
        }

        /// <summary>
        /// Loads an atlas or single-image into futile
        /// If only image data is provided, it'll load as single-image element of same name. If slicing data is provided, will load as atlas with elements, if there's metadata, it'll be applied to the texture settings.
        /// An atlas loaded through this method can overwrite other loaded atlases based on their name, or overwrite single elements from other atlases. If the name colides but it doesn't replace all the elements from the conflicting atlas, the name of the resulting atlas is salted.
        /// </summary>
        /// <param name="atlasName">Name of the atlas</param>
        /// <param name="imageData">texture of the atlas, required</param>
        /// <param name="slicerData">parsed atlas slicer data, optional</param>
        /// <param name="metaData">parsed unity texture metadata, optional</param>
        /// <returns>A reference to the loaded atlas, which is available through futile</returns>
        public static FAtlas LoadCustomAtlas(string atlasName, Texture2D imageData, Dictionary<string, object> slicerData, Dictionary<string, string> metaData)
        {
            // Some defaults, metadata can overwrite
            // common snense
            if (slicerData != null) // sprite atlases are mostly unaliesed
            {
                imageData.anisoLevel = 1;
                imageData.filterMode = 0;
            }
            else // Single-image should clamp
            {
                imageData.wrapMode = TextureWrapMode.Clamp;
            }

            if (metaData != null)
            {
                metaData.TryGetValue("aniso", out string anisoValue);
                if (!string.IsNullOrEmpty(anisoValue) && int.Parse(anisoValue) > -1) imageData.anisoLevel = int.Parse(anisoValue);
                metaData.TryGetValue("filterMode", out string filterMode);
                if (!string.IsNullOrEmpty(filterMode) && int.Parse(filterMode) > -1) imageData.filterMode = (FilterMode)int.Parse(filterMode);
                metaData.TryGetValue("wrapMode", out string wrapMode);
                if (!string.IsNullOrEmpty(wrapMode) && int.Parse(wrapMode) > -1) imageData.wrapMode = (TextureWrapMode)int.Parse(wrapMode);
                // Todo -  the other 100 useless params
            }

            // make singleimage atlas
            FAtlas fatlas = new FAtlas(atlasName, imageData, FAtlasManager._nextAtlasIndex);

            if (slicerData == null) // was actually singleimage
            {
                // Done
                if (Futile.atlasManager.DoesContainAtlas(atlasName))
                {
                    PetrifiedWood.WriteLine("Single-image atlas '" + atlasName + "' being replaced.");
                    Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName); // Unload previous version if present
                }
                if (Futile.atlasManager._allElementsByName.Remove(atlasName)) PetrifiedWood.WriteLine("Element '" + atlasName + "' being replaced with new one from atlas " + atlasName);
                FAtlasManager._nextAtlasIndex++; // is this guy even used
                Futile.atlasManager.AddAtlas(fatlas); // Simple
                return fatlas;
            }

            // convert to full atlas
            fatlas._elements.Clear();
            fatlas._elementsByName.Clear();
            fatlas._isSingleImage = false;


            //ctrl c
            //ctrl v

            Dictionary<string, object> dictionary2 = (Dictionary<string, object>)slicerData["frames"];
            float resourceScaleInverse = Futile.resourceScaleInverse;
            int num = 0;
            foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
            {
                FAtlasElement fatlasElement = new FAtlasElement();
                fatlasElement.indexInAtlas = num++;
                string text = keyValuePair.Key;
                if (Futile.shouldRemoveAtlasElementFileExtensions)
                {
                    int num2 = text.LastIndexOf(".");
                    if (num2 >= 0)
                    {
                        text = text.Substring(0, num2);
                    }
                }
                fatlasElement.name = text;
                IDictionary dictionary3 = (IDictionary)keyValuePair.Value;
                fatlasElement.isTrimmed = (bool)dictionary3["trimmed"];
                if ((bool)dictionary3["rotated"])
                {
                    throw new NotSupportedException("Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the " + fatlas._dataPath + " atlas.");
                }
                IDictionary dictionary4 = (IDictionary)dictionary3["frame"];
                float num3 = float.Parse(dictionary4["x"].ToString());
                float num4 = float.Parse(dictionary4["y"].ToString());
                float num5 = float.Parse(dictionary4["w"].ToString());
                float num6 = float.Parse(dictionary4["h"].ToString());
                Rect uvRect = new Rect(num3 / fatlas._textureSize.x, (fatlas._textureSize.y - num4 - num6) / fatlas._textureSize.y, num5 / fatlas._textureSize.x, num6 / fatlas._textureSize.y);
                fatlasElement.uvRect = uvRect;
                fatlasElement.uvTopLeft.Set(uvRect.xMin, uvRect.yMax);
                fatlasElement.uvTopRight.Set(uvRect.xMax, uvRect.yMax);
                fatlasElement.uvBottomRight.Set(uvRect.xMax, uvRect.yMin);
                fatlasElement.uvBottomLeft.Set(uvRect.xMin, uvRect.yMin);
                IDictionary dictionary5 = (IDictionary)dictionary3["sourceSize"];
                fatlasElement.sourcePixelSize.x = float.Parse(dictionary5["w"].ToString());
                fatlasElement.sourcePixelSize.y = float.Parse(dictionary5["h"].ToString());
                fatlasElement.sourceSize.x = fatlasElement.sourcePixelSize.x * resourceScaleInverse;
                fatlasElement.sourceSize.y = fatlasElement.sourcePixelSize.y * resourceScaleInverse;
                IDictionary dictionary6 = (IDictionary)dictionary3["spriteSourceSize"];
                float left = float.Parse(dictionary6["x"].ToString()) * resourceScaleInverse;
                float top = float.Parse(dictionary6["y"].ToString()) * resourceScaleInverse;
                float width = float.Parse(dictionary6["w"].ToString()) * resourceScaleInverse;
                float height = float.Parse(dictionary6["h"].ToString()) * resourceScaleInverse;
                fatlasElement.sourceRect = new Rect(left, top, width, height);
                fatlas._elements.Add(fatlasElement);
                fatlas._elementsByName.Add(fatlasElement.name, fatlasElement);
            }

            // This currently doesn't remove elements from old atlases, just removes elements from the manager.
            bool nameInUse = Futile.atlasManager.DoesContainAtlas(atlasName);
            if (!nameInUse)
            {
                // remove duplicated elements and add atlas
                foreach (FAtlasElement fae in fatlas._elements)
                {
                    if (Futile.atlasManager._allElementsByName.Remove(fae.name)) PetrifiedWood.WriteLine("Element '" + fae.name + "' being replaced with new one from atlas " + atlasName);
                }
                FAtlasManager._nextAtlasIndex++;
                Futile.atlasManager.AddAtlas(fatlas);
            }
            else
            {
                FAtlas other = Futile.atlasManager.GetAtlasWithName(atlasName);
                bool isFullReplacement = true;
                foreach (FAtlasElement fae in other.elements)
                {
                    if (!fatlas._elementsByName.ContainsKey(fae.name)) isFullReplacement = false;
                }
                if (isFullReplacement)
                {
                    // Done, we're good, unload the old and load the new
                    PetrifiedWood.WriteLine("Atlas '" + atlasName + "' being fully replaced with custom one");
                    Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName); // Unload previous version if present
                    FAtlasManager._nextAtlasIndex++;
                    Futile.atlasManager.AddAtlas(fatlas); // Simple
                }
                else
                {
                    // uuuugh
                    // partially unload the old
                    foreach (FAtlasElement fae in fatlas._elements)
                    {
                        if (Futile.atlasManager._allElementsByName.Remove(fae.name)) PetrifiedWood.WriteLine("Element '" + fae.name + "' being replaced with new one from atlas " + atlasName);
                    }
                    // load the new with a salted name
                    do
                    {
                        atlasName += UnityEngine.Random.Range(0, 9);
                    }
                    while (Futile.atlasManager.DoesContainAtlas(atlasName));
                    fatlas._name = atlasName;
                    FAtlasManager._nextAtlasIndex++;
                    Futile.atlasManager.AddAtlas(fatlas); // Finally
                }
            }
            return fatlas;
        }
    }

}
