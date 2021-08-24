﻿using System;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;

//Made by Slime_Cubed and Doggo
namespace RegionKit.TheMast
{
    internal static class WormGrassFix
    {
        public static void Apply()
        {
            // Disable scavenger terrain-clipping protection when in wormgrass
            On.PhysicalObject.Update += PhysicalObject_Update;
            On.Scavenger.Update += Scavenger_Update;

            // Disable water physics when in wormgrass
            new Hook(
                typeof(BodyChunk).GetProperty("submersion", BindingFlags.Public | BindingFlags.Instance).GetGetMethod(),
                typeof(WormGrassFix).GetMethod("BodyChunk_submersion")
            );
        }

        public static float BodyChunk_submersion(Func<BodyChunk, float> orig, BodyChunk self)
        {
            if ((self.owner.room?.world?.name == "TM") && !self.collideWithTerrain) return 0f;
            return orig(self);
        }

        private static bool _clipScavBody;
        private static Vector2 _clipPos;
        private static Vector2 _lastClipPos;
        private static Vector2 _lastLastClipPos;
        private static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
        {
            orig(self, eu);
            if (_clipScavBody)
            {
                BodyChunk mbc = (self as Creature).mainBodyChunk;
                mbc.lastLastPos = _lastLastClipPos;
                mbc.lastPos = _lastClipPos;
                mbc.pos = _clipPos;
                _clipScavBody = false;
            }
        }

        private static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            _clipScavBody = false;
            if(self is Scavenger scav && (self.room?.world?.name == "TM"))
            {
                if (!self.bodyChunks[0].collideWithTerrain)
                {
                    self.bodyChunks[2].collideWithTerrain = false;
                    _clipScavBody = true;
                }
            }
            orig(self, eu);
            if(_clipScavBody)
            {
                BodyChunk mbc = (self as Creature).mainBodyChunk;
                _lastLastClipPos = mbc.lastLastPos;
                _lastClipPos = mbc.lastPos;
                _clipPos = mbc.pos;
            }
        }
    }
}
