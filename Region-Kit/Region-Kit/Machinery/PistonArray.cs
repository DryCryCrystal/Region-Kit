using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegionKit.Utils;
using UnityEngine;
using static RWCustom.Custom;

namespace RegionKit.Machinery
{
    public class PistonArray : UpdatableAndDeletable
    {
        public PistonArray(Room rm, PlacedObject obj)
        {
            this.PO = obj;
            this.room = rm;
            GeneratePistons();
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game?.devUI == null) return;
            for (int i = 0; i < pistons.Length; i++)
            {
                var pair = pistons[i];
                var ndt = pdByIndex(i);
                ndt.CopyToKin(pair.item1);
            }
        }

        private PlacedObject PO;
        internal PistonArrayData pArrData => PO?.data as PistonArrayData;
        internal Tuple<PistonData, SimplePiston>[] pistons;
        internal Vector2 p1 => PO.pos;
        internal Vector2 p2 => pArrData.GetValue<Vector2>("point2");
        internal float baseDir => VecToDeg(PerpendicularVector(p2 - p1));

        #region child gen by index
        internal PistonData pdByIndex(int index)
        {
            var res = new PistonData(null)
            {
                forcePos = posByIndex(index),
                rotation = baseDir + pArrData.relativeRotation,
                sharpFac = pArrData.sharpFac,
                align = false,
                phase = pArrData.phaseInc * index,
                amplitude = pArrData.amplitude,
                frequency = pArrData.frequency,
                opmode = omByIndex(index),
            };
            return res;
        }

        internal OperationMode omByIndex(int index)
        {
            return OperationMode.Sinal;
            return (index % 2 == 0) ? OperationMode.Cosinal : OperationMode.Sinal;
        }
        internal Vector2 posByIndex(int index)
        {
            return Vector2.Lerp(p1, p1 + p2, (float)index / pArrData.pistonCount);
        }
        internal float rotByIndex(int index) { return baseDir + pArrData.relativeRotation; }
        #endregion


        internal void CleanUpPistons()
        {
            if (pistons != null)
            {
                foreach (var pair in pistons) { pair.item2.Destroy(); }
            }
            pistons = null;
        }
        internal void GeneratePistons()
        {
            CleanUpPistons();
            pistons = new Tuple<PistonData, SimplePiston>[pArrData.pistonCount];
            var cPhase = 0f;
            for (int i = 0; i < pistons.Length; i++)
            {
                cPhase += pArrData.phaseInc;
                var pdata = pdByIndex(i);
                var piston = new SimplePiston(room, null, pdata);
                this.room.AddObject(piston);
                pistons[i] = new Tuple<PistonData, SimplePiston>(pdata, piston);

            }
        }
    }
}
