using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using RegionKit.POM;

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
            PlacedObjectsManager.RegisterEmptyObjectType<ParticleBehaviourProvider.WavinessProvider, PlacedObjectsManager.ManagedRepresentation>("ParticleWaviness");
            PlacedObjectsManager.RegisterEmptyObjectType<ParticleBehaviourProvider.SpinProvider, PlacedObjectsManager.ManagedRepresentation>("ParticleSpin");
            PlacedObjectsManager.RegisterEmptyObjectType<ParticleBehaviourProvider.PlainModuleRegister, PlacedObjectsManager.ManagedRepresentation>("GenericPBMDispenser");
            PlacedObjectsManager.RegisterManagedObject<RoomParticleSystem, RectParticleSpawnerData, PlacedObjectsManager.ManagedRepresentation>("RectParticleSpawner");
            PlacedObjectsManager.RegisterManagedObject<RoomParticleSystem, OffscreenSpawnerData, PlacedObjectsManager.ManagedRepresentation>("OffscreenParticleSpawner");
            PlacedObjectsManager.RegisterManagedObject<RoomParticleSystem, WholeScreenSpawnerData, PlacedObjectsManager.ManagedRepresentation>("WholeScreenSpawner");
        }
        internal static void Disable()
        {

        }
    }
}
