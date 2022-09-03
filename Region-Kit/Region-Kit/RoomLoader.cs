using DevInterface;
using UnityEngine;
using RWCustom;
using RegionKit.Objects;

namespace RegionKit
{

    public static class EnumExt_Objects 
    {
        public static PlacedObject.Type PWLightrod;
    }

    public static class EnumExt_Effects
    {
        public static RoomSettings.RoomEffect.Type PWMalfunction;
        public static RoomSettings.RoomEffect.Type FogOfWarSolid;
        public static RoomSettings.RoomEffect.Type FogOfWarDarkened;
        public static RoomSettings.RoomEffect.Type CloudAdjustment;

    }
    class RoomLoader {
        public static void Patch() {
            On.Room.Loaded += Room_Loaded;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.RoomSettingsPage.Refresh += RoomSettingsPage_Refresh;
        }

        private static void RoomSettingsPage_Refresh(On.DevInterface.RoomSettingsPage.orig_Refresh orig, RoomSettingsPage self)
        {
            orig(self);

            Effects.FogOfWar.Refresh(self.owner.room);
        }

        public static void Disable() {
            On.Room.Loaded -= Room_Loaded;
            On.PlacedObject.GenerateEmptyData -= PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep -= ObjectsPage_CreateObjRep;
        }

        public static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
            if (tp == EnumExt_Objects.PWLightrod) {
                bool isNewObject = false;
                if (pObj == null) {
                    isNewObject = true;
                    pObj = new PlacedObject(tp, null) {
                        pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                    self.owner.room.AddObject(new PWLightRod(pObj, self.owner.room));
                }
                PlacedObjectRepresentation rep = new PWLightRodRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString(), isNewObject);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            } else {
                orig(self, tp, pObj);
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
            orig(self);
            if (self.type == EnumExt_Objects.PWLightrod) {
                self.data = new PWLightRodData(self);
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self) {
            orig(self);
            //ManyMoreFixes Patch
            if (self.game == null) { return; }

            Effects.FogOfWar.Refresh(self);

            //Load all the effects
            for (int k = 0; k < self.roomSettings.effects.Count; k++)
            {
                var effect = self.roomSettings.effects[k];

                if (effect.type == EnumExt_Effects.PWMalfunction && self.world.rainCycle.brokenAntiGrav == null)
                {
                    //Directly adds a brokenAntiGraivty to the world
                    self.world.rainCycle.brokenAntiGrav = new AntiGravity.BrokenAntiGravity(self.game.setupValues.gravityFlickerCycleMin, self.game.setupValues.gravityFlickerCycleMax, self.game);
                }
            }

            //Load Objects
            for (int l = 0; l < self.roomSettings.placedObjects.Count; ++l) 
            {
                var obj = self.roomSettings.placedObjects[l];
                if (obj.active) {
                    if (obj.type == EnumExt_Objects.PWLightrod) {
                        self.AddObject(new PWLightRod(obj, self));
                    }
                }
            }
        }
    }
}
