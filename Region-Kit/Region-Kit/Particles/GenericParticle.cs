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
    /// <summary>
    /// Working unit for <see cref="RoomParticleSystem"/>.
    /// </summary>
    public class GenericParticle : CosmeticSprite
    {

        public static GenericParticle MakeNew(PMoveState start, PVisualState visuals)
        {
            return new GenericParticle(start, visuals);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bSt">instantiation movement and fade in/out data</param>
        /// <param name="vSt">visuals package</param>
        public GenericParticle(PMoveState bSt, PVisualState vSt) : base()
        {
            //throw null;
            vSt.aElm = vSt.aElm ?? "SkyDandelion";
            start = bSt;
            visuals = vSt;
            vel = DegToVec(bSt.dir).normalized * bSt.speed;
            pos = bSt.pos;
            lastPos = pos;
        }

        public void SliceLT(float frac)
        {

        }

        public override void Update(bool eu)
        {
            lastRot = rot;
            lifetime += 1f;
            //every frame, velocity is set to initial. Make sure to treat it accordingly in your custom behaviour modules
            var cpw = CurrentPower;
            var crd = cRad(cpw);
            if (!SetUpRan)
            {
                foreach (var m in Modules) m.Enable();
                OnCreate?.Invoke();
                if (visuals.lInt > 0f && visuals.lRadMax > 0f)
                {
                    myLight = new LightSource(pos, false, visuals.lCol, this);
                    myLight.requireUpKeep = true;
                    myLight.HardSetAlpha(cpw);
                    myLight.HardSetRad(crd);
                    room.AddObject(myLight);
                }
            }
            SetUpRan = true;
            ProgressLifecycle();
            if (myLight != null)
            {
                myLight.setAlpha = cpw;
                myLight.setRad = crd;
                myLight.setPos = this.pos;
                myLight.stayAlive = true;
                myLight.color = visuals.lCol;
            }
            vel = DegToVec(start.dir) * start.speed;
            OnUpdatePreMove?.Invoke();
            base.Update(eu);
            OnUpdatePostMove?.Invoke();
        }

        public override void Destroy()
        {
            OnDestroy?.Invoke();
            foreach (var m in Modules) m.Disable();
            base.Destroy();
        }

        #region modules
        public void addModule(PBehaviourModule m) { Modules.Add(m); }
        public readonly List<PBehaviourModule> Modules = new List<PBehaviourModule>();

        public delegate void lcStages();
        /// <summary>
        /// invoked near the end of every frame
        /// </summary>
        public event lcStages OnUpdatePreMove;
        /// <summary>
        /// Invoked after base update call. Can be used to undo position changes.
        /// </summary>
        public event lcStages OnUpdatePostMove;
        /// <summary>
        /// invoked on first frame
        /// </summary>
        public event lcStages OnCreate;
        /// <summary>
        /// invoked when particle is about to be destroyed
        /// </summary>
        public event lcStages OnDestroy;
        #endregion

        #region lifecycle

        protected virtual float cRad(float power) => Lerp(visuals.lRadMin, visuals.lRadMax, power);
        /// <summary>
        /// 0 to 1; represents how thick/transparent a particle is at the moment
        /// </summary>
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
        /// <summary>
        /// every frame, ticks down the clock of a particle's birth, thrive and inevitable demise
        /// </summary>
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
        /// <summary>
        /// returns length of current life phase
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
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
        #endregion
        /// <summary>
        /// starting movement parameters and fade in/out settings
        /// </summary>
        public PMoveState start;
        /// <summary>
        /// visual package - atlas element, container, etc
        /// </summary>
        public PVisualState visuals;
        /// <summary>
        /// attached light source
        /// </summary>
        protected LightSource myLight;
        protected bool SetUpRan = false;
        public float lifetime { get; protected set; } = 0f;
        private float lastRot;
        /// <summary>
        /// to make your sprite go speen
        /// </summary>
        public float rot;
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
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(visuals.container.ToString()));
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
            sLeaser.sprites[0].rotation = LerpAngle(lastRot, rot, timeStacker);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        #endregion
    }
}
