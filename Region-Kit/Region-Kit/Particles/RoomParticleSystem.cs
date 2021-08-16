using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Particles
{
    public class RoomParticleSystem : UpdatableAndDeletable, INotifyWhenRoomIsViewed
    {
        public RoomParticleSystem(PlacedObject owner, Room room)
        {
            Owner = owner;
        }
        internal PlacedObject Owner;
        internal int cooldown;
        public delegate void ParticleCreate(PBehaviourState data);
        public event ParticleCreate CreatingParticle;

        public void PopulateExpectedArea()
        {
#warning populate not done
            throw new NotImplementedException();
        }
        public virtual void RoomViewed()
        {
            PopulateExpectedArea();
            //throw new NotImplementedException();
        }

        public virtual void RoomNoLongerViewed()
        {
            //throw new NotImplementedException();
        }
    }
}
