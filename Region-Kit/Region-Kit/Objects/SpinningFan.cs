using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using static RegionKit.POM.PlacedObjectsManager;

public static class SpinningFanObjRep
{
    internal static void SpinningFanRep()
    {
        List<ManagedField> fields = new List<ManagedField>
        {
            new FloatField("speed", 0f, 1f, 0.6f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Speed"),
            new FloatField("scale", 0f, 1f, 0.3f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Scale"),
            new FloatField("depth", 0f, 1f, 0.3f,0.01f, ManagedFieldWithPanel.ControlType.slider, "Depth")
        };
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(SpinningFan));
    }
}

public class SpinningFan : UpdatableAndDeletable, IDrawable
{
    public PlacedObject pObj;
    public float getToSpeed;
    public Vector2 pos;
    public float speed;
    public float scale;
    public float depth;
    public SpinningFan(PlacedObject pObj, Room room)
    {
        this.pObj = pObj;
        this.room = room;
        this.speed = (this.pObj.data as ManagedData).GetValue<float>("speed");
        this.scale = (this.pObj.data as ManagedData).GetValue<float>("scale");
        this.depth = (this.pObj.data as ManagedData).GetValue<float>("depth");
    }

    public override void Update(bool eu)
    {
        this.pos = this.pObj.pos;
        this.getToSpeed = Mathf.Lerp(-10f, 10f, (this.pObj.data as ManagedData).GetValue<float>("speed"));
        if (this.room.world.rainCycle.brokenAntiGrav != null)
        {
            float target = (this.room.world.rainCycle.brokenAntiGrav.on ? this.getToSpeed : 0f);
            this.speed = Custom.LerpAndTick(this.speed, target, 0.035f, 0.0008f);
        }
        else
        {
            this.speed = this.getToSpeed;
        }
        this.scale = (this.pObj.data as ManagedData).GetValue<float>("scale");
        this.depth = (this.pObj.data as ManagedData).GetValue<float>("depth");
        base.Update(eu);
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("spinningFan", true);
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["ColoredSprite2"];
        this.AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = this.pos.x - camPos.x;
        sLeaser.sprites[0].y = this.pos.y - camPos.y;
        sLeaser.sprites[0].scale = Mathf.Lerp(0.2f, 2f, this.scale);
        sLeaser.sprites[0].rotation += this.speed * timeStacker;
        sLeaser.sprites[0].alpha = this.depth;
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {

    }
}

