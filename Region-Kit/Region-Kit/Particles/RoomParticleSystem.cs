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
    public class RoomParticleSystem : UpdatableAndDeletable, INotifyWhenRoomIsViewed
    {
        public RoomParticleSystem(PlacedObject owner, Room room)
        {
            Owner = owner;
            //PVC = new List<ParticleVisualCustomizer>();
            FetchVisualsAndBM(room);
            BirthEvent += GenericParticle.MakeNew;
            //BirthEvent += WavyParticle.MakeNew;

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
                        p.addModule(newmodule);
                    }
                    room.AddObject(p);

                } //BirthEvent.Invoke(PSD.DataForNew(), PVC?.DataForNew() ?? default);
                cooldown = UnityEngine.Random.Range(PSD.minCooldown, PSD.maxCooldown);
            }
        }

        //protected System.Random R;
        protected ParticleSystemData PSD => Owner.data as ParticleSystemData;
        protected PlacedObject Owner;
        protected int cooldown;

        protected virtual void FetchVisualsAndBM(Room room)
        {
            Visuals.Clear();
            Modifiers.Clear();
            for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
            {
                if (room.roomSettings.placedObjects[i].data is ParticleVisualCustomizer f_PVC
                    && (Owner.pos - f_PVC.owner.pos).sqrMagnitude < f_PVC.p2.sqrMagnitude) 
                { Visuals.Add(f_PVC); }
                if (room.roomSettings.placedObjects[i].data is ParticleBehaviourProvider f_BMD 
                    && (Owner.pos - f_BMD.owner.pos).sqrMagnitude < f_BMD.p2.sqrMagnitude)
                { Modifiers.Add(f_BMD); }
            }
        }
        protected readonly List<ParticleVisualCustomizer> Visuals = new List<ParticleVisualCustomizer>();
        protected readonly List<ParticleBehaviourProvider> Modifiers = new List<ParticleBehaviourProvider>();
        
        public delegate GenericParticle ParticleCreate(PMoveState suggestedStart, PVisualState suggestedVis);
        public event ParticleCreate BirthEvent;

        //protected virtual float EstimateTravelDistance(float ltSlice)
        //{
            
        //}
        //protected virtual float EstimateAngDev(float ltSlice)
        //{

        //}
        //protected virtual Vector2 PickSpawnPos(float ltSlice)
        //{
        //    var res = PickSpawnPos();
        //    res += DegToVec(EstimateAngDev(ltSlice)).normalized * EstimateTravelDistance(ltSlice);
        //    return res;
        //}
        protected virtual Vector2 PickSpawnPos()
        {
            var tiles = PSD.ReturnSuitableTiles(room);
            if (tiles.Count == 0) { tiles.Add((Owner.pos / 20).ToIV2());}
            var tile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
            return new Vector2()
            {
                x = Mathf.Lerp(tile.x * 20, (tile.x + 1) * 20, UnityEngine.Random.value),
                y = Mathf.Lerp(tile.y * 20, (tile.y + 1) * 20, UnityEngine.Random.value),
            };
        }

        public void PopulateExpectedArea()
        {
#warning populate not done
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
