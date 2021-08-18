using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static RWCustom.Custom;
using static UnityEngine.Mathf;


namespace RegionKit.Particles
{
    public class WavyParticle : GenericParticle
    {
        public static WavyParticle MakeNew(PBehaviourState start, PVisualState visuals)
        {
            return new WavyParticle(start, visuals);
        }

        public WavyParticle(PBehaviourState start, PVisualState visuals) : base(start, visuals)
        {
            _amp = 25f;
            _frq = 0.08f;
        }

        float _lt = 0;
        float _amp;
        float _frq;
        float angDev => _amp * Sin(_lt * _frq);

        public override void Update(bool eu)
        {
            _lt += 1f;
            this.vel = DegToVec(start.dir + angDev).normalized * start.speed;
            base.Update(eu);
        }

        
    }
}
