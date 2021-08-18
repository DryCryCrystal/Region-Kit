using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;
using RegionKit.Utils;

namespace RegionKit.MiscPO
{
    internal static class MiscPOStatic
    {
        private static bool EnabledOnce = false;

        internal static void Enable()
        {
            if (!EnabledOnce)
            {
                RegisterMPO();
                GenerateHooks();
            }
            EnabledOnce = true;
            foreach (var hk in mHk) if (!hk.IsApplied) hk.Apply();
        }

        #region hooks
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

        private static List<Hook> mHk;
        private static void GenerateHooks()
        {
            mHk = new List<Hook>
            {
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.Loaded)), typeof(MiscPOStatic).GetMethodAllContexts(nameof(Room_Loaded))),
                new Hook(typeof(Room).GetMethodAllContexts(nameof(Room.NowViewed)), typeof(MiscPOStatic).GetMethodAllContexts(nameof(Room_Viewed)))
            };
        }
        #endregion

        private static void RegisterMPO()
        {
            PlacedObjectsManager.RegisterEmptyObjectType<WormgrassRectData, PlacedObjectsManager.ManagedRepresentation>("WormgrassRect");
            PlacedObjectsManager.RegisterManagedObject<ConfWaterFall, PlacedWaterfallData, PlacedObjectsManager.ManagedRepresentation>("PlacedWaterfall");
        }

        internal static void Disable()
        {
            foreach (var hk in mHk) if (hk.IsApplied) hk.Undo();
        }
    }
}
