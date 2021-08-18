using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RegionKit.Particles
{
    public struct PBehaviourState
    {
#warning finalize what goes here
        public float dir;
        public float speed;
        public int fadeIn;
        public int lifetime;
        public int fadeOut;
        public Vector2 pos;

        public PBehaviourState(float dir, float speed, int fadeIn, int lifetime, int fadeOut, Vector2 pos)
        {
            this.dir = dir;
            this.speed = speed;
            this.fadeIn = fadeIn;
            this.lifetime = lifetime;
            this.fadeOut = fadeOut;
            this.pos = pos;
        }
    }
}
