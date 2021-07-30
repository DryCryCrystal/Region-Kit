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
        public SimplePiston(Room rm, PlacedObject pobj, PistonData mdt = null)// : base (new UselessAbstractPiston(rm.world, rm.GetWorldCoordinate(pobj?.pos ?? mdt?.forcePos ?? default), null))
        {
            PO = pobj;
            this._assignedMData = mdt;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            _lt += room?.world?.rainCycle.brokenAntiGrav?.CurrentLightsOn ?? 1f;
            oldPos = currentPos;
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
                Func<float, float> chosenFunc;
                switch (mData.opmode)
                {
                    default:
                    case OperationMode.Sinal:
                        chosenFunc = Sin;
                        break;
                    case OperationMode.Cosinal:
                        chosenFunc = Cos;
                        break;
                }
                res *= chosenFunc((_lt + mData.phase) * mData.frequency);
                return res;
            }
        }
        internal Vector2 currentPos => originPoint + DegToVec(effRot) * Shift;
        internal Vector2 oldPos;

        internal MachineryCustomizer _mc;
        
        internal void GrabMC()
        {
            _mc = _mc 
                ?? room.roomSettings.placedObjects.FirstOrDefault(
                    x => x.data is MachineryCustomizer nmc && nmc.GetValue<MachineryID>("amID") == MachineryID.Piston && (x.pos - this.PO.pos).sqrMagnitude <= nmc.GetValue<Vector2>("radius").sqrMagnitude)?.data as MachineryCustomizer 
                ?? new MachineryCustomizer(null);
        }

        #region irawable things
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            GrabMC();
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel");
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var pos = Vector2.Lerp(oldPos, currentPos, timeStacker);
            _mc.BringToKin(sLeaser.sprites[0]);
            sLeaser.sprites[0].rotation = effRot + _mc.addRot;
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {

        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            foreach (var fs in sLeaser.sprites) fs.RemoveFromContainer();
            try { (newContatiner ?? rCam.ReturnFContainer(_mc.ContainerName)).AddChild(sLeaser.sprites[0]); }
            catch { rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]); }
            
        }
        #endregion
    }

}
