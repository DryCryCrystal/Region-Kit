/// <summary>
/// From LB Gamer.
/// </summary>

using UnityEngine;
using System;

namespace RegionKit
{
	public static class ArenaFixes
	{
		public static void ApplyHK()
		{
			On.ScavengerTreasury.ctor += ScavengerTreasury_ctorHK;
			On.DaddyCorruption.ctor += DaddyCorruption_ctorHK;
		}

        private static void ScavengerTreasury_ctorHK(On.ScavengerTreasury.orig_ctor orig, ScavengerTreasury self, Room room, PlacedObject placedObj)
        {
            try
            {
				orig(self, room, placedObj);
            }
			catch (NullReferenceException)
            {
				for (int k = 0; k < self.tiles.Count; k++)
				{
					if (UnityEngine.Random.value < Mathf.InverseLerp(self.Rad, self.Rad / 5f, Vector2.Distance(room.MiddleOfTile(self.tiles[k]), placedObj.pos)))
					{
						AbstractPhysicalObject abstractPhysicalObject = (UnityEngine.Random.value < 0.1f) ? new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, room.GetWorldCoordinate(self.tiles[k]), room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc) : ((UnityEngine.Random.value < 0.142857149f) ? new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(self.tiles[k]), room.game.GetNewID()) : ((!(UnityEngine.Random.value < 1f /  20f)) ? new AbstractSpear(room.world, null, room.GetWorldCoordinate(self.tiles[k]), room.game.GetNewID(), UnityEngine.Random.value < 0.75f) : new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, room.GetWorldCoordinate(self.tiles[k]), room.game.GetNewID())));
						self.property.Add(abstractPhysicalObject);
						if (abstractPhysicalObject != null)
						{
							room.abstractRoom.entities.Add(abstractPhysicalObject);
						}
					}
				}
			}
        }

        private static void DaddyCorruption_ctorHK(On.DaddyCorruption.orig_ctor orig, DaddyCorruption self, Room room)
		{
			try
			{
				orig(self, room);
			}
			catch (NullReferenceException)
			{
				self.GWmode = false;
				self.effectColor = new Color(0f, 0f, 1f);
				self.eyeColor = self.effectColor;
			}
		}
	}
}
