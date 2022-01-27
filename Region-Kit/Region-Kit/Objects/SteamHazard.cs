using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using static RegionKit.POM.PlacedObjectsManager;



public static class SteamObjRep
{
    internal static void SteamRep()
    {
        List<ManagedField> fields = new List<ManagedField>
        {
            new FloatField("f1", 0f, 1f, 0.5f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Duration"),
            new FloatField("f2", 0f,1f,0.5f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Frequency"),
            new FloatField("f3", 0f,1f,0.5f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Lifetime"),
            new Vector2Field("v1", new Vector2(0f,45f), Vector2Field.VectorReprType.line)
        };
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(SteamHazard));
    }
}


public class SteamHazard : UpdatableAndDeletable
{
    public PlacedObject placedObject;
    public float durationRate;
    public float frequencyRate;
    public float duration;
    public float frequency;
    public float lifetime;
    public float dangerRange;
    public Vector2 fromPos;
    public Vector2 toPos;
    public Vector2[] steamZone;
    public Smoke.SteamSmoke steam;
    public RectangularDynamicSoundLoop soundLoop;
    public SteamHazard(PlacedObject pObj, Room room)
    {
        this.placedObject = pObj;
        this.room = room;
        this.durationRate = (this.placedObject.data as ManagedData).GetValue<float>("f1");
        this.frequencyRate = (this.placedObject.data as ManagedData).GetValue<float>("f2");
        this.duration = 0f;
        this.frequency = 0f;
        this.lifetime = (this.placedObject.data as ManagedData).GetValue<float>("f3");
        this.fromPos = this.placedObject.pos;
        this.toPos = (this.placedObject.data as ManagedData).GetValue<Vector2>("v1");
        this.steam = new Smoke.SteamSmoke(this.room);
        this.soundLoop = new RectangularDynamicSoundLoop(this, new FloatRect(this.fromPos.x - 20f, this.fromPos.y - 20f, this.fromPos.x + 20f, this.fromPos.y + 20f), this.room);
        this.soundLoop.sound = SoundID.Gate_Water_Steam_LOOP;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.durationRate = (this.placedObject.data as ManagedData).GetValue<float>("f1");
        this.frequencyRate = (this.placedObject.data as ManagedData).GetValue<float>("f2");
        this.lifetime = (this.placedObject.data as ManagedData).GetValue<float>("f3");
        this.dangerRange = 0.9f;
        this.fromPos = this.placedObject.pos;
        this.toPos = (this.placedObject.data as ManagedData).GetValue<Vector2>("v1");

        //Steam burst
        if (this.soundLoop != null)
        {
            if (soundLoop.Volume > 0f)
            {
                this.soundLoop.Update();
            }
            this.frequency += frequencyRate * Time.deltaTime;
            if (this.frequency >= 1f)
            {
                this.soundLoop.Volume = 0.6f;
                this.duration += durationRate * Time.deltaTime;
                this.steam.EmitSmoke(fromPos, toPos * 0.15f, this.room.RoomRect, this.lifetime);
                if (this.duration >= 1f)
                {
                    this.duration = 0f;
                    this.frequency = 0f;
                }
            }
            else
            {
                this.soundLoop.Volume -= 0.5f * Time.deltaTime;
                if(this.soundLoop.Volume <= 0f)
                {
                    this.soundLoop.Stop();
                }
            }
        }

        //Creature hit by steam
        for (int i = 0; i < this.steam.particles.Count; i++)
        {
            if (this.steam.particles[i].life > this.dangerRange)
            {
                for (int w = 0; w < this.room.physicalObjects.Length; w++)
                {
                    for (int j = 0; j < this.room.physicalObjects[w].Count; j++)
                    {
                        for (int k = 0; k < this.room.physicalObjects[w][j].bodyChunks.Length; k++)
                        {
                            Vector2 a = this.room.physicalObjects[w][j].bodyChunks[k].ContactPoint.ToVector2();
                            Vector2 v = this.room.physicalObjects[w][j].bodyChunks[k].pos + a * (this.room.physicalObjects[w][j].bodyChunks[k].rad + 30f);

                            if (Vector2.Distance(this.steam.particles[i].pos, v) < 20f)
                            {
                                if (this.room.physicalObjects[w][j] is Creature)
                                {
                                    if ((this.room.physicalObjects[w][j] as Creature).stun == 0)
                                    {
                                        (this.room.physicalObjects[w][j] as Creature).stun = 100;
                                        this.room.AddObject(new CreatureSpasmer((this.room.physicalObjects[w][j] as Creature), false, (this.room.physicalObjects[w][j] as Creature).stun));
                                        float silentChance = this.room.game.cameras[0].virtualMicrophone.soundLoader.soundTriggers[(int)SoundID.Gate_Water_Steam_Puff].silentChance;
                                        this.room.game.cameras[0].virtualMicrophone.soundLoader.soundTriggers[(int)SoundID.Gate_Water_Steam_Puff].silentChance = 0f;
                                        this.room.PlaySound(SoundID.Gate_Water_Steam_Puff, (this.room.physicalObjects[w][j] as Creature).mainBodyChunk,false, 0.8f, 1f);
                                        this.room.PlaySound(SoundID.Big_Spider_Spit_Warning_Rustle, (this.room.physicalObjects[w][j] as Creature).mainBodyChunk,false, 1f, 1f);
                                        this.room.game.cameras[0].virtualMicrophone.soundLoader.soundTriggers[(int)SoundID.Gate_Water_Steam_Puff].silentChance = silentChance;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

