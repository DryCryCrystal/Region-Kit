using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Menu;

namespace SandboxUnlockCore;
/// <summary>
/// Port of the current code from SandboxUnlockCore, original verion from the one who screams I guess
/// It is not compatible with most mods that add unlocks their own way so we'll need to modify it.
/// I added the delegates each time but orig isn't called for each hook.
/// </summary>
public class Main
{
	public static List<MultiplayerUnlocks.SandboxUnlockID> creatures;

	public static List<MultiplayerUnlocks.SandboxUnlockID> items;

	public static Dictionary<MultiplayerUnlocks.SandboxUnlockID, int> killScores;

	private static int noKillNumber = 11;

	private static int killRows;

	public static void ApplyHK()
	{
		creatures = new();
		items = new();
		killScores = new();

		On.Menu.SandboxEditorSelector.ctor += SandboxEditorSelector_ctor;
		On.Menu.SandboxSettingsInterface.ctor += SandboxSettingsInterface_ctor;
		On.Menu.SandboxSettingsInterface.AddScoreButton_1 += SandboxSettingsInterface_AddScoreButton_1;
		On.Menu.SandboxSettingsInterface.DefaultKillScores += SandboxSettingsInterface_DefaultKillScores;
	}

	private static void SandboxSettingsInterface_DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
	{
		orig(ref killScores);
		for (int i = 0; i < Main.killScores.Count; i++) killScores[(int)Main.killScores.Keys.ElementAt(i)] = Main.killScores.Values.ElementAt(i);
	}

	private static void SandboxSettingsInterface_AddScoreButton_1(On.Menu.SandboxSettingsInterface.orig_AddScoreButton_1 orig, SandboxSettingsInterface self, SandboxSettingsInterface.ScoreController button, ref IntVector2 ps)
	{
		if (button != null)
		{
			self.scoreControllers.Add(button);
			self.subObjects.Add(button);
			button.pos = new(ps.x * 88.666f + 0.01f, ps.y * -30f + 30f * (killRows - 8));
		}
		ps.y++;
		if (ps.y > killRows)
		{
			ps.y = 0;
			ps.x++;
		}
	}

