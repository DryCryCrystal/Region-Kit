/// <summary>
/// From LB Gamer.
/// </summary>

using UnityEngine;
using System;

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
