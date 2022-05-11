using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


public static class LooseSpriteLoader
{
    public static void LoadSprites()
    {
        On.RainWorld.LoadResources += RainWorld_LoadResources;
    }

    private static void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
    {
        orig.Invoke(self);

        Assembly asm = Assembly.GetExecutingAssembly();
        string[] resources = asm.GetManifestResourceNames();
        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i].StartsWith("RegionKit.Sprites."))
            {
                string spriteName = Regex.Split(resources[i], "\\.")[2];
                if (!Futile.atlasManager.DoesContainAtlas(spriteName))
                {
                    Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    {
                        Stream atlasImage = asm.GetManifestResourceStream(resources[i]);
                        byte[] data = new byte[atlasImage.Length];
                        atlasImage.Read(data, 0, data.Length);
                        tex.LoadImage(data);
                        tex.filterMode = FilterMode.Point;
                    }
                    Futile.atlasManager.LoadAtlasFromTexture(spriteName, tex);
                    //PetrifiedWood.WriteLine("RegionKit: Loaded loose sprite - " + spriteName);
                }
            }
        }
    }
}

