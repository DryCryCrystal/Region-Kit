//using System;
//using On;
//using Partiality.Modloader;
//using UnityEngine;

//public class ABThing : PartialityMod
//{
//    public override void Init()
//    {
//        this.ModID = "ABThing";
//        Version = "0002";
//        author = "Dracenis";
//    }
//    public override void OnLoad()
//    {
//        base.OnLoad();
//        On.Room.Loaded += RoomLoadedHK;
//    }

//    public void RoomLoadedHK(On.Room.orig_Loaded orig, Room self)
//    {
//        orig(self); // same as this.orig_Loaded();
//                    // then all the rest of the code, but using `self` instead of `this`
//        for (int k = 0; k < self.roomSettings.effects.Count; k++)
//        {
//            if (self.roomSettings.effects[k].type == EnumExt_ABThing.SandStorm)
//            {
//                self.AddObject(new SandStorm(self.roomSettings.effects[k], self));
//            }
//            else if (self.roomSettings.effects[k].type == EnumExt_ABThing.SandPuffs)
//            {
//                self.AddObject(new SandPuffs(self.roomSettings.effects[k], self));
//            }
//        }
//        /*
//        if (self.abstractRoom.firstTimeRealized)
//        {
//            for (int m = 0; m < self.roomSettings.placedObjects.Count; m++)
//            {
//                if (self.roomSettings.placedObjects[m].active)
//                {
//                    PlacedObject.Type type = self.roomSettings.placedObjects[m].type;
//                    switch (type)
//                    {
//                        case PlacedObject.Type.SporePlant:
//                            if (!(self.game.session is StoryGameSession) || !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
//                            {
//                                AbstractPhysicalObject abstractPhysicalObject = new CactusFruit.AbstractCactusFruit(self.world, null, self.GetWorldCoordinate(self.roomSettings.placedObjects[m].pos), self.game.GetNewID(), self.abstractRoom.index, m, self.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData, false, false);
//                                (abstractPhysicalObject as AbstractConsumable).isConsumed = false;
//                                self.abstractRoom.entities.Add(abstractPhysicalObject);
//                            }
//                            break;
//                    }
//                }
//            }
//        }*/

//    }
//}
