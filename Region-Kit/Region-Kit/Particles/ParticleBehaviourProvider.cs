using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RegionKit.Utils;

using static PlacedObjectsManager;

namespace RegionKit.Particles
{
    public abstract class ParticleBehaviourProvider : ManagedData
    {
        public abstract PBehaviourModule GetNewForParticle(GenericParticle p);

        [Vector2Field("p2", 30f, 30f, Vector2Field.VectorReprType.circle)]
        public Vector2 p2; //=> GetValue<Vector2>("p2");
        public ParticleBehaviourProvider(PlacedObject owner, List<ManagedField> addFields) : 
            base(owner, addFields?.AddRangeReturnSelf(new ManagedField[] 
            {
                //new Vector2Field("p2", new Vector2(30f, 30f), Vector2Field.VectorReprType.circle)
            }).ToArray())
        {

        }

        public class AntibodyProvider : ParticleBehaviourProvider
        {
            public AntibodyProvider(PlacedObject owner) : base(owner, null)
            {

            }

            public override PBehaviourModule GetNewForParticle(GenericParticle p)
            {
                return new PBehaviourModule.ANTIBODY(p);
                //throw new NotImplementedException();
            }
        }
        public class AfflictionProvider : ParticleBehaviourProvider
        {
            public AfflictionProvider(PlacedObject owner) : base(owner, null)
            {
            }

            public override PBehaviourModule GetNewForParticle(GenericParticle p)
            {
                return new PBehaviourModule.AFFLICTION(p);
                //throw new NotImplementedException();
            }
        }
        public class WavinessProvider : ParticleBehaviourProvider
        {
            [FloatField("amp", 0.1f, 40f, 15f, displayName:"Amplitude")]
            public float amp;
            [FloatField("ampFluke", 0.1f, 40f, 0f, displayName: "Ampfluke")]
            public float ampFluke;
            [FloatField("frq", 0.02f, 5f, 1f, increment: 0.02f, displayName: "Frequency")]
            public float frq;
            [FloatField("frqFluke", 0.02f, 5f, 0f, increment: 0.02f, displayName: "Freq fluke")]
            public float frqFluke;
            [FloatField("phs", -5f, 5f, 0f, displayName:"Phase")]
            public float phase;
            [FloatField("phsFluke", -5f, 5f, 0f, displayName:"Phase fluke")]
            public float phaseFluke;

            protected Machinery.OscillationParams default_op => new Machinery.OscillationParams(amp, frq, phase, Mathf.Sin);
            protected Machinery.OscillationParams dev_op => new Machinery.OscillationParams(ampFluke, frqFluke, phaseFluke, Mathf.Cos); 

            public Machinery.OscillationParams GetOscParams()
            {
                return default_op.Deviate(dev_op);
            }
            public WavinessProvider(PlacedObject owner) : base (owner, null) { }

            public override PBehaviourModule GetNewForParticle(GenericParticle p)
            {
                //PetrifiedWood.WriteLine("Creating a new module");
                return new PBehaviourModule.Wavy(p, GetOscParams());//throw new NotImplementedException();
            }
        }
    }
}
