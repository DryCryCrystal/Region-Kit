using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

//Made By LeeMoriya
public class EnumExt_ARKillRect
{
    public static PlacedObject.Type ARKillRect;
}

public class ARKillRect : UpdatableAndDeletable
{
    public PlacedObject po;
    public IntRect rect;

    public ARKillRect(Room room, PlacedObject pObj)
    {
        this.room = room;
        this.po = pObj;
        this.rect = (this.po.data as PlacedObject.GridRectObjectData).Rect;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        for (int i = 0; i < this.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
            {
                for (int k = 0; k < this.room.physicalObjects[i][j].bodyChunks.Length; k++)
                {
                    Vector2 a = this.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
                    Vector2 v = this.room.physicalObjects[i][j].bodyChunks[k].pos + a * (this.room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
                    if (Custom.InsideRect(this.room.GetTilePosition(v), this.rect))
                    {
                        if (this.room.physicalObjects[i][j] is Creature)
                        {
                            if (!(this.room.physicalObjects[i][j] as Creature).dead)
                            {
                                (this.room.physicalObjects[i][j] as Creature).Die();
                            }
                        }
                    }
                }
            }
        }
    }
}
