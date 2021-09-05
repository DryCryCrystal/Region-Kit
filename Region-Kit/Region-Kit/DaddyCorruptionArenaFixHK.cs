/// <summary>
/// From LB Gamer.
/// </summary>

using System.Collections.Generic;
using UnityEngine;

namespace RegionKit
{
	public static class DaddyCorruptionArenaFixHK
	{
		public static void ApplyHK()
		{
            On.DaddyCorruption.ctor += DaddyCorruption_ctorHK;
		}

        private static void DaddyCorruption_ctorHK(On.DaddyCorruption.orig_ctor orig, DaddyCorruption self, Room room)
		{
			if (!room.game.IsStorySession)
			{
				self.places = new List<PlacedObject>();
				self.climbTubes = new List<DaddyCorruption.ClimbableCorruptionTube>();
				self.restrainedDaddies = new List<DaddyCorruption.DaddyRestraint>();
				self.eatCreatures = new List<DaddyCorruption.EatenCreature>();
				self.GWmode = false;
				self.effectColor = new Color(0f, 0f, 1f);
				self.eyeColor = self.effectColor;
			}
			else
			{
				orig(self, room);
			}
		}
	}
}
