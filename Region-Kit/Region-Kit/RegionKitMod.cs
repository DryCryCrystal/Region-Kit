//Main mod class.
//Please keep it as slim as possible: bring your object code
//to separate classes and only add hooking calls here.
//Thank you!

//using Partiality.Modloader;
using UnityEngine;
using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using RegionKit.Utils;
using System.Reflection;

using PWood = RegionKit.Utils.PetrifiedWood;
using RegionKit.Objects;

namespace RegionKit
{
    [BepInPlugin("RegionKit", "RegionKit", modVersion + "." + buildVersion)]
    public partial class RegionKitMod : BaseUnityPlugin {

        public const string modVersion = "2.1.1"; //used for assembly version!
        public const string buildVersion = "0"; //Increments for every code change without a version change.

        public void OnEnable() {
            __me = new(this);
            Logger.Log(LogLevel.Info, "running RK ruleset:" + Environment.GetEnvironmentVariable(RKEnv.RKENVKEY));
            //wood setup
            string woodpath = "RegionKitLog.txt";
            try
            {
                if (RKEnv.RulesDet is not null && RKEnv.RulesDet.TryGetValue("RKLogOutput", out var prm))
                {
                    woodpath = prm?.FirstOrDefault();
                }
                var lsf = RKEnv.Rules?.Contains("RKLogFile") ?? false;// ?? false;
                Logger.LogInfo((lsf) ? "Writing RKlogs to file" : "rerouting Rk logs to normal listener");
                PWood.SetNewPathAndErase(woodpath, !lsf);
                //VARIOUS PATCHES
                RoomLoader.Patch();
                SuperstructureFusesFix.Patch();
                ArenaFixes.ApplyHK();
                SunBlockerFix.Apply();
                Effects.GlowingSwimmersCI.Apply();
                MiscPO.NoWallSlideZones.Apply();
                CustomArenaDivisions.Patch();
                EchoExtender.EchoExtender.ApplyHooks();
                LooseSpriteLoader.LoadSprites();
                ConditionalEffects.CECentral.Enable(); //Applies Conditional Effects
                Effects.FogOfWar.Patch();
                MiscPO.LittlePlanet.ApplyHooks();
                MiscPO.RKProjectedCircle.ApplyHooks();
                MiscPO.CustomEntranceSymbols.ApplyHooks();
                PaletteTextInput.Apply();
                CloudAdjustment.Apply();


                bool MastInstalled = false;
                bool ABInstalled = false;
                bool ForsakenStationInstalled = false;
                bool ARInstalled = false;
                bool CGInstalled = false;
                //0 - none, 1 - any, 2 - 1.3 and higher
                byte CSLInstalled = default;
                byte SBehInstalled = default;
                byte EGInstalled = default;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.FullName.Contains("ExtendedGates")) EGInstalled++;
                    if (asm.FullName.Contains("ABThing")) ABInstalled = true;
                    if (asm.FullName.Contains("TheMast")) MastInstalled = true;
                    if (asm.FullName.Contains("ForsakenStation") || asm.FullName.Contains("Forsaken Station") || asm.FullName.Contains("Forsaken_Station")) ForsakenStationInstalled = true;
                    if (asm.FullName.Contains("ARObjects")) ARInstalled = true;
                    if (asm.FullName.Contains("ConcealedGarden")) CGInstalled = true;
                    if (asm.FullName.Contains("ShelterBehaviors")) SBehInstalled++;
                }
                foreach (var mod in Partiality.PartialityManager.Instance.modManager.loadedMods)
                {
                    if (mod.ModID == Sprites.CSLCentral.csl_modid)
                    {
                        CSLInstalled++;
                        if (new Version(mod.Version) >= new Version(Sprites.CSLCentral.breakVer)) CSLInstalled++;
                    }
                    //if (mod.ModID == SBeh.SBehCentral.ModID) SBehInstalled++;
                }
                PWood.WriteLine($"CSL check results: {CSLInstalled}");

                if (!ForsakenStationInstalled)
                {
                    PWood.WriteLine("Forsaken Station.dll not loaded; applying related object hooks.");
                    Effects.ReplaceEffectColor.Apply();
                    Effects.ColoredRoomEffect.Apply();
                }
                else PWood.WriteLine("Forsaken Station.dll loaded; not applying related object hooks.");

                //The Mast
                if (!MastInstalled)
                {
                    PWood.WriteLine("TheMast.dll not loaded; applying related object hooks.");
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
                    PWood.WriteLine("ABThing.dll not loaded; applying related object hooks.");
                    AridBarrens.ABCentral.Register();
                }//On.Room.Loaded += AB_RoomloadDetour;

