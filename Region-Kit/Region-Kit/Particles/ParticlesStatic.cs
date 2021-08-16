using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace RegionKit.Particles
{
    internal static class ParticlesStatic
    {
        internal static void Enable()
        {
            if (!AppliedOnce)
            {
                RegisterMPO();
            }
            AppliedOnce = true;
        }
        internal static bool AppliedOnce = false;
        internal static void RegisterMPO()
        {
            PlacedObjectsManager.RegisterEmptyObjectType<ParticleVisualCustomizer, PlacedObjectsManager.ManagedRepresentation>("ParticleVisualCustomizer");
            PlacedObjectsManager.RegisterManagedObject<RoomParticleSystem, RectParticleSpawnerData, PlacedObjectsManager.ManagedRepresentation>("RectParticleSpawner");
        }


        internal static void Disable()
        {

        }

        public static void RegisterParticleType<T>()
            where T : GenericParticle
        {
            var pt = typeof(T);
            if (pt.IsAbstract) throw new ArgumentException("Unable to register an abstract particle class!");
            if (partTypes.Contains(pt)) throw new ArgumentException("Can not register the same class twice!");
            partTypes.Add(pt);
        }
        internal static HashSet<Type> partTypes = new HashSet<Type>();
    }
}
