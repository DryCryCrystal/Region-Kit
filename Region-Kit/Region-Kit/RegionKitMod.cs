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

//TODO0(DELTATIME): Make logging that can be used for entire project
//TODO0: done but untested. see Utils.PetrifiedWood.
namespace RegionKit {
    [BepInPlugin("RegionKit", "RegionKit", "1.4")]
    public partial class RegionKitMod : BaseUnityPlugin {

        public const string modVersion = "1.4";
        public const string buildVersion = "1"; //Increments for every code change without a version change.

        public RegionKitMod() {
            //ModID = "RegionKit";
            //author = "Substratum Dev Team & More";
            //Version = modVersion;
        }

        public void OnEnable() {
            //base.OnEnable();
            Utils.PetrifiedWood.SetNewPathAndErase("RegionKitLog.txt");
            //VARIOUS PATCHES
            RoomLoader.Patch();
            SuperstructureFusesFix.Patch();
            ArenaFixes.ApplyHK();
            SunBlockerFix.Apply();
            GlowingSwimmersCI.Apply();
            NoWallSlideZones.Apply();
            CustomArenaDivisions.Patch();
            EchoExtender.EchoExtender.ApplyHooks();
            LooseSpriteLoader.LoadSprites();
            ConditionalEffects.CECentral.Enable(); //Applies Conditional Effects
            bool MastInstalled = false;
            bool ABInstalled = false;
            bool ForsakenStationInstalled = false;
            bool ARInstalled = false;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.Contains("ABThing")) ABInstalled = true;
                if (asm.FullName.Contains("TheMast")) MastInstalled = true;
                if (asm.FullName.Contains("ForsakenStation") || asm.FullName.Contains("Forsaken Station") || asm.FullName.Contains("Forsaken_Station")) ForsakenStationInstalled = true;
                if (asm.FullName.Contains("ARObjects")) ARInstalled = true;
            }

            if (!ForsakenStationInstalled)
            {
                Utils.PetrifiedWood.WriteLine("Forsaken Station.dll not loaded; applying related object hooks.");
                Effects.ReplaceEffectColor.Apply();
                Effects.ColoredRoomEffect.Apply();
            }
            else Utils.PetrifiedWood.WriteLine("Forsaken Station.dll loaded; not applying related object hooks.");

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

            if (!ARInstalled)
            {
                NewObjects.Hook();
            }

            //Objects
            Objects.ColouredLightSource.RegisterAsFullyManagedObject();
            Machinery.MachineryStatic.Enable();
            MiscPO.MiscPOStatic.Enable();
            Particles.ParticlesStatic.Enable();
            Objects.Drawable.Register();
            SpinningFanObjRep.SpinningFanRep();
            //ShroudObjRep.ShroudRep();
            //Add new things here - remember to add them to OnDisable() as well!
            // Use this to enable the example managedobjecttypes for testing or debugging
            //ManagedObjectExamples.PlacedObjectsExample();

            //EFFECTS
            Effects.PWMalfunction.Patch();
        }

        public void OnDisable() {

            //base.OnDisable();
            RoomLoader.Disable();
            SuperstructureFusesFix.Disable();
            EchoExtender.EchoExtender.RemoveHooks();
            Machinery.MachineryStatic.Disable();
            //PetrifiedWood.ShutDown();
            Utils.PetrifiedWood.Lifetime = 5;
            MiscPO.MiscPOStatic.Disable();
            Particles.ParticlesStatic.Disable();
            ConditionalEffects.CECentral.Disable();
            //Add new things here- remember to add them to OnEnable() as well!
        }

        //private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        //{
        //    orig(self, manager, showRegionSpecificBkg);
        //    if (TheMast.EnumExt_WindSystem.PlacedWind == 0)
        //    {
        //        Debug.Log("EnumExtender is not installed!!!");
        //        self.pages[0].subObjects.Add(new Menu.MenuLabel(self, self.pages[0], "Region Kit requires EnumExtender!", new Vector2(683f + 20f, 370f - 10f), new Vector2(300f, 50f), false));
        //    }
        //}

        // Code for AutoUpdate support --------------------
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/7/0";
        // Version - increase this by 1 when you upload a new version of the mod.
        public int version = 12;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "pQTSWONMkz/+cDljDGQPVe33mzBTAjabsB8++ZF7h+5rx65KSpvqviESF8X6tKFZPQBxaQD+JwLK05kSt9lopcUsLe8T+Vxia4HXDnEGmAMuZg477vpib+JCgKP0pAMjwtLiD8GpvbI3kUcxD8qJ3+l6ULCTbT8Z120U2lae22AzMU5Tpz0Yvl/vATv3472roBYe7N9LA5mFaACPT+E+U36/hSoIhtVmIxtbOXCmCod/k4L3/CPDs4w34gb1Vo43GiLLo9jOSXVPhMTMkWHrYWnEWy4tu9Ujcj0KZcuHGylO6MYfV+dSJwdAgkcFuq4plNRHt+pmAnwbI0U5kcd2FlpkI2ihqVShvDyj4v3mFNmd/0YighTcBXmYQj3h06NKup9cPfNCPRdwP9CTNjtLljA+SWkl7z/j+z29lWFuE6a1xNiYZb+GGj4UbExUDgcZ1YFOqSgPeQPeoFGqY5nGBQN0UOv/9GmrdaxxWGDrgkbRL2+L/NwKV2uH8HVBzSu0VBRnTjz3JTzkKTBR+ai0LDfmez7BBvG8giTvgNrHi3LxxvVGUugj9GnbRxnTSY6By8JKSwKkgPztVb+irUPW+1lQv76Gyx6fh/8V/+EbrgKSVUEH/mF/Yg8MDQseRbF7X697ZNcfiHm/dGjV+zNcR8CL0Tvtj2mqdNPH4Eib/qE=";
        // ------------------------------------------------
    }
}
