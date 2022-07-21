﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.IO;

namespace RegionKit
{
    internal class CloudAdjustment
    {

        public static class EnumExt_CloudAdjustment
        {
            public static RoomSettings.RoomEffect.Type CloudAdjustment;
        }
        public static void Apply()
        {
            //load values from the Properties file
            On.World.LoadMapConfig += World_LoadMapConfig;

            //set startAltitude and endAltitude if they're adjusted
            On.AboveCloudsView.ctor += AboveCloudsView_ctor;

            //pretty much the actual changes
            On.BackgroundScene.RoomToWorldPos += BackgroundScene_RoomToWorldPos;

            CloudAdjustment.CRS = false;
            Version version = new Version("0.9.43"); 
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "CustomRegions")
                {
                    if (assembly.GetName().Version < version)
                    { Utils.PetrifiedWood.WriteLine("Please update your CRS to use all CloudAdjustment features! v0.9.43 is needed"); }
                    else
                    { CloudAdjustment.CRS = true; }
                }
            }

        }

        private static bool CRS;
        public static float Offset = 0f;
        public static float OffsetMin = -1000f;
        public static float OffsetMax = 1620f;
        public static float startAltitude = 20000f;
        public static float endAltitude = 31400f;

        private static void World_LoadMapConfig(On.World.orig_LoadMapConfig orig, World self, int slugcatNumber)
        {
            //reset to the default values, before attempting to change them
            Offset = 0f;
            OffsetMin = -500f;
            OffsetMax = 1620f;
            startAltitude = 20000f;
            endAltitude = 31400f;

            string Cloudy = CloudSearch(self.region.name);
            if (Cloudy != null)
            {
                CloudAssign(Cloudy);
            }

            orig(self, slugcatNumber);
        }


        //public static float startAltitude = 20000f;
        //public static float endAltitude = 31400f
        //divide these ^ by 20, basically


        private static Vector2 BackgroundScene_RoomToWorldPos(On.BackgroundScene.orig_RoomToWorldPos orig, BackgroundScene self, Vector2 inRoomPos)
        {
            bool NewValue = false;
            Vector2 a = self.room.world.GetAbstractRoom(self.room.abstractRoom.index).mapPos / 3f + new Vector2(10f, 10f);

            if (self.room.game.IsArenaSession)
            {
                for (int k = 0; k < self.room.roomSettings.effects.Count; k++)
                {
                    if (self.room.roomSettings.effects[k].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
                    {
                        a.y = Mathf.Lerp(900, 4000, (float)Math.Pow(self.room.roomSettings.effects[k].amount, 2.5));
                        NewValue = true;
                    }

                }
            }
            else if (self.room.game.IsStorySession)
            {
                for (int k = 0; k < self.room.roomSettings.effects.Count; k++)
                {
                    if (self.room.roomSettings.effects[k].type == RoomSettings.RoomEffect.Type.AboveCloudsView && self.room.roomSettings.effects[k].amount != 1f)
                    {
                        //a.y = Mathf.Lerp(startAltitude / 20f - 100, endAltitude / 10f, (float)Math.Pow(self.room.roomSettings.effects[k].amount, 2.5));
                        a.y += Mathf.Lerp(OffsetMin, OffsetMax, self.room.roomSettings.effects[k].amount);

                        NewValue = true;
                    }

                }
                if (!NewValue && Offset != 0f)
                {
                    a.y += Offset;
                    NewValue = true;
                }
            }

            if (!NewValue) { orig(self, inRoomPos); }

            return a * 20f + inRoomPos - new Vector2((float)self.room.world.GetAbstractRoom(self.room.abstractRoom.index).size.x * 20f,
                (float)self.room.world.GetAbstractRoom(self.room.abstractRoom.index).size.y * 20f) / 2f;

        }

        private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
        {
            orig(self, room, effect);

            if (self.room.game.IsStorySession)
            {
                //if these variables are changed from default, change them

                if (startAltitude != 20000f)
                {
                    self.startAltitude = startAltitude;
                }
                if (endAltitude != 31400f)
                {
                    self.endAltitude = endAltitude;
                }

                self.sceneOrigo = new Vector2(2514f, (self.startAltitude + self.endAltitude) / 2f);
                Debug.Log("Cloud offset is" + Offset);
            }
        }


        /// <summary>
        /// returns the path to the Properties.txt file if it exists, otherwise returns null
        /// </summary>
        public static string CloudSearch(string region)
        {
            if (region == null)
            { return null; }

            string path = null;

            if (CRS)
            {
                foreach (KeyValuePair<string, string> keyValuePair in CustomRegions.Mod.API.ActivatedPacks)
                {
                    path = CustomRegions.Mod.API.BuildPath(keyValuePair.Value, "RegionID", region, "Properties.txt");

                    if (File.Exists(path))
                    { break; }

                    else { path = null; }
                }

            }
            if (path == null)
            {
                path = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                Path.DirectorySeparatorChar,
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                region,
                Path.DirectorySeparatorChar,
                "Properties.txt"
                });
            }
            return path;
        }

        /// <summary>
        /// sets the static members if they're found in the Properties
        /// </summary>
        public static void CloudAssign(string path)
        {


            if (File.Exists(path))
            {
                foreach (string text in File.ReadAllText(path).Split(new string[]
                        { Environment.NewLine }, StringSplitOptions.None))

                {
                    string[] array = System.Text.RegularExpressions.Regex.Split(text, ": ");
                    switch (array[0])
                    {
                        case "CloudOffset":
                            float.TryParse(array[1], out Offset);
                            break;

                        case "CloudSliderMin":
                            float.TryParse(array[1], out OffsetMin);
                            break;

                        case "CloudSliderMax":
                            float.TryParse(array[1], out OffsetMax);
                            break;

                        case "CloudStartAltitude":
                            float.TryParse(array[1], out startAltitude);
                            break;

                        case "CloudEndAltitude":
                            float.TryParse(array[1], out endAltitude);
                            break;
                    }


                }

            }

        }



    }
}
