using Partiality.Modloader;
//using RegionKit.Objects;
//using RegionKit.Utils;
using UnityEngine;
using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;

//TODO0(DELTATIME): Make logging that can be used for entire project
//TODO0: done but untested. see Utils.PetrifiedWood.
namespace RegionKit {
    public class RegionKitMod : PartialityMod {

        public const string modVersion = "1.2.1";
        public const string buildVersion = "1"; //Increments for every code change without a version change.

        public RegionKitMod() {
            ModID = "RegionKit";
            author = "Substratum Dev Team & More";
            Version = modVersion;
        }

        public override void OnEnable() {
            base.OnEnable();
            Utils.PetrifiedWood.SetNewPathAndErase("RegionKitLog.txt");
            //VARIOUS PATCHES
            RoomLoader.Patch();
            SuperstructureFusesFix.Patch();
            ArenaFixes.ApplyHK();
            CustomArenaDivisions.Patch();
            EchoExtender.EchoExtender.ApplyHooks();
            NewObjects.Hook();
            LooseSpriteLoader.LoadSprites();
            AddHooks(); //Applies Conditional Effects
            bool MastInstalled = false;
            bool ABInstalled = false;
            //bool ARInstalled = false;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.Contains("ABThing")) ABInstalled = true;
                if (asm.FullName.Contains("TheMast")) MastInstalled = true;
                //if (asm.FullName.Contains("ARThings")) ARInstalled = true;
            }

            //The Mast
            if (!MastInstalled)
            {
                Utils.PetrifiedWood.WriteLine("TheMast.dll not loaded; applying related object hooks.");
                TheMast.ArenaBackgrounds.Apply();
                TheMast.BetterClouds.Apply();
                TheMast.DeerFix.Apply();
                TheMast.ElectricGates.Apply();
                TheMast.PearlChains.Apply();
                TheMast.RainThreatFix.Apply();
                TheMast.SkyDandelionBgFix.Apply();
                TheMast.WindSystem.Apply();
                TheMast.WormGrassFix.Apply();
            }

            //Arid Barrens
            if (!ABInstalled)
            {
                Utils.PetrifiedWood.WriteLine("ABThing.dll not loaded; applying related object hooks.");
                AridBarrens.ABCentral.Register();
            }//On.Room.Loaded += AB_RoomloadDetour;

            //Objects
            Objects.ColouredLightSource.RegisterAsFullyManagedObject();
            Machinery.MachineryStatic.Enable();
            MiscPO.MiscPOStatic.Enable();
            Particles.ParticlesStatic.Enable();
            Objects.Drawable.Register();
            SpinningFanObjRep.SpinningFanRep();
            ShroudObjRep.ShroudRep();
            //Add new things here - remember to add them to OnDisable() as well!
            // Use this to enable the example managedobjecttypes for testing or debugging
            //ManagedObjectExamples.PlacedObjectsExample();

            //EFFECTS
            Effects.PWMalfunction.Patch();
        }

        public override void OnDisable() {
            base.OnDisable();
            RoomLoader.Disable();
            SuperstructureFusesFix.Disable();
            EchoExtender.EchoExtender.RemoveHooks();
            Machinery.MachineryStatic.Disable();
            //PetrifiedWood.ShutDown();
            Utils.PetrifiedWood.Lifetime = 5;
            MiscPO.MiscPOStatic.Disable();
            Particles.ParticlesStatic.Disable();
            //Add new things here- remember to add them to OnEnable() as well!
        }

