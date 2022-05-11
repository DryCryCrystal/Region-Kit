﻿//  - - - - -
//  machinery module
//  author: thalber
//  unlicense
//  - - - - -

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MonoMod.RuntimeDetour;
using RegionKit.Utils;
using RegionKit.POM;

namespace RegionKit.Machinery
{
    /// <summary>
    /// Handles registration of machinery objects.
    /// </summary>
    public static class MachineryStatic
    {
        private static bool appliedOnce = false;
        /// <summary>
        /// Applies hooks and registers MPO.
        /// </summary>
        public static void Enable()
        {
            if (!appliedOnce)
            {
                RegisterMPO();
                GenerateHooks();
            }
            appliedOnce = true;
            
            foreach (var h in MachineryHooks) h.Apply();
        }

        internal static List<Hook> MachineryHooks;

        /// <summary>
        /// Undoes hooks.
        /// </summary>
        public static void Disable()
        {
            foreach (var h in MachineryHooks) h.Undo();
        }

        internal static RainWorld rw;
        #region hooks
        private static void RW_Start(RainWorld_Start orig, RainWorld self)
        {
            orig(self);
            rw = self;
        }
        private delegate void RainWorld_Start(RainWorld self);
        private static void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            orig(self, obj);
            if (ManagersByRoom.TryGetValue(self.GetHashCode(), out var manager) && obj is RoomPowerManager.IRoomPowerModifier rpm) manager.RegisterPowerDevice(rpm);
        }
        private delegate float orig_Room_GetPower(Room self);
        private static void RWG_new(RWG_Ctor orig, RainWorldGame self, ProcessManager manager)
        {
            ManagersByRoom.Clear();
            orig(self, manager);
        }
        private delegate void RWG_Ctor(RainWorldGame self, ProcessManager manager);
        private static float Room_GetPower(orig_Room_GetPower orig, Room self)
        {
            if (ManagersByRoom.TryGetValue(self.GetHashCode(), out var rpm)) return rpm.GetGlobalPower();
            return orig(self);
        }
        private static void GenerateHooks()
        {
            MachineryHooks = new List<Hook>
            {
                new Hook(typeof(RainWorld).GetMethodAllContexts(nameof(RainWorld.Start)), typeof(MachineryStatic).GetMethodAllContexts(nameof(RW_Start))),
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.AddObject)), typeof(MachineryStatic).GetMethodAllContexts(nameof(Room_AddObject))),
                new Hook(typeof(Room).GetPropertyAllContexts(nameof(Room.ElectricPower)).GetGetMethod(), typeof(MachineryStatic).GetMethodAllContexts(nameof(Room_GetPower))),
                new Hook(typeof(RainWorldGame).GetConstructor(new Type[]{ typeof(ProcessManager)}), typeof(MachineryStatic).GetMethodAllContexts(nameof(RWG_new)))
            };
        }
        #endregion

        private static void RegisterMPO()
        {
            PlacedObjectsManager.RegisterManagedObject<SimplePiston, PistonData, PlacedObjectsManager.ManagedRepresentation>("SimplePiston");
            PlacedObjectsManager.RegisterManagedObject<PistonArray, PistonArrayData, PlacedObjectsManager.ManagedRepresentation>("PistonArray");
            PlacedObjectsManager.RegisterEmptyObjectType<MachineryCustomizer, PlacedObjectsManager.ManagedRepresentation>("MachineryCustomizer");
            PlacedObjectsManager.RegisterManagedObject<SimpleCog, SimpleCogData, PlacedObjectsManager.ManagedRepresentation>("SimpleCog");

            PlacedObjectsManager.RegisterManagedObject<RoomPowerManager, PowerManagerData, PlacedObjectsManager.ManagedRepresentation>("PowerManager", true);
        }

        public static readonly Dictionary<int, RoomPowerManager> ManagersByRoom = new Dictionary<int, RoomPowerManager>();

    }


    public static class EnumExt_RKMachinery
    {
        //public static AbstractPhysicalObject.AbstractObjectType abstractPiston;
    }

    /// <summary>
    /// Machinery operation modes.
    /// </summary>
    public enum OperationMode
    {
        Sinal = 2,
        Cosinal = 4,
    }
    /// <summary>
    /// Used as filter for <see cref="MachineryCustomizer"/>
    /// </summary>
    public enum MachineryID
    {
        Piston,
        Cog
    }
}

//  to-do list/idea stash:
//  - integrate with brokenzerog
//  - add more machines
//  - cog array?
//  - sounds
//  - power manager might become performance heavy if there's too many subscribers. consider ways of preventing.