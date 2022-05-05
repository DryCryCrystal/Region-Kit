using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RegionKit.Utils;
using RegionKit.ConditionalEffects;

namespace RegionKit.Effects
{
	public static class ColoredRoomEffect /// By M4rbleL1ne/LB Gamer
	{
		public static List<RoomSettings.RoomEffect.Type> coloredEffects = new List<RoomSettings.RoomEffect.Type>();

		public static AttachedField<RoomSettings.RoomEffect, ColorSettings> colorSettings = new AttachedField<RoomSettings.RoomEffect, ColorSettings>();

		public class ColorSettings
		{
			public bool colored; //false
			public float colorR; //0f
			public float colorG; //0f
			public float colorB; //0f

			public Color Color => new Color(colorR, colorG, colorB);
		}

		private static string GRKString(RoomSettings.RoomEffect self)
		{
			/*if (RegionKitMod.TryGetWeak(RegionKitMod.filterFlags, self, out bool[] value2))
			{
				int num2 = 0;
				bool flag = true;
				for (int i = 0; i < value2.Length; i++)
				{
					if (!value2[i]) flag = false;
					else num2 |= 1 << i;
				}

				if (!flag) text = text + "-" + num2;
			}*/
			/*float oldAmount = -1f;
			if (RegionKitMod.TryGetWeak(RegionKitMod.baseIntensities, self, out float savedAmount))
			{
				oldAmount = self.amount;
				self.amount = savedAmount;
			}*/
			string ret = "";
			if (CEExt.TryGetWeak(CECentral.filterFlags, self, out bool[] flags))
			{
				int bitMask = 0;
				bool allTrue = true;
				for (int i = 0; i < flags.Length; i++)
					if (!flags[i]) allTrue = false;
					else bitMask |= 1 << i;
				if (!allTrue) ret += "-" + bitMask;
			}
			/*if (oldAmount != -1f)
				self.amount = oldAmount;*/
			return ret;
		}

