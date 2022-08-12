//ShelterBehaviors by Henpemaz

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Permissions;
using RegionKit.POM;

using static RegionKit.POM.PlacedObjectsManager;

namespace RegionKit.SBeh
{
    public static class SBehCentral
    {
        public const string ModID = "ShelterBehaviorsMod";
        public const string breakVer = "1.0";

        internal static bool EnabledOnce = false;

        /// <summary>
        /// Makes creatures <see cref="ShelterBehaviorManager.CycleSpawnPosition"/> on <see cref="AbstractCreature.RealizeInRoom"/>
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="instance"></param>
        public static void CreatureShuffleHook(On.AbstractCreature.orig_RealizeInRoom orig, AbstractCreature instance)
        {
            var mngr = instance.Room.realizedRoom?.updateList?.FirstOrDefault(x => x is ShelterBehaviorManager) as ShelterBehaviorManager;
            mngr?.CycleSpawnPosition();
            orig(instance);

        }
        public static void Enable()
        {
            // Hooking code goose hre
            On.AbstractCreature.RealizeInRoom += CreatureShuffleHook;

            //PlacedObjectsManager.ApplyHooks();
            if (EnabledOnce)
            {

            }
            else
            {
                RegisterFullyManagedObjectType(new ManagedField[]{
                new PlacedObjectsManager.BooleanField("nvd", true, displayName:"No Vanilla Door"),
                new PlacedObjectsManager.BooleanField("htt", false, displayName:"Hold To Trigger"),
                new PlacedObjectsManager.IntegerField("htts", 1, 10, 4, displayName:"HTT Trigger Speed"),
                new PlacedObjectsManager.BooleanField("cs", false, displayName:"Consumable Shelter"),
                new PlacedObjectsManager.IntegerField("csmin", -1, 30, 3, displayName:"Consum. Cooldown Min"),
                new PlacedObjectsManager.IntegerField("csmax", 0, 30, 6, displayName:"Consum. Cooldown Max"),
                new PlacedObjectsManager.IntegerField("ftt", 0, 400, 20, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Frames to Trigger"),
                new PlacedObjectsManager.IntegerField("fts", 0, 400, 40, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Frames to Sleep"),
                new PlacedObjectsManager.IntegerField("ftsv", 0, 400, 60, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Frames to Starvation"),
                new PlacedObjectsManager.IntegerField("ftw", 0, 400, 120, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Frames to Win"),
                new PlacedObjectsManager.IntegerField("ini", 0, 400, 120, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Initial wait"),
                new PlacedObjectsManager.IntegerField("ouf", 0, 400, 120, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, displayName:"Open up anim"),
                new PlacedObjectsManager.BooleanField("ani", false, displayName:"Animate Water"),

                }, typeof(ShelterBehaviorManager), "ShelterBhvrManager");
                RegisterFullyManagedObjectType(new ManagedField[]{
                //new PlacedObjectsManager.BooleanField("httt", false, displayName: "HTT Tutorial"),
                new IntegerField("htttcd", -1, 12, 6, displayName: "HTT Tut. Cooldown"), }
                    , typeof(ShelterBehaviorManager.HoldToTriggerTutorialObject), "ShelterBhvrHTTTutorial");

                //PlacedObjectsManager.RegisterEmptyObjectType("ShelterBhvrPlacedDoor", typeof()) TODO directional data and rep;
                RegisterFullyManagedObjectType(new ManagedField[]{
                new IntVector2Field("dir", new RWCustom.IntVector2(0,1), IntVector2Field.IntVectorReprType.fourdir), }
                , null, "ShelterBhvrPlacedDoor");

                RegisterEmptyObjectType("ShelterBhvrTriggerZone", typeof(PlacedObject.GridRectObjectData), typeof(DevInterface.GridRectObjectRepresentation));
                RegisterEmptyObjectType("ShelterBhvrNoTriggerZone", typeof(PlacedObject.GridRectObjectData), typeof(DevInterface.GridRectObjectRepresentation));
                RegisterEmptyObjectType("ShelterBhvrSpawnPosition", null, null); // No data required :)
            }
        }

        public static void Disable()
        {
            On.AbstractCreature.RealizeInRoom -= CreatureShuffleHook;
        }

        public static class EnumExt_ShelterBehaviorsMod
        {
            public static PlacedObject.Type ShelterBhvrManager;
            public static PlacedObject.Type ShelterBhvrPlacedDoor;
            public static PlacedObject.Type ShelterBhvrTriggerZone;
            public static PlacedObject.Type ShelterBhvrNoTriggerZone;
            public static PlacedObject.Type ShelterBhvrHTTTutorial;
            public static PlacedObject.Type ShelterBhvrSpawnPosition;
        }
    }
}