                if (!ARInstalled)
                {
                    PWood.WriteLine("AR objects not installed; applying related object hooks.");
                    NewObjects.Hook();
                }
                if (!CGInstalled)
                {
                    PWood.WriteLine("ConcealedGarden not installed, applying related hooks");
                    ConcealedGarden.CGDrySpot.Register();
                    ConcealedGarden.CGGateCustomization.Register();
                    ConcealedGarden.CGFourthLayerFix.Apply();
                }
                //henpemods:
                //CSL, extendedgates, shelterbehaviours
                if (RKEnv.RulesDet is not null && RKEnv.RulesDet.TryGetValue("CSLForceState", out prm))
                {
                    byte.TryParse(prm.First(), out CSLInstalled);
                    PWood.WriteLine("Forcing CSL mode: " + CSLInstalled);
                }
                if (CSLInstalled < 2)
                {
                    Sprites.CSLCentral.Enable(CSLInstalled == 1);
                    PWood.WriteLine(CSLInstalled switch
                    {
                        0 => "CSL not installed, full apply",
                        1 => $"found CSL below break ver, only scanning CRS folders",
                        _ => "found CSL equal or greater than self, not enabling"
                    });
                }
                if (EGInstalled == 0)
                {
                    PWood.WriteLine("ExtendedGates not found, applying related hooks");
                    ExtendedGates.Enable();
                }
                if (SBehInstalled == 0)
                {
                    PWood.WriteLine("ShelterBehaviors not installed, applying related hooks");
                    SBeh.SBehCentral.Enable();
                }


                //Objects
                ColouredLightSource.RegisterAsFullyManagedObject();
                Machinery.MachineryStatic.Enable();
                MiscPO.MiscPOStatic.Enable();
                Particles.ParticlesStatic.Enable();
                Drawable.Register();
                SpinningFanObjRep.SpinningFanRep();
                //ShroudObjRep.ShroudRep();
                //Add new things here - remember to add them to OnDisable() as well!
                // Use this to enable the example managedobjecttypes for testing or debugging
                //ManagedObjectExamples.PlacedObjectsExample();
                //scdgeighep
                //EFFECTS
                Effects.PWMalfunction.Patch();
            }
            catch (Exception e)
            {
                Logger.LogError("Error on RK Onenable! " + e);
            }

        }

        public void OnDisable() {
            ExtendedGates.Disable();
            SBeh.SBehCentral.Disable();
            Sprites.CSLCentral.Disable();
            RoomLoader.Disable();
            SuperstructureFusesFix.Disable();
            EchoExtender.EchoExtender.RemoveHooks();
            Machinery.MachineryStatic.Disable();
            //PetrifiedWood.ShutDown();
            PWood.Lifetime = 5;
            MiscPO.MiscPOStatic.Disable();
            Particles.ParticlesStatic.Disable();
            ConditionalEffects.CECentral.Disable();
            //Add new things here- remember to add them to OnEnable() as well!
            var casm = Assembly.GetExecutingAssembly();
            foreach (var t in casm.GetTypes()) if (t != typeof(PWood)) t.CleanUpStatic();
                else {
                    PWood.selfDestruct |= PWood.wrThr?.ThreadState == System.Threading.ThreadState.Running;
                    if (!PWood.selfDestruct) t.CleanUpStatic();
                }
        }

        internal ManualLogSource publog => Logger;

        private static WeakReference __me;
        internal static RegionKitMod ME => __me?.Target as RegionKitMod;

        // Code for AutoUpdate support --------------------
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/7/0";
        // Version - increase this by 1 when you upload a new version of the mod.
        public int version = 15;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "pQTSWONMkz/+cDljDGQPVe33mzBTAjabsB8++ZF7h+5rx65KSpvqviESF8X6tKFZPQBxaQD+JwLK05kSt9lopcUsLe8T+Vxia4HXDnEGmAMuZg477vpib+JCgKP0pAMjwtLiD8GpvbI3kUcxD8qJ3+l6ULCTbT8Z120U2lae22AzMU5Tpz0Yvl/vATv3472roBYe7N9LA5mFaACPT+E+U36/hSoIhtVmIxtbOXCmCod/k4L3/CPDs4w34gb1Vo43GiLLo9jOSXVPhMTMkWHrYWnEWy4tu9Ujcj0KZcuHGylO6MYfV+dSJwdAgkcFuq4plNRHt+pmAnwbI0U5kcd2FlpkI2ihqVShvDyj4v3mFNmd/0YighTcBXmYQj3h06NKup9cPfNCPRdwP9CTNjtLljA+SWkl7z/j+z29lWFuE6a1xNiYZb+GGj4UbExUDgcZ1YFOqSgPeQPeoFGqY5nGBQN0UOv/9GmrdaxxWGDrgkbRL2+L/NwKV2uH8HVBzSu0VBRnTjz3JTzkKTBR+ai0LDfmez7BBvG8giTvgNrHi3LxxvVGUugj9GnbRxnTSY6By8JKSwKkgPztVb+irUPW+1lQv76Gyx6fh/8V/+EbrgKSVUEH/mF/Yg8MDQseRbF7X697ZNcfiHm/dGjV+zNcR8CL0Tvtj2mqdNPH4Eib/qE=";
        // ------------------------------------------------
    }
}
