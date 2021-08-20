using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;
using static UnityEngine.Mathf;
using static RWCustom.Custom;

namespace RegionKit.Particles
{
    /// <summary>
    /// particle spawner.
    /// </summary>
    public class RoomParticleSystem : UpdatableAndDeletable, INotifyWhenRoomIsViewed
    {
        /// <summary>
        /// Constructor used by MPO.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="room"></param>
        public RoomParticleSystem(PlacedObject owner, Room room) : this (owner, room, GenericParticle.MakeNew)
        {

        }

        public RoomParticleSystem(PlacedObject owner, Room room, params ParticleCreate[] births)
        {
            Owner = owner;
            FetchVisualsAndBM(room);
            if (births != null) foreach (var d in births) BirthEvent += d;
        }

        public override void Update(bool eu)
        {
            ProgressCreationCycle();
            base.Update(eu);
        }
        protected virtual void ProgressCreationCycle()
        {
            cooldown--;
            if (cooldown <= 0)
            {
                var PossibleBirths = BirthEvent?.GetInvocationList();
                if (PossibleBirths != null && PossibleBirths.Length > 0) 
                {
                    var p = (GenericParticle)
                        PossibleBirths.RandomOrDefault().DynamicInvoke(
                            PSD.DataForNew(),
                            Visuals.RandomOrDefault()?.DataForNew() ?? default);
                    p.pos = PickSpawnPos();
                    foreach (var provider in Modifiers)
                    {
                        var newmodule = provider.GetNewForParticle(p);
                        if (newmodule == null) continue;
                        p.addModule(newmodule);
                    }
                    room.AddObject(p);

                } //BirthEvent.Invoke(PSD.DataForNew(), PVC?.DataForNew() ?? default);
                cooldown = UnityEngine.Random.Range(PSD.minCooldown, PSD.maxCooldown);
            }
        }

        protected ParticleSystemData PSD => Owner.data as ParticleSystemData ?? backupPSD;
        //use this if you want to have PSD 
        public ParticleSystemData backupPSD;
        
        protected PlacedObject Owner;
        //same as aboove
        public Vector2 overridePos;
        protected Vector2 MyPos => Owner?.pos ?? overridePos;

        protected int cooldown;
        /// <summary>
        /// Acquires references to all relevant <see cref="ParticleVisualCustomizer"/>s and <see cref="ParticleBehaviourProvider"/>s
        /// </summary>
        /// <param name="room"></param>
        protected virtual void FetchVisualsAndBM(Room room)
        {
            Visuals.Clear();
            Modifiers.Clear();
            for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
            {
                if (room.roomSettings.placedObjects[i].data is ParticleVisualCustomizer f_PVC
                    && (MyPos - f_PVC.owner.pos).sqrMagnitude < f_PVC.p2.sqrMagnitude) 
                { Visuals.Add(f_PVC); }
                if (room.roomSettings.placedObjects[i].data is ParticleBehaviourProvider f_BMD 
                    && (MyPos - f_BMD.owner.pos).sqrMagnitude < f_BMD.p2.sqrMagnitude)
                { Modifiers.Add(f_BMD); }
            }
            //sorted for apply order to work
            Modifiers.Sort(new Comparison<ParticleBehaviourProvider>(
                (x, y) => 
                {
                    if (x.applyOrder > y.applyOrder) return 1;
                    else if (x.applyOrder == y.applyOrder) return 0;
                    else return -1; }));
        }
        protected readonly List<ParticleVisualCustomizer> Visuals = new List<ParticleVisualCustomizer>();
        protected readonly List<ParticleBehaviourProvider> Modifiers = new List<ParticleBehaviourProvider>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="suggestedStart">suggested starting move params</param>
        /// <param name="suggestedVis">suggested visuals </param>
        /// <returns></returns>
        public delegate GenericParticle ParticleCreate(PMoveState suggestedStart, PVisualState suggestedVis);
        /// <summary>
        /// A random subscriber is invoked whenever a particle needs to be created
        /// </summary>
        public event ParticleCreate BirthEvent;

        /// <summary>
        /// Acquires detailed coords from within area of effect
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 PickSpawnPos()
        {
            var tiles = PSD.ReturnSuitableTiles(room);
            if (tiles.Count == 0) { tiles.Add((MyPos / 20).ToIV2());}
            var tile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
            return new Vector2()
            {
                x = Lerp(tile.x * 20, (tile.x + 1) * 20, UnityEngine.Random.value),
                y = Lerp(tile.y * 20, (tile.y + 1) * 20, UnityEngine.Random.value),
            };
        }

        public int AverageLifetime()
        {
            return PSD.fadeIn + PSD.lifeTime + PSD.fadeOut;
        }
        public float AverageSpeed()
        {
            return PSD.startSpeed;
        }

        public void PopulateExpectedArea()
        {
#warning populate not done
            foreach (var tile in PSD.ReturnSuitableTiles(room))
            {
                var detpos = room.MiddleOfTile(tile);
            }
            //throw new NotImplementedException();
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
