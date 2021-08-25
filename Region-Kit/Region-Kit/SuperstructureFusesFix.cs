/* Author: Woodensponge and Slime_Cubed */

using System;
using UnityEngine;

namespace RegionKit 
{
    class SuperstructureFusesFix 
    {
        public static void Patch() 
        {
            On.RainWorld.Start += RainWorldOnStart;
        }

        private static void RainWorldOnStart(On.RainWorld.orig_Start orig, RainWorld self) {
            orig(self);
            try {
                On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;
            }
            catch (Exception x) {
                Debug.Log("There was an issue initializing SuperStructureFusesFix:");
                Debug.Log(x);
            }
        }

        public static void Disable() 
        {
            On.SuperStructureFuses.ctor -= SuperStructureFuses_ctor;
        }

        private static void SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, RWCustom.IntRect rect, Room room) 
        {
            self.placedObject = placedObject;
            self.pos = placedObject.pos;
            self.rect = rect;
            self.lights = new float[rect.Width * 2, rect.Height * 2, 5];
            self.depth = 0;
            for (int i = rect.left; i <= rect.right; i++) 
            {
                for (int j = rect.bottom; j <= rect.top; j++) 
                {
                    if (!room.GetTile(i, j).Solid && ((!room.GetTile(i, j).wallbehind) ? 2 : 1) > self.depth) 
                    {
                        self.depth = ((!room.GetTile(i, j).wallbehind) ? 2 : 1);
                    }
                }
            }
            self.broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
            if (room.world.region == null || room.world.region.name != "SS" && room.world.region.name != "UW") 
            {
                self.broken = 1f;
            }
            self.gravityDependent = (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f);
            self.power = 1f;
            self.powerFlicker = 1f;

            //PWMalfunction needs to hook onto this class as well, so this will go here (this one will not call orig)
            Effects.PWMalfunction.SuperStructureFuses_Ctor(orig, self, placedObject, rect, room);

        }
    }
}
