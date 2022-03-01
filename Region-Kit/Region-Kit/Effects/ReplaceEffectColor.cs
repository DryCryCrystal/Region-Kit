﻿using UnityEngine;
using static RegionKit.Effects.ReplaceEffectColor.EnumExt_ReplaceEffectColor;

namespace RegionKit.Effects
{
    public class ReplaceEffectColor : UpdatableAndDeletable /// By M4rbleL1ne/LB Gamer
    {
        public static class EnumExt_ReplaceEffectColor
        {
            public static RoomSettings.RoomEffect.Type ReplaceEffectColorA;
            public static RoomSettings.RoomEffect.Type ReplaceEffectColorB;
        }

        public ReplaceEffectColor(Room room) => this.room = room;

        internal static void Apply()
        {
            On.RainWorld.Start += delegate(On.RainWorld.orig_Start orig, RainWorld self)
            {
                orig(self);
                ColoredRoomEffect.coloredEffects.Add(ReplaceEffectColorA);
                ColoredRoomEffect.coloredEffects.Add(ReplaceEffectColorB);
            };
            On.Room.Loaded += delegate(On.Room.orig_Loaded orig, Room self)
            {
                orig(self);
                for (int k = 0; k < self.roomSettings.effects.Count; k++)
                {
                    var effect = self.roomSettings.effects[k];
                    if (effect.type == ReplaceEffectColorA || effect.type == ReplaceEffectColorB) self.AddObject(new ReplaceEffectColor(self));
                }
            };
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room?.game != null)
            {
                foreach (var cam in room.game.cameras)
                {
                    if (cam.room?.roomSettings != null)
                    {
                        var a = cam.room.roomSettings.GetEffectAmount(ReplaceEffectColorA);
                        var b = cam.room.roomSettings.GetEffectAmount(ReplaceEffectColorB);
                        var clrar = cam.room.roomSettings.GetColoredEffectRed(ReplaceEffectColorA);
                        var clrbr = cam.room.roomSettings.GetColoredEffectRed(ReplaceEffectColorB);
                        var clrag = cam.room.roomSettings.GetColoredEffectGreen(ReplaceEffectColorA);
                        var clrbg = cam.room.roomSettings.GetColoredEffectGreen(ReplaceEffectColorB);
                        var clrab = cam.room.roomSettings.GetColoredEffectBlue(ReplaceEffectColorA);
                        var clrbb = cam.room.roomSettings.GetColoredEffectBlue(ReplaceEffectColorB);
                        if (cam.room.roomSettings.IsEffectInRoom(ReplaceEffectColorA))
                        {
                            cam.fadeTexA.SetPixels(30, 4, 2, 2, new Color[] { new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a), new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a) }, 0);
                            cam.fadeTexA.SetPixels(30, 12, 2, 2, new Color[] { new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a), new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a) }, 0);
                        }
                        if (cam.room.roomSettings.IsEffectInRoom(ReplaceEffectColorB))
                        {
                            cam.fadeTexA.SetPixels(30, 2, 2, 2, new Color[] { new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b), new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b) }, 0);
                            cam.fadeTexA.SetPixels(30, 10, 2, 2, new Color[] { new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b), new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b) }, 0);
                        }
                        if (cam.paletteB > -1)
                        {
                            if (cam.room.roomSettings.IsEffectInRoom(ReplaceEffectColorA))
                            {
                                cam.fadeTexB.SetPixels(30, 4, 2, 2, new Color[] { new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a), new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a) }, 0);
                                cam.fadeTexB.SetPixels(30, 12, 2, 2, new Color[] { new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a), new Color(clrar, clrag, clrab), new Color(clrar - a, clrag - a, clrab - a) }, 0);
                            }
                            if (cam.room.roomSettings.IsEffectInRoom(ReplaceEffectColorB))
                            {
                                cam.fadeTexB.SetPixels(30, 2, 2, 2, new Color[] { new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b), new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b) }, 0);
                                cam.fadeTexB.SetPixels(30, 10, 2, 2, new Color[] { new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b), new Color(clrbr, clrbg, clrbb), new Color(clrbr - b, clrbg - b, clrbb - b) }, 0);
                            }
                        }
                    }
                    cam.ApplyFade();
                }
            }
        }
    }
}