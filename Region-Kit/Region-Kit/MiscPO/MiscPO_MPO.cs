using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;

using static PlacedObjectsManager;

namespace RegionKit.MiscPO
{

    internal class WormgrassRectData : ManagedData
    {
        internal IntVector2 p2 => GetValue<IntVector2>("p2");

        public WormgrassRectData(PlacedObject po) : base(po, new ManagedField[] 
        {
            new IntVector2Field("p2", new IntVector2(3, 3), IntVector2Field.IntVectorReprType.rect)
        })
        {

        }
    }
}