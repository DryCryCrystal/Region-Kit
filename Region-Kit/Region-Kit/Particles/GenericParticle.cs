using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using RegionKit.Utils;

using static UnityEngine.Mathf;
using static RWCustom.Custom;
using static RegionKit.Utils.PetrifiedWood;

namespace RegionKit.Particles
{
    public abstract class GenericParticle : CosmeticSprite
    {
        public GenericParticle(PBehaviourState bSt, PVisualState vSt) : base()
        {
            start = bSt;
            visuals = vSt;
            //fadeIn = bSt.fadeIn;
            //lt = bSt.lifetime;
            //fadeOut = bSt.fadeOut;
            //startDir = bSt.dir;
            //startSpeed = bSt.speed;
            vel = DegToVec(bSt.dir).normalized * bSt.speed;
            pos = bSt.pos;
            lastPos = pos;
        }

        public override void Update(bool eu)
        {
            var cpw = CurrentPower;
            var crd = cRad(cpw);
            if (!lightSetUpRan)
            {
                if (visuals.lInt > 0f && visuals.lRadMax > 0f)
                {
                    myLight = new LightSource(pos, false, visuals.lCol, this);
                    myLight.requireUpKeep = true;
                    myLight.HardSetAlpha(cpw);
                    myLight.HardSetRad(crd);
                    room.AddObject(myLight);
                }
                lightSetUpRan = true;
            }
            ProgressLifecycle();
            base.Update(eu);
            if (myLight != null)
            {
                myLight.setAlpha = cpw;
                myLight.setRad = crd;
                myLight.setPos = this.pos;
                myLight.stayAlive = true;
                myLight.color = visuals.lCol;
            }
            
        }

        #region lifecycle

        protected virtual float cRad(float power) => Lerp(visuals.lRadMin, visuals.lRadMax, power);
        public virtual float CurrentPower
        {
            get
            {
                switch (phase)
                {
                    case 0: return Lerp(0f, 1f, (float)progress / (float)GetPhaseLimit(0));
                    case 1: return 1f;
                    case 2: return Lerp(1f, 0f, (float)progress / (float)GetPhaseLimit(2));
                    default: return 0f;
                }
            }
        }
        internal void ProgressLifecycle()
        {
            progress++;
            if (progress > GetPhaseLimit(phase))
            {
                progress = 0;
                phase++;
            }
            if (phase > 2) this.Destroy();
        }
        public int progress
        {
            get => _pr;
            private set { _pr = value; }
        }
        private int _pr;
        private int GetPhaseLimit(byte phase)
        {
            switch (phase)
            {
                case 0: return start.fadeIn;
                case 1: return start.lifetime;
                case 2: return start.fadeOut;
                default: return 0;
            }
        }
        public byte phase = 0;
        //public readonly float startDir;
        //public readonly float startSpeed;
        //public readonly int fadeIn;
        //public readonly int lt;
        //public readonly int fadeOut;
        #endregion

        public PBehaviourState start;
        public PVisualState visuals;
        protected LightSource myLight;
        protected bool lightSetUpRan = false;

        //protected Vector2 VEL;

        #region IDrawable things
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            try
            {
                sLeaser.sprites[0] = new FSprite(visuals.aElm);
            }
            catch (Exception fue)
            {
                PetrifiedWood.WriteLine($"Invalid atlas element {visuals.aElm}!");
                PetrifiedWood.WriteLine(fue);
                sLeaser.sprites[0] = new FSprite("SkyDandelion", true);// .element = Futile.atlasManager.GetElementWithName("SkyDandelion");
            }
            room.game.rainWorld.Shaders.TryGetValue("Basic", out var sh);
            sLeaser.sprites[0].color = visuals.sCol;
            sLeaser.sprites[0].shader = sh;
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(ContainerCodes.Foreground.ToString()));
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var cpos = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].SetPosition(cpos - camPos);
            sLeaser.sprites[0].alpha = CurrentPower;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        #endregion


    }
}
