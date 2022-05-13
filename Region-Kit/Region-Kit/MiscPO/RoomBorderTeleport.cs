using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;

using static RegionKit.Utils.RKUtils;
using static System.Math;

namespace RegionKit.MiscPO
{
    internal class RoomBorderTeleport : UpdatableAndDeletable
    {
        public RoomBorderTeleport(PlacedObject owner, Room rm)
        {
            _ow = owner;
        }
        private readonly PlacedObject _ow;
        private BorderTpData btpd => _ow.data as BorderTpData;
        private float buffPX => (float)btpd.buff * 20f;

        public override void Update(bool eu)
        {
            base.Update(eu);
            foreach (var uad in room.updateList)
            {
                if (uad is not PhysicalObject po) continue;
                
                //Vector2 shift = default;
                var rm = room.RoomRect;
                var outer = rm.Grow(buffPX);
                IntVector2 reqshifts = default;
                foreach (var chunk in po.bodyChunks)
                {
                    var cp = chunk.pos;
                    if (cp.x > outer.right) reqshifts.x--;
                    if (cp.x < outer.left) reqshifts.x++;
                    if (cp.y > outer.top) reqshifts.y--;
                    if (cp.y < outer.bottom) reqshifts.y++;
                }
                Vector2 shift = new()
                {
                    x = (Abs(reqshifts.x) == po.bodyChunks.Length) ? (room.PixelWidth + buffPX * 1.5f) * Sign(reqshifts.x) : 0f,
                    y = (Abs(reqshifts.y) == po.bodyChunks.Length) ? (room.PixelHeight + buffPX * 1.5f) * Sign(reqshifts.y) : 0f,
                };
                if (shift is { x:0f, y:0f }) continue;
                foreach (var chunk in po.bodyChunks) chunk.pos += shift;
                if (po.graphicsModule is not null) po.graphicsModule.Reset();
                PetrifiedWood.WriteLine("tp! " + po.firstChunk.pos);
            }
        }
    }
}
