﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;

using static RWCustom.Custom;
using static SharedPhysics;

namespace RegionKit.Particles
{
    /// <summary>
    /// behaviour modules that can be slapped onto particles. Instantiated by <see cref="ParticleBehaviourProvider"/>s, passed by <see cref="RoomParticleSystem"/>s and used by <see cref="GenericParticle"/>s.
    /// </summary>
    public abstract class PBehaviourModule
    {
        GenericParticle owner;

        /// <summary>
        /// Use this to indicate how computationally heavy is your <see cref="PBehaviourModule"/> derivative. Used to smoothen loading process.
        /// </summary>
        public virtual float ComputationalCost => 0.15f;

        public PBehaviourModule(GenericParticle gp)
        {
            owner = gp;
        }
        /// <summary>
        /// Ran when module is added to a particle
        /// </summary>
        public abstract void Enable();
        /// <summary>
        /// Ran when owner is about to be destroyed
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Applies wavy path effect to a particle
        /// </summary>
        public class Wavy : PBehaviourModule
        {
            public Wavy(GenericParticle gp, Machinery.OscillationParams osp) : base(gp)
            {
                wave = osp;
            }

            Machinery.OscillationParams wave;

            public void owner_update()
            {
                owner.vel  = RotateAroundOrigo(owner.vel, wave.oscm((owner.lifetime + wave.phase) * wave.frq) * wave.amp);//+= PerpendicularVector(owner.vel).normalized * wave.oscm((owner.lifetime + wave.phase) * wave.frq) * wave.amp;//
            }

            public override void Disable()
            {
                owner.OnUpdatePreMove -= owner_update;
            }
            public override void Enable()
            {
                owner.OnUpdatePreMove += owner_update;
            }
            public override float ComputationalCost => base.ComputationalCost + 0.06f;
        }
        /// <summary>
        /// Example module: does nothing outside of triggering <see cref="ANTIBODY"/> instances
        /// </summary>
        public class AFFLICTION : PBehaviourModule
        {
            //public static AFFLICTION makeNew(GenericParticle gp) => new AFFLICTION(gp);
            public AFFLICTION(GenericParticle gp) : base(gp) { }

            public override void Disable() { }

            public override void Enable() { }
        }
        /// <summary>
        /// Makes the particle destroy any nearby particles with <see cref="AFFLICTION"/> when found
        /// </summary>
        public class ANTIBODY : PBehaviourModule
        {
            public ANTIBODY(GenericParticle gp) : base(gp) { }

            public override void Disable()
            { owner.OnUpdatePreMove -= owner_update; }
            public override void Enable()
            { owner.OnUpdatePreMove += owner_update; }

            private void owner_update() {
                for (int i = 9; i > 0; i--)
                {
                    var tar = owner.room.updateList.RandomOrDefault();
                    if (tar == owner) continue;
                    if (tar != null && tar is GenericParticle particle && (owner.pos - particle.pos).magnitude < 40f) 
                        foreach (var mod in particle.Modules)
                        {
                            if (mod is AFFLICTION)
                            {
                                particle.Destroy();
                                owner.Destroy();
                                owner.room.AddObject
                                    (new ShockWave(owner.pos, 50f, 0.1f, 20));
                                owner.room.PlaySound(SoundID.Seed_Cob_Pop, owner.pos, 0.7f, 1.3f);
                            }
                        }
                }
            }
            public override float ComputationalCost => 0.09f;
        }

        public class AvoidWater : PBehaviourModule
        {
            public AvoidWater(GenericParticle gp) : base(gp)
            {
            }

            public override void Disable()
            {
                owner.OnUpdatePostMove -= actionCycle;
            }

            public override void Enable()
            {
                owner.OnUpdatePostMove += actionCycle;
            }

            internal void actionCycle()
            {
                var y = owner.room.FloatWaterLevel(owner.pos.x);
                if (owner.pos.y < y) owner.pos.y = y;
            }
        }
        public class wallCollision : PBehaviourModule
        {
            public wallCollision(GenericParticle gp) : base(gp)
            {

            }

            public override void Disable()
            {
                owner.OnUpdatePostMove -= postMoveAct;
            }
            public override void Enable()
            {
                owner.OnUpdatePostMove += postMoveAct;
            }

            protected virtual void postMoveAct()
            {
                var cd = new TerrainCollisionData(owner.pos, owner.lastPos, owner.vel, 1f, default, false);
                cd = VerticalCollision(owner.room, cd);
                cd = HorizontalCollision(owner.room, cd);
                owner.pos = cd.pos;
                owner.vel = cd.vel;
            }
            public override float ComputationalCost => base.ComputationalCost + 0.06f;
        }
        public class stickToSurface : wallCollision
        {
            public stickToSurface(GenericParticle gp) : base(gp)
            {

            }

            public override void Disable()
            {
                base.Disable();
                owner.OnUpdatePreMove -= preMoveAct;
            }
            public override void Enable()
            {
                base.Enable();
                owner.OnUpdatePreMove += preMoveAct;
            }
            Vector2 cpos;

            private void preMoveAct()
            {
                if (stuck) owner.vel = default;
                cpos = owner.pos;
            }

            protected override void postMoveAct()
            {
                if (stuck)
                {
                    owner.pos = cpos;
                    owner.lastPos = cpos;
                }
                var op = owner.pos;
                base.postMoveAct();
                if (op != owner.pos) stuck = true;
            }
            private bool stuck;

            public override float ComputationalCost => base.ComputationalCost + 0.07f;
        }
        public class Spin : PBehaviourModule
        {
            public Spin(GenericParticle gp, float angVb, Machinery.OscillationParams osp) : base(gp)
            {
                myosp = osp;
                angVelBase = angVb;
            }
            float angVelBase;
            Machinery.OscillationParams myosp;
            public override void Disable()
            {
                owner.OnUpdatePostMove -= actionCycle;
            }

            public override void Enable()
            {
                owner.OnUpdatePostMove += actionCycle;
            }

            private void actionCycle()
            {
                owner.rot += angVelBase + myosp.oscm(owner.lifetime * myosp.frq) * myosp.amp;
            }
            public override float ComputationalCost => base.ComputationalCost + 0.06f;
        }
    }
}
