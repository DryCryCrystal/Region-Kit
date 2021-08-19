using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;

using static RWCustom.Custom;

namespace RegionKit.Particles
{
    public abstract class PBehaviourModule
    {
        GenericParticle owner;
        public PBehaviourModule(GenericParticle gp)
        {
            owner = gp;
        }
        public abstract void Enable();
        public abstract void Disable();

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
                owner.OnUpdate -= owner_update;
            }
            public override void Enable()
            {
                owner.OnUpdate += owner_update;
            }
        }
        public class AFFLICTION : PBehaviourModule
        {
            public AFFLICTION(GenericParticle gp) : base(gp) { }

            public override void Disable() { }

            public override void Enable() { }
        }
        public class ANTIBODY : PBehaviourModule
        {
            public ANTIBODY(GenericParticle gp) : base(gp) { }

            public override void Disable()
            { owner.OnUpdate -= owner_update; }
            public override void Enable()
            { owner.OnUpdate += owner_update; }

            private void DoAfflictionCheck()
            {
                for (int i = 10; i > 0; i--)
                {
                    var tar = owner.room.updateList.RandomOrDefault();
                    if (tar != null && tar is GenericParticle particle && (owner.pos - particle.pos).magnitude < 40f)
                    {
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
            }

            private void owner_update() { DoAfflictionCheck(); }
        }
    }
}
