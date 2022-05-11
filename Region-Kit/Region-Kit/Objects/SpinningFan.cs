﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using static RegionKit.POM.PlacedObjectsManager;

namespace RegionKit.Objects
{
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
            speed = (this.pObj.data as ManagedData).GetValue<float>("speed");
            scale = (this.pObj.data as ManagedData).GetValue<float>("scale");
            depth = (this.pObj.data as ManagedData).GetValue<float>("depth");
        }

        public override void Update(bool eu)
        {
            pos = pObj.pos;
            getToSpeed = Mathf.Lerp(-10f, 10f, (pObj.data as ManagedData).GetValue<float>("speed"));
            if (room.world.rainCycle.brokenAntiGrav != null)
            {
                float target = room.world.rainCycle.brokenAntiGrav.CurrentLightsOn > 0f ? getToSpeed : 0f;
                speed = Custom.LerpAndTick(speed, target, 0.035f, 0.0008f);
            }
            else
            {
                speed = getToSpeed;
            }
            scale = (pObj.data as ManagedData).GetValue<float>("scale");
            depth = (pObj.data as ManagedData).GetValue<float>("depth");
            base.Update(eu);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("spinningFan", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["ColoredSprite2"];
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
            sLeaser.sprites[0].scale = Mathf.Lerp(0.2f, 2f, scale);
            sLeaser.sprites[0].rotation += speed * timeStacker;
            sLeaser.sprites[0].alpha = depth;
            if (slatedForDeletetion || room != rCam.room)
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
}