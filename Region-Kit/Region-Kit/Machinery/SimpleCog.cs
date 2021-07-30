using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static UnityEngine.Mathf;
namespace RegionKit.Machinery
{
    public class SimpleCog : UpdatableAndDeletable, IDrawable
    {
        public SimpleCog(Room rm, PlacedObject pobj) : this(rm, pobj, null) { }
        public SimpleCog(Room rm, PlacedObject pobj, SimpleCogData acd = null)
        {
            PO = pobj;
            _assignedCD = acd;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            _lt += room?.world?.rainCycle.brokenAntiGrav?.CurrentLightsOn ?? 1f;
            lastRot = rot;
            rot = (rot + cAngVel) % 360f;
        }

        internal SimpleCogData cData => PO?.data as SimpleCogData ?? _assignedCD ?? new SimpleCogData(null);
        private readonly SimpleCogData _assignedCD;
        private PlacedObject PO;

        internal MachineryCustomizer _mc;
        internal void GrabMC()
        {
            _mc = _mc
                ?? room.roomSettings.placedObjects.FirstOrDefault(
                    x => x.data is MachineryCustomizer nmc && nmc.GetValue<MachineryID>("amID") == MachineryID.Cog && (x.pos - this.PO.pos).sqrMagnitude <= nmc.GetValue<Vector2>("radius").sqrMagnitude)?.data as MachineryCustomizer
                ?? new MachineryCustomizer(null);
        }

        private float _lt;
        internal float lastRot;
        internal float rot;
        internal float cAngVel
        {
            get
            {
                var res = cData.baseAngVel;
                Func<float, float> targetFunc;
                switch (cData.opmode)
                {
                    default:
                    case OperationMode.Sinal:
                        targetFunc = Sin;
                        break;
                    case OperationMode.Cosinal:
                        targetFunc = Cos;
                        break;
                }
                res += cData.angVelShiftAmp * targetFunc((_lt + cData.angVelShiftPhs) * cData.angVelShiftFrq);
                return res;
            }
        }

        #region idrawable things
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            GrabMC();
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("ShelterGate_cog");
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            _mc.BringToKin(sLeaser.sprites[0]);
            sLeaser.sprites[0].rotation = Lerp(lastRot, rot, timeStacker);
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