		internal static void Apply()
		{
            On.RoomSettings.RoomEffect.ctor += delegate(On.RoomSettings.RoomEffect.orig_ctor orig, RoomSettings.RoomEffect self, RoomSettings.RoomEffect.Type type, float amount, bool inherited)
			{
				orig(self, type, amount, inherited);
				colorSettings[self] = new ColorSettings();
				for (int i = 0; i < coloredEffects.Count; i++)
				{
					if (coloredEffects[i] == type)
					{
						colorSettings[self].colored = true;
						break;
					}
				}
			};
            On.RoomSettings.RoomEffect.ToString += delegate(On.RoomSettings.RoomEffect.orig_ToString orig, RoomSettings.RoomEffect self)
			{
				var reg = GRKString(self);
				var res = orig(self);
				if (colorSettings.TryGet(self, out var cs) && cs.colored) res = $"{self.type}-{self.amount}-{self.panelPosition.x}-{self.panelPosition.y}{reg}-Color-{colorSettings[self].colorR}-{colorSettings[self].colorG}-{colorSettings[self].colorB}";
				return res;
			};
            On.RoomSettings.RoomEffect.FromString += delegate(On.RoomSettings.RoomEffect.orig_FromString orig, RoomSettings.RoomEffect self, string[] s)
			{
				orig(self, s);
				try
				{
					for (int i = 0; i < s.Length; i++)
					{
						if (s[i] == "Color" && colorSettings.TryGetNoVar(self))
						{
							self.amount = float.Parse(s[1]); //--> amount doesn't work if I don't add it again
							colorSettings[self].colored = true;
							colorSettings[self].colorR = float.Parse(s[i + 1]);
							colorSettings[self].colorG = float.Parse(s[i + 2]);
							colorSettings[self].colorB = float.Parse(s[i + 3]);
							break;
						}
					}
				}
				catch { Debug.Log("[Error  :ForsakenStation.ColoredRoomEffect] Wrong syntax effect loaded: " + s[0]); }
			};
            On.DevInterface.EffectPanel.ctor += delegate(On.DevInterface.EffectPanel.orig_ctor orig, EffectPanel self, DevUI owner, DevUINode parentNode, Vector2 pos, RoomSettings.RoomEffect effect)
			{
				orig(self, owner, parentNode, pos, effect);
				if (colorSettings.TryGet(effect, out var cs) && cs.colored)
				{
					self.size.y += 60f;
					int indSn = -1;
					int ftSn = -1;
					for (int i = 0; i < self.subNodes.Count; i++)
					{
						if (self.subNodes[i].IDstring == "Amount_Slider") indSn = i;
						if (self.subNodes[i].IDstring == "Filter_Toggles") ftSn = i;
					}
					if (indSn != -1 && self.subNodes[indSn] is EffectPanel.EffectPanelSlider)
					{
						(self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.y += 60f;
						self.subNodes.Add(new EffectPanel.EffectPanelSlider(owner, "ColorR_Slider", self, new Vector2((self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.x, (self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.y - 20f), "Red: "));
						self.subNodes.Add(new EffectPanel.EffectPanelSlider(owner, "ColorG_Slider", self, new Vector2((self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.x, (self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.y - 40f), "Green: "));
						self.subNodes.Add(new EffectPanel.EffectPanelSlider(owner, "ColorB_Slider", self, new Vector2((self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.x, (self.subNodes[indSn] as EffectPanel.EffectPanelSlider).pos.y - 60f), "Blue: "));
					}
					if (ftSn != -1 && self.subNodes[ftSn] is PositionedDevUINode) (self.subNodes[ftSn] as PositionedDevUINode).pos.y += 60f;
				}
			};
            On.DevInterface.EffectPanel.EffectPanelSlider.Refresh += delegate(On.DevInterface.EffectPanel.EffectPanelSlider.orig_Refresh orig, EffectPanel.EffectPanelSlider self)
			{
				orig(self);
				if (colorSettings.TryGet(self.effect, out var cs) && cs.colored)
				{
					float num = 0f;
					switch (self.IDstring)
					{
						case "Amount_Slider":
							num = self.effect.amount;
							self.NumberText = (int)(num * 100f) + "%"; //--> amount slider doesn't work if I don't add it again
							break;
						case "ColorR_Slider":
							num = colorSettings[self.effect].colorR;
							self.NumberText = ((int)(num * 255f)).ToString();
							break;
						case "ColorG_Slider":
							num = colorSettings[self.effect].colorG;
							self.NumberText = ((int)(num * 255f)).ToString();
							break;
						case "ColorB_Slider":
							num = colorSettings[self.effect].colorB;
							self.NumberText = ((int)(num * 255f)).ToString();
							break;
					}
					self.RefreshNubPos(num);
				}
			};
            On.DevInterface.EffectPanel.EffectPanelSlider.NubDragged += delegate(On.DevInterface.EffectPanel.EffectPanelSlider.orig_NubDragged orig, EffectPanel.EffectPanelSlider self, float nubPos)
			{
				if (!self.effect.inherited && colorSettings.TryGet(self.effect, out var cs) && cs.colored)
				{
					switch (self.IDstring)
					{
						case "Amount_Slider":
							self.effect.amount = nubPos;
							var type = self.effect.type;
							if (type == RoomSettings.RoomEffect.Type.VoidMelt)
							{
								self.owner.room.game.cameras[0].levelGraphic.alpha = self.effect.amount;
								if (self.owner.room.game.cameras[0].fullScreenEffect != null) self.owner.room.game.cameras[0].fullScreenEffect.alpha = self.effect.amount;
							}
							break;
						case "ColorR_Slider":
							colorSettings[self.effect].colorR = nubPos;
							break;
						case "ColorG_Slider":
							colorSettings[self.effect].colorG = nubPos;
							break;
						case "ColorB_Slider":
							colorSettings[self.effect].colorB = nubPos;
							break;
					}
					self.Refresh();
				}
				else orig(self, nubPos);
			};
		}

        #region Extensions
        public static float GetColoredEffectRed(this RoomSettings self, RoomSettings.RoomEffect.Type type)
		{
			for (int i = 0; i < self.effects.Count; i++)
			{
				if (self.effects[i].type == type && colorSettings.TryGetNoVar(self.effects[i])) return colorSettings[self.effects[i]].colorR;
			}
			return 0f;
		}

		public static float GetColoredEffectGreen(this RoomSettings self, RoomSettings.RoomEffect.Type type)
		{
			for (int i = 0; i < self.effects.Count; i++)
			{
				if (self.effects[i].type == type && colorSettings.TryGetNoVar(self.effects[i])) return colorSettings[self.effects[i]].colorG;
			}
			return 0f;
		}

		public static float GetColoredEffectBlue(this RoomSettings self, RoomSettings.RoomEffect.Type type)
		{
			for (int i = 0; i < self.effects.Count; i++)
			{
				if (self.effects[i].type == type && colorSettings.TryGetNoVar(self.effects[i])) return colorSettings[self.effects[i]].colorB;
			}
			return 0f;
		}

		public static Color GetColoredEffectColor(this RoomSettings self, RoomSettings.RoomEffect.Type type)
		{
			for (int i = 0; i < self.effects.Count; i++)
			{
				if (self.effects[i].type == type && colorSettings.TryGetNoVar(self.effects[i])) return colorSettings[self.effects[i]].Color;
			}
			return Color.black;
		}

		public static bool IsEffectInRoom(this RoomSettings self, RoomSettings.RoomEffect.Type type) => self.GetEffect(type) != null;

		public static bool IsEffectColored(this RoomSettings self, RoomSettings.RoomEffect.Type type)
		{
			for (int i = 0; i < self.effects.Count; i++)
			{
				if (self.effects[i].type == type && colorSettings.TryGetNoVar(self.effects[i])) return colorSettings[self.effects[i]].colored;
			}
			return false;
		}

		public static bool TryGetNoVar<TKey, TValue>(this AttachedField<TKey, TValue> self, TKey obj) => self.TryGet(obj, out _);
		#endregion Extensions
	}
}