	private static void SandboxSettingsInterface_ctor(On.Menu.SandboxSettingsInterface.orig_ctor orig, SandboxSettingsInterface self, Menu.Menu menu, MenuObject owner)
	{
		int num = MultiplayerUnlocks.CreaturesUnlocks - noKillNumber + 4 + killScores.Count;
		killRows = num / 4 - ((num % 4 == 0) ? 1 : 0);
		if (killRows > 10) SquishPlayerSelect(menu as MultiplayerMenu);
		orig(self, menu, owner);
		for (int num2 = self.subObjects.Count - 1; num2 >= 0; num2--)
		{
			self.subObjects[num2].RemoveSprites();
			self.RemoveSubObject(self.subObjects[num2]);
		}
		self.scoreControllers = new();
		IntVector2 ps = new(0, 0);
		for (int i = 0; i < MultiplayerUnlocks.CreaturesUnlocks; i++)
		{
			if (i != 10 && i != 14 && i != 15 && i != 40 && i != 30 && i != 19 && i != 42 && i != 36 && i != 22 && i != 27 && i != 28) self.AddScoreButton((MultiplayerUnlocks.SandboxUnlockID)i, ref ps);
		}
		for (int j = 0; j < killScores.Count; j++) self.AddScoreButton(killScores.Keys.ElementAt(j), ref ps);
		for (int k = 0; k < 1; k++) self.AddScoreButton(null, ref ps);
		self.AddScoreButton(new SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Food"), "FOODSCORE"), ref ps);
		self.AddScoreButton(new SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Survive"), "SURVIVESCORE"), ref ps);
		self.AddScoreButton(new SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Spear hit"), "SPEARHITSCORE"), ref ps);
		if (menu.CurrLang != 0)
		{
			for (int l = 1; l < 4; l++)
			{
				SandboxSettingsInterface.ScoreController scoreController = self.scoreControllers[self.scoreControllers.Count - l];
				scoreController.pos.x += 24f;
			}
		}
		self.subObjects.Add(new SymbolButton(menu, self, "Menu_Symbol_Clear_All", "CLEARSCORES", new(0f, -280f)));
		for (int m = 0; m < self.subObjects.Count; m++)
		{
			if (self.subObjects[m] is SandboxSettingsInterface.ScoreController) (self.subObjects[m] as SandboxSettingsInterface.ScoreController).scoreDragger.UpdateScoreText();
		}
	}

	private static void SandboxEditorSelector_ctor(On.Menu.SandboxEditorSelector.orig_ctor orig, SandboxEditorSelector self, Menu.Menu menu, MenuObject owner, SandboxOverlayOwner overlayOwner)
	{
		int num = MultiplayerUnlocks.CreaturesUnlocks + MultiplayerUnlocks.ItemsUnlocks + creatures.Count + items.Count + 8;
		int num2 = num / 19 + 1;
		while (SandboxEditorSelector.Height < num2) SandboxEditorSelector.Height++;
		orig(self, menu, owner, overlayOwner);
		for (int num3 = self.subObjects.Count - 1; num3 >= 0; num3--)
		{
			self.subObjects[num3].RemoveSprites();
			self.RemoveSubObject(self.subObjects[num3]);
		}
		self.lastPos = new(-1000f, -1000f);
		self.overlayOwner = overlayOwner;
		overlayOwner.selector = self;
		self.bkgRect = new(menu, self, new(-10f, -30f), self.size + new Vector2(20f, 60f), true);
		self.subObjects.Add(self.bkgRect);
		self.infoLabel = new(menu, self, string.Empty, new(self.size.x / 2f - 100f, 0f), new(200f, 20f), false);
		self.subObjects.Add(self.infoLabel);
		self.buttons = new SandboxEditorSelector.Button[SandboxEditorSelector.Width, SandboxEditorSelector.Height];
		int counter = 0;
		self.AddButton(new SandboxEditorSelector.RectButton(menu, self, SandboxEditorSelector.ActionButton.Action.ClearAll), ref counter);
		for (int i = 0; i < 2; i++) self.AddButton(null, ref counter);
		for (int j = MultiplayerUnlocks.CreaturesUnlocks; j < MultiplayerUnlocks.CreaturesUnlocks + MultiplayerUnlocks.ItemsUnlocks; j++)
		{
			if (self.unlocks.SandboxItemUnlocked((MultiplayerUnlocks.SandboxUnlockID)j)) self.AddButton(new SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock((MultiplayerUnlocks.SandboxUnlockID)j)), ref counter);
			else self.AddButton(new SandboxEditorSelector.LockedButton(menu, self), ref counter);
		}
		for (int k = 0; k < items.Count; k++)
		{
			if (self.unlocks.SandboxItemUnlocked(items[k])) self.AddButton(new SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock(items[k])), ref counter);
			else self.AddButton(new SandboxEditorSelector.LockedButton(menu, self), ref counter);
		}
		for (int l = 0; l < MultiplayerUnlocks.CreaturesUnlocks; l++)
		{
			if (self.unlocks.SandboxItemUnlocked((MultiplayerUnlocks.SandboxUnlockID)l)) self.AddButton(new SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock((MultiplayerUnlocks.SandboxUnlockID)l)), ref counter);
			else self.AddButton(new SandboxEditorSelector.LockedButton(menu, self), ref counter);
		}
		for (int m = 0; m < creatures.Count; m++)
		{
			if (self.unlocks.SandboxItemUnlocked(creatures[m])) self.AddButton(new SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock(creatures[m])), ref counter);
			else self.AddButton(new SandboxEditorSelector.LockedButton(menu, self), ref counter);
		}
		self.AddButton(new SandboxEditorSelector.RectButton(menu, self, SandboxEditorSelector.ActionButton.Action.Play), SandboxEditorSelector.Width - 1, 0);
		self.AddButton(new SandboxEditorSelector.RandomizeButton(menu, self), SandboxEditorSelector.Width - 6, 0);
		self.AddButton(new SandboxEditorSelector.ConfigButton(menu, self, SandboxEditorSelector.ActionButton.Action.ConfigA, 0), SandboxEditorSelector.Width - 5, 0);
		self.AddButton(new SandboxEditorSelector.ConfigButton(menu, self, SandboxEditorSelector.ActionButton.Action.ConfigB, 1), SandboxEditorSelector.Width - 4, 0);
		self.AddButton(new SandboxEditorSelector.ConfigButton(menu, self, SandboxEditorSelector.ActionButton.Action.ConfigC, 2), SandboxEditorSelector.Width - 3, 0);
		for (int n = 0; n < SandboxEditorSelector.Width; n++)
		{
			for (int num4 = 0; num4 < SandboxEditorSelector.Height; num4++)
			{
				if (self.buttons[n, num4] != null) self.buttons[n, num4].Initiate(new(n, num4));
			}
		}
		self.cursors = new();
	}

	public static void SquishPlayerSelect(MultiplayerMenu self)
	{
		for (int i = 0; i < self.playerJoinButtons.Length; i++)
		{
			self.playerJoinButtons[i].RemoveSprites();
			self.pages[0].RemoveSubObject(self.playerJoinButtons[i]);
		}
		self.playerJoinButtons = new PlayerJoinButton[4];
		for (int j = 0; j < self.playerJoinButtons.Length; j++)
		{
			self.playerJoinButtons[j] = new(self, self.pages[0], new Vector2(600f + j * 110f + 40f, 500f) + new Vector2(106f, -20f), j);
			self.pages[0].subObjects.Add(self.playerJoinButtons[j]);
		}
	}
}