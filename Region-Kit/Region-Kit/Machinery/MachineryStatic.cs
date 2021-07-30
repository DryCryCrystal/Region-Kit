//  - - - - -
//  machinery module
//  author: thalber
//  unlicense
//  - - - - -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;

namespace RegionKit.Machinery
{
    public static class MachineryStatic
    {
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

        internal static HashSet<Hook> MachineryHooks;

        public static void Disable()
        {
            foreach (var h in MachineryHooks) h.Undo();
        }

        private static void RWC_Hook(On.RainWorld.orig_ctor orig, RainWorld self)
        {
            orig(self);
            rw = self;
        }
        internal static RainWorld rw;

        private static void GenerateHooks()
        {
            MachineryHooks = new HashSet<Hook>
            {
                new Hook(typeof(RainWorld).GetMethod(".ctor"), typeof(MachineryStatic).GetMethod(nameof(RWC_Hook)))
            };
        }
        private static void RegisterMPO()
        {
            PlacedObjectsManager.RegisterManagedObject<SimplePiston, PistonData, PlacedObjectsManager.ManagedRepresentation>("SimplePiston");
            PlacedObjectsManager.RegisterManagedObject<PistonArray, PistonArrayData, PlacedObjectsManager.ManagedRepresentation>("PistonArray");
            PlacedObjectsManager.RegisterEmptyObjectType<MachineryCustomizer, PlacedObjectsManager.ManagedRepresentation>("MachineryCustomizer");
            PlacedObjectsManager.RegisterManagedObject<SimpleCog, SimpleCogData, PlacedObjectsManager.ManagedRepresentation>("SimpleCog");
        }
        private static bool appliedOnce = false;
    }

    public static class EnumExt_RKMachinery
    {
        public static AbstractPhysicalObject.AbstractObjectType abstractPiston;
    }

    public enum OperationMode
    {
        Sinal = 2,
        Cosinal = 4,
    }
    public enum MachineryID
    {
        Piston,
        Cog
    }
}

//  to-do list/idea stash:
//  - integrate with brokenzerog
//  - add more machines
//  - sounds