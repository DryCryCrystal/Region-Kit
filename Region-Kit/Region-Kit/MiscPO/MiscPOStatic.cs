using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;
using RegionKit.Utils;
using RegionKit.POM;
using UnityEngine;

using static RegionKit.POM.PlacedObjectsManager;

using GHalo = global::TempleGuardGraphics.Halo;
using URand = UnityEngine.Random;
using UDe = UnityEngine.Debug;

namespace RegionKit.MiscPO
{
    internal static class MiscPOStatic
    {
        private static bool EnabledOnce = false;
        private static readonly Type _mt = typeof(MiscPOStatic); 
        internal static void Enable()
        {
            if (!EnabledOnce)
            {
                RegisterMPO();
                GenerateHooks();
            }
            EnabledOnce = true;
            foreach (var hk in mHk) if (!hk.IsApplied) hk.Apply();
            //On.TempleGuardGraphics.Halo.RadAtCircle += halo_racc;
            //On.Room.Loaded += guardcache;
        }



        #region hooks
        internal static AttachedField<Room, TempleGuardGraphics> cachedGuards;
        private static void guardcache(On.Room.orig_Loaded orig, Room self)
        {
            //slightly evil (and nonfunct) abstr hack
            if (self.game == null) { orig(self); return; }
            var phfound = false;
            foreach (var po in self.roomSettings.placedObjects) phfound |= po.data is PlacedHaloData;
            if (phfound)
            {
                AbstractCreature ac = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, self.GetWorldCoordinate(new Vector2(-50000f, -50000f)), new EntityID(-1, (self.abstractRoom.index)));
                ac.realizedCreature = new TempleGuard(ac, self.world);
                ac.realizedCreature.graphicsModule = new TempleGuardGraphics(ac.realizedCreature);
                cachedGuards.Set(self, ac.realizedCreature.graphicsModule as TempleGuardGraphics);
            }
            orig(self);
        }
        internal static AttachedField<GHalo, PlacedHalo> reghalos = new AttachedField<GHalo, PlacedHalo>();
        internal static PlacedHalo chal;

        internal delegate void Room_Void_None(Room instance);
        internal static void Room_Loaded(Room_Void_None orig, Room instance)
        {
            orig(instance);
            if (instance.game == null) return;
            instance.AddObject(new WormgrassManager(instance));
        }
        internal static void Room_NotViewed(Room_Void_None orig, Room instance)
        {
            orig(instance);
            foreach (var uad in instance.updateList) if (uad is INotifyWhenRoomIsViewed tar) tar.RoomNoLongerViewed();
        }
        internal static void Room_Viewed(Room_Void_None orig, Room instance)
        {
            orig(instance);
            foreach (var uad in instance.updateList) if (uad is INotifyWhenRoomIsViewed tar) tar.RoomViewed();
        }

        internal static float halo_speed(
            Func<GHalo, float> orig,
            GHalo self)
        {
            if (reghalos.TryGet(self, out var ph))
            {
                return ph.speed;
            }
            return orig(self);
            //if (reghalos)
        }
        private static float halo_racc(On.TempleGuardGraphics.Halo.orig_RadAtCircle orig, GHalo self, float c, float ts, float dis)
        {
            if (reghalos.TryGet(self, out var ph) || chal != null)
            {
                ph = ph ?? chal;
                //UDe.LogWarning("scrom");
                return ph.RadAtCircle(c, ts, dis);
            }
            return orig(self, c, ts, dis);
        }

        private static List<Hook> mHk;
        private static void GenerateHooks()
        {
            mHk = new List<Hook>
            {
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.Loaded)), _mt.GetMethodAllContexts(nameof(Room_Loaded))),
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.NowViewed)), _mt.GetMethodAllContexts(nameof(Room_Viewed))),
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.NoLongerViewed)), _mt.GetMethodAllContexts(nameof(Room_NotViewed))),
                //new Hook(typeof(GHalo).GetMethodAllContexts("get_Speed"), _mt.GetMethodAllContexts(nameof(halo_speed)))
            };
        }
        #endregion

        private static void RegisterMPO()
        {
            RegisterManagedObject<RoomBorderTeleport, BorderTpData, ManagedRepresentation>("RoomBorderTP");
            RegisterEmptyObjectType<WormgrassRectData, ManagedRepresentation>("WormgrassRect");
            RegisterManagedObject<PlacedWaterFall, PlacedWaterfallData, ManagedRepresentation>("PlacedWaterfall");
            //RegisterManagedObject<PlacedHalo, PlacedHaloData, ManagedRepresentation>("PlacedHalo");
        }

        internal static void Disable()
        {
            foreach (var hk in mHk) if (hk.IsApplied) hk.Undo();
            //On.TempleGuardGraphics.Halo.RadAtCircle -= halo_racc;
            //On.Room.Loaded -= guardcache;
        }
    }
}
