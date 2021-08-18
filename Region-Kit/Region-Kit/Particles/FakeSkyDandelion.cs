using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegionKit.Utils;
using UnityEngine;
using RWCustom;

namespace RegionKit.Particles
{
    public class FakeSkyDandelion : GenericParticle
    {
        public static GenericParticle MakeNew(PBehaviourState start, PVisualState whatever)
        {
            return new FakeSkyDandelion(start);
        }

        public FakeSkyDandelion(PBehaviourState start) 
            : base (start, 
                  new PVisualState("SkyDandelion", "Basic", ContainerCodes.Foreground, Color.white, Color.red, 1f, 40f, 20f, 0f))
        {

        }
    }
}
