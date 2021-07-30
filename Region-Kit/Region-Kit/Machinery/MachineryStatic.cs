using System;
//  - - - - -
//  machinery module
//  author: thalber
//  unlicense
//  - - - - -

using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RegionKit.Machinery
{
    public static class MachineryStatic
    {
        public static void Enable()
        {
            if (!appliedOnce) { RegisterMPO(); }
            appliedOnce = true;
        }

        public static void Disable()
        {

        }

        private static void RegisterMPO()
        {
            PlacedObjectsManager.RegisterManagedObject(new PlacedObjectsManager.ManagedObjectType("SimplePiston", typeof(SimplePiston), typeof(PistonData), typeof(PlacedObjectsManager.ManagedRepresentation)));
            PlacedObjectsManager.RegisterManagedObject(new PlacedObjectsManager.ManagedObjectType("PistonArray", typeof(PistonArray), typeof(PistonArrayData), typeof(PlacedObjectsManager.ManagedRepresentation)));
            PlacedObjectsManager.RegisterEmptyObjectType("MachineryCustomizer", typeof(MachineryCustomizer), typeof(PlacedObjectsManager.ManagedRepresentation));
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
//  add more machines
//