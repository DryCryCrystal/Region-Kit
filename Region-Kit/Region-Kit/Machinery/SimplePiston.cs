using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static RWCustom.Custom;
using static UnityEngine.Mathf;

namespace RegionKit.Machinery
{
    public class SimplePiston : UpdatableAndDeletable, IDrawable
    {
        public SimplePiston(Room rm, PlacedObject pobj) : this(rm, pobj, null) { }
        public SimplePiston(Room rm, PlacedObject pobj, PistonData mdt = null)
        {
            PO = pobj;
            this._assignedMData = mdt;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            _lt += 1f;
        }

        internal PistonData mData => _assignedMData ?? PO?.data as PistonData;
        private readonly PistonData _assignedMData;
        internal readonly PlacedObject PO;
        private float _lt = 0f;
        internal Vector2 originPoint => PO?.pos ?? _assignedMData?.forcePos ?? default;
        internal float effRot => mData.align ? ((int)mData.rotation / 45 * 45) : mData.rotation;
        internal float Shift
        {
            get
            {
                var res = mData.amplitude;
                
                switch (mData.opmode)
                {
                    case OperationMode.Sinal:
                        res *= Sin((_lt + mData.phase) * mData.frequency);
                        break;
                    case OperationMode.Cosinal:
                        res *= Cos((_lt + mData.phase)* mData.frequency);
                        break;
                }
                return res;
            }
        }

        

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel");
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
#warning add ts smoothing
            var pos = originPoint + DegToVec(effRot) * Shift;
            sLeaser.sprites[0].rotation = effRot;
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = new Color(1f, 0f, 0f);
            sLeaser.sprites[0].scale = 20f;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            foreach (var fs in sLeaser.sprites) fs.RemoveFromContainer();
            (newContatiner ?? rCam.ReturnFContainer("Items")).AddChild(sLeaser.sprites[0]);
        }

        
        }

}
