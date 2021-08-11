﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;
using static System.Math;
using static RWCustom.Custom;

namespace RegionKit.MiscPO
{
    public class WormgrassManager : UpdatableAndDeletable
    {
        public WormgrassManager(Room rm)
        {

        }

        private bool regAlreadyAttempted = false;
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room?.game == null || regAlreadyAttempted) return;
            regAlreadyAttempted = true;
            foreach (var po in room.roomSettings.placedObjects)
            {
                TryRegisterArea(po);
            }
            room.AddObject(new WormGrass(room, tarTiles));
        }
        private bool TryRegisterArea(PlacedObject po)
        {
            if (po.data is WormgrassRectData wgrect)
            {
                var st = (po.pos / 20).ToIV2();
                var ht = st + wgrect.p2;
                for (int i = Min(st.x, ht.x); i < Max(st.x, ht.x); i++)
                {
                    for (int j = Min(st.y, ht.y); j < Max(st.y, ht.y); j++)
                    {
                        TryRegisterTile(new IntVector2(i, j));
                    }
                }
                return true;
            }
            return false;
        }
        private void TryRegisterTile(IntVector2 tile)
        {
            tarTiles.Add(tile);
        }
        private readonly List<IntVector2> tarTiles = new List<IntVector2>();
    }
}
