﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.AridBarrens
{
    static class ABCentral
    {
        public static void Register()
        {
            On.Room.Loaded += AB_RoomloadDetour;
        }
        public static void Disable()
        {
            On.Room.Loaded -= AB_RoomloadDetour;
        }

        public static void AB_RoomloadDetour(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            for (int k = 0; k < self.roomSettings.effects.Count; k++)
            {
                if (self.roomSettings.effects[k].type == EnumExt_ABThing.SandStorm)
                {
                    self.AddObject(new SandStorm(self.roomSettings.effects[k], self));
                }
                else if (self.roomSettings.effects[k].type == EnumExt_ABThing.SandPuffs)
                {
                    self.AddObject(new SandPuffs(self.roomSettings.effects[k], self));
                }
            }
        }
    }
}
