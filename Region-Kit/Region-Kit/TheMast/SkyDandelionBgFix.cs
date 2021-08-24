﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

//Made by Slime_Cubed and Doggo
namespace RegionKit.TheMast
{
    internal static class SkyDandelionBgFix
    {
        public static void Apply()
        {
            On.SkyDandelions.SkyDandelion.AddToContainer += SkyDandelion_AddToContainer;
        }

        private static RoomSettings.RoomEffect.Type[] _bgEffects = new RoomSettings.RoomEffect.Type[]
        {
            RoomSettings.RoomEffect.Type.AboveCloudsView,
            RoomSettings.RoomEffect.Type.RoofTopView,
            RoomSettings.RoomEffect.Type.VoidSea
        };

        private static bool HasBackgroundScene(Room room)
        {
            for(int i = 0; i < _bgEffects.Length; i++)
                if (room.roomSettings.GetEffect(_bgEffects[i]) != null) return true;
            return false;
        }

        private static void SkyDandelion_AddToContainer(On.SkyDandelions.SkyDandelion.orig_AddToContainer orig, SkyDandelions.SkyDandelion self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (HasBackgroundScene(rCam.room))
            {
                newContatiner = rCam.ReturnFContainer("GrabShaders");
                sLeaser.sprites[0].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[0]);
                if (sLeaser.sprites.Length == 2)
                {
                    sLeaser.sprites[1].RemoveFromContainer();
                    rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[1]);
                }
            }
            else
                orig(self, sLeaser, rCam, newContatiner);
        }
    }
}