        private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig(self, manager, showRegionSpecificBkg);
            if (TheMast.EnumExt_WindSystem.PlacedWind == 0)
            {
                Debug.Log("EnumExtender is not installed!!!");
                self.pages[0].subObjects.Add(new Menu.MenuLabel(self, self.pages[0], "Region Kit requires EnumExtender!", new Vector2(683f + 20f, 370f - 10f), new Vector2(300f, 50f), false));
            }
        }

        //Arid Barrens by Dracentis
        //public void AB_RoomloadDetour(On.Room.orig_Loaded orig, Room self)
        //{
        //    orig(self);
        //    for (int k = 0; k < self.roomSettings.effects.Count; k++)
        //    {
        //        if (self.roomSettings.effects[k].type == AridBarrens.EnumExt_ABThing.SandStorm)
        //        {
        //            self.AddObject(new AridBarrens.SandStorm(self.roomSettings.effects[k], self));
        //        }
        //        else if (self.roomSettings.effects[k].type == EnumExt_ABThing.SandPuffs)
        //        {
        //            self.AddObject(new SandPuffs(self.roomSettings.effects[k], self));
        //        }
        //    }
        //}


        //Conditional Effects by Slime_Cubed
        public static Dictionary<WeakReference, bool[]> filterFlags = new Dictionary<WeakReference, bool[]>();
        public static Dictionary<WeakReference, float> baseIntensities = new Dictionary<WeakReference, float>();

        public static void AddHooks()
        {
            On.RoomSettings.RoomEffect.ToString += RoomEffect_ToString;
            On.RoomSettings.RoomEffect.FromString += RoomEffect_FromString;
            On.RainWorld.Update += RainWorld_Update;
            On.DevInterface.EffectPanel.ctor += EffectPanel_ctor;
            On.DevInterface.EffectPanel.EffectPanelSlider.NubDragged += EffectPanelSlider_NubDragged;
        }

        public static void RemoveHooks()
        {
            On.RoomSettings.RoomEffect.ToString -= RoomEffect_ToString;
            On.RoomSettings.RoomEffect.FromString -= RoomEffect_FromString;
            On.RainWorld.Update -= RainWorld_Update;
            On.DevInterface.EffectPanel.ctor -= EffectPanel_ctor;
            On.DevInterface.EffectPanel.EffectPanelSlider.NubDragged -= EffectPanelSlider_NubDragged;
        }

        private static void EffectPanelSlider_NubDragged(On.DevInterface.EffectPanel.EffectPanelSlider.orig_NubDragged orig, EffectPanel.EffectPanelSlider self, float nubPos)
        {
            if (TryGetWeak(filterFlags, self.effect, out bool[] flags))
            {
                if ((self.owner.game.StoryCharacter < flags.Length) && (self.owner.game.StoryCharacter >= 0))
                    if (!flags[self.owner.game.StoryCharacter])
                        return;
            }
            orig.Invoke(self, nubPos);
        }

        private static int scanFiltersIndex = 0;
        private static int scanIntensitiesIndex = 0;
        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig.Invoke(self);
            if (filterFlags.Count > 0)
            {
                if (++scanFiltersIndex >= filterFlags.Count) scanFiltersIndex = 0;
                WeakReference key = filterFlags.ElementAt(scanFiltersIndex).Key;
                if (!key.IsAlive)
                    filterFlags.Remove(key);
            }
            if (baseIntensities.Count > 0)
            {
                if (++scanIntensitiesIndex >= baseIntensities.Count) scanIntensitiesIndex = 0;
                WeakReference key = baseIntensities.ElementAt(scanIntensitiesIndex).Key;
                if (!key.IsAlive)
                    baseIntensities.Remove(key);
            }
        }

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
            value = default(T);
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

        private static void RoomEffect_FromString(On.RoomSettings.RoomEffect.orig_FromString orig, RoomSettings.RoomEffect self, string[] s)
        {
            orig.Invoke(self, s);
            try
            {
                if (s.Length > 4)
                {
                    bool[] flags = new bool[3] { false, false, false };
                    SetWeak(filterFlags, self, flags);
                    int bitMask = int.Parse(s[4]);
                    for (int i = 0; i < flags.Length; i++)
                    {
                        if ((bitMask & (1 << i)) > 0)
                            flags[i] = true;
                    }
                }
            }
            catch
            {
                Debug.Log("Wrong syntax effect loaded for filter: " + s[0]);
            }
            RainWorld rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
            if (TryGetWeak(filterFlags, self, out bool[] testFlags))
                if (!testFlags[rw.progression.currentSaveState.saveStateNumber])
                {
                    SetWeak(baseIntensities, self, self.amount);
                    self.amount = 0;
                }
        }

        private static string RoomEffect_ToString(On.RoomSettings.RoomEffect.orig_ToString orig, RoomSettings.RoomEffect self)
        {
            float oldAmount = -1f;
            if (TryGetWeak(baseIntensities, self, out float savedAmount))
            {
                oldAmount = self.amount;
                self.amount = savedAmount;
            }
            string ret = orig.Invoke(self);
            if (TryGetWeak(filterFlags, self, out bool[] flags))
            {
                int bitMask = 0;
                bool allTrue = true;
                for (int i = 0; i < flags.Length; i++)
                    if (!flags[i])
                        allTrue = false;
                    else
                        bitMask |= 1 << i;
                if (!allTrue)
                    ret += "-" + bitMask;
            }
            if (oldAmount != -1f)
                self.amount = oldAmount;
            return ret;
        }

        private static void EffectPanel_ctor(On.DevInterface.EffectPanel.orig_ctor orig, EffectPanel self, DevUI owner, DevUINode parentNode, Vector2 pos, RoomSettings.RoomEffect effect)
        {
            orig.Invoke(self, owner, parentNode, pos, effect);
            if (!TryGetWeak(filterFlags, effect, out _))
                SetWeak(filterFlags, effect, new bool[] { true, true, true });
            self.subNodes.Add(new EffectPanelFilters(owner, "Filter_Toggles", self, new Vector2(5f, 0f + 20f)));
            self.size.y += 8f;
        }

        public class EffectPanelFilters : PositionedDevUINode
        {
            public EffectPanelFilters(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos)
            {
                for (int i = 0; i < 3; i++)
                {
                    FSprite sprite = new FSprite("Circle20", true);
                    sprite.color = PlayerGraphics.SlugcatColor(i);
                    sprite.anchorX = 0f;
                    sprite.anchorY = 0.5f;
                    sprite.scaleX = 6f / 10f;
                    sprite.scaleY = 6f / 10f;
                    fSprites.Add(sprite);
                    Futile.stage.AddChild(sprite);
                }
            }

            public bool lastMouseDown = false;
            public override void Update()
            {
                base.Update();
                if (!lastMouseDown && Input.GetMouseButton(0))
                {
                    RoomSettings.RoomEffect effect = (parentNode as EffectPanel).effect;
                    if (!TryGetWeak(filterFlags, effect, out bool[] filters))
                    {
                        filters = new bool[] { true, true, true };
                        SetWeak(filterFlags, effect, filters);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 lPos = owner.mousePos - fSprites[i].GetPosition();
                        if (lPos.x > 0f && lPos.x < 16f && lPos.y < 8f && lPos.y > -8f)
                        {
                            filters[i] = !filters[i];
                            if (i == owner.game.StoryCharacter)
                            {
                                if (filters[i])
                                {
                                    effect.amount = GetWeak(baseIntensities, effect);
                                    RemoveWeak(baseIntensities, effect);
                                }
                                else
                                {
                                    SetWeak(baseIntensities, effect, effect.amount);
                                    effect.amount = 0f;
                                }
                            }
                            parentNode.Refresh();
                        }
                    }
                }
                lastMouseDown = Input.GetMouseButton(0);
            }

            public override void Refresh()
            {
                base.Refresh();
                TryGetWeak(filterFlags, (parentNode as EffectPanel).effect, out bool[] filters);
                for (int i = 0; i < 3; i++)
                {
                    fSprites[i].color = ((filters == null) || filters[i]) ? PlayerGraphics.SlugcatColor(i) : Color.Lerp(PlayerGraphics.SlugcatColor(i), Color.black, 0.5f);
                    MoveSprite(i, absPos + new Vector2(5f + i * 21f, 10f));
                }
            }
        }
    //End of Conditional Effects

    // Code for AutoUpdate support --------------------
    // This URL is specific to this mod, and identifies it on AUDB.
    public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/7/0";
        // Version - increase this by 1 when you upload a new version of the mod.
        public int version = 11;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "pQTSWONMkz/+cDljDGQPVe33mzBTAjabsB8++ZF7h+5rx65KSpvqviESF8X6tKFZPQBxaQD+JwLK05kSt9lopcUsLe8T+Vxia4HXDnEGmAMuZg477vpib+JCgKP0pAMjwtLiD8GpvbI3kUcxD8qJ3+l6ULCTbT8Z120U2lae22AzMU5Tpz0Yvl/vATv3472roBYe7N9LA5mFaACPT+E+U36/hSoIhtVmIxtbOXCmCod/k4L3/CPDs4w34gb1Vo43GiLLo9jOSXVPhMTMkWHrYWnEWy4tu9Ujcj0KZcuHGylO6MYfV+dSJwdAgkcFuq4plNRHt+pmAnwbI0U5kcd2FlpkI2ihqVShvDyj4v3mFNmd/0YighTcBXmYQj3h06NKup9cPfNCPRdwP9CTNjtLljA+SWkl7z/j+z29lWFuE6a1xNiYZb+GGj4UbExUDgcZ1YFOqSgPeQPeoFGqY5nGBQN0UOv/9GmrdaxxWGDrgkbRL2+L/NwKV2uH8HVBzSu0VBRnTjz3JTzkKTBR+ai0LDfmez7BBvG8giTvgNrHi3LxxvVGUugj9GnbRxnTSY6By8JKSwKkgPztVb+irUPW+1lQv76Gyx6fh/8V/+EbrgKSVUEH/mF/Yg8MDQseRbF7X697ZNcfiHm/dGjV+zNcR8CL0Tvtj2mqdNPH4Eib/qE=";
        // ------------------------------------------------
    }
}
