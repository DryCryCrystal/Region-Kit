using System.Collections.Generic;
using System.Linq;
using On.Menu;
using RWCustom;
using UnityEngine;
using MenuLabel = Menu.MenuLabel;
using MenuObject = Menu.MenuObject;
using MultiplayerMenu = Menu.MultiplayerMenu;
using PlayerJoinButton = Menu.PlayerJoinButton;
using RoundedRect = Menu.RoundedRect;
using SymbolButton = Menu.SymbolButton;

namespace RegionKit {
    public class SandboxUnlockCore {
        public List<MultiplayerUnlocks.SandboxUnlockID> creatures;
        public List<MultiplayerUnlocks.SandboxUnlockID> items;
        public Dictionary<MultiplayerUnlocks.SandboxUnlockID, int> killScores;
        private static int noKillNumber = 11;
        private int killRows;
        public static SandboxUnlockCore Main;

        public SandboxUnlockCore() {
            Main = this;
        }
        public void ApplyHooks() {
            creatures = new List<MultiplayerUnlocks.SandboxUnlockID>();
            items = new List<MultiplayerUnlocks.SandboxUnlockID>();
            killScores = new Dictionary<MultiplayerUnlocks.SandboxUnlockID, int>();
            SandboxEditorSelector.ctor += AddIcon;
            SandboxSettingsInterface.ctor += AddKills;
            SandboxSettingsInterface.AddScoreButton_1 += AddButton;
            SandboxSettingsInterface.DefaultKillScores += Default;
        }

        private void Default(SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] lkillScores) {
            orig.Invoke(ref lkillScores);
            for (int index = 0; index < killScores.Count; ++index)
                killScores[killScores.Keys.ElementAt(index)] = killScores.Values.ElementAt(index);
        }

        private static void SquishPlayerSelect(MultiplayerMenu self) {
            foreach (var pJointButton in self.playerJoinButtons) {
                pJointButton.RemoveSprites();
                self.pages[0].RemoveSubObject(pJointButton);
            }

            self.playerJoinButtons = new PlayerJoinButton[4];
            for (int index = 0; index < self.playerJoinButtons.Length; ++index) {
                self.playerJoinButtons[index] = new PlayerJoinButton(self, self.pages[0], new Vector2((float)(600.0 + index * 110.0 + 40.0), 500f) + new Vector2(106f, -20f), index);
                self.pages[0].subObjects.Add(self.playerJoinButtons[index]);
            }
        }

        private void AddButton(SandboxSettingsInterface.orig_AddScoreButton_1 orig, Menu.SandboxSettingsInterface self, Menu.SandboxSettingsInterface.ScoreController button, ref IntVector2 ps) {
            if (button != null) {
                self.scoreControllers.Add(button);
                self.subObjects.Add(button);
                button.pos = new Vector2((float)(ps.x * 88.6660003662109 + 0.00999999977648258), (float)(ps.y * -30.0 + 30.0 * (killRows - 8)));
            }

            ++ps.y;
            if (ps.y <= killRows)
                return;
            ps.y = 0;
            ++ps.x;
        }

        private void AddKills(SandboxSettingsInterface.orig_ctor orig, Menu.SandboxSettingsInterface self, Menu.Menu menu, MenuObject owner) {
            int num = MultiplayerUnlocks.CreaturesUnlocks - noKillNumber + 4 + killScores.Count;
            killRows = num / 4 - (num % 4 == 0 ? 1 : 0);
            if (killRows > 10)
                SquishPlayerSelect(menu as MultiplayerMenu);
            orig.Invoke(self, menu, owner);
            for (int index = self.subObjects.Count - 1; index >= 0; --index) {
                self.subObjects[index].RemoveSprites();
                self.RemoveSubObject(self.subObjects[index]);
            }

            self.scoreControllers = new List<Menu.SandboxSettingsInterface.ScoreController>();
            IntVector2 ps = new IntVector2(0, 0);
            for (int index = 0; index < MultiplayerUnlocks.CreaturesUnlocks; ++index) {
                if (index != 10 && index != 14 && index != 15 && index != 40 && index != 30 && index != 19 && index != 42 && index != 36 && index != 22 && index != 27 && index != 28)
                    self.AddScoreButton((MultiplayerUnlocks.SandboxUnlockID)index, ref ps);
            }

            for (int index = 0; index < killScores.Count; ++index)
                self.AddScoreButton(killScores.Keys.ElementAt(index), ref ps);
            for (int index = 0; index < 1; ++index)
                self.AddScoreButton(null, ref ps);
            self.AddScoreButton(new Menu.SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Food"), "FOODSCORE"), ref ps);
            self.AddScoreButton(new Menu.SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Survive"), "SURVIVESCORE"), ref ps);
            self.AddScoreButton(new Menu.SandboxSettingsInterface.MiscScore(menu, self, menu.Translate("Spear hit"), "SPEARHITSCORE"), ref ps);
            if ((uint)menu.CurrLang > 0U) {
                for (int index = 1; index < 4; ++index)
                    self.scoreControllers[self.scoreControllers.Count - index].pos.x += 24f;
            }

            self.subObjects.Add(new SymbolButton(menu, self, "Menu_Symbol_Clear_All", "CLEARSCORES", new Vector2(0.0f, -280f)));
            foreach (var menuObj in self.subObjects.OfType<Menu.SandboxSettingsInterface.ScoreController>()) {
                menuObj.scoreDragger.UpdateScoreText();
            }
        }

        private void AddIcon(SandboxEditorSelector.orig_ctor orig, Menu.SandboxEditorSelector self, Menu.Menu menu, MenuObject owner, SandboxOverlayOwner overlayOwner) {
            int num = (MultiplayerUnlocks.CreaturesUnlocks + MultiplayerUnlocks.ItemsUnlocks + creatures.Count + items.Count + 8) / 19 + 1;
            while (Menu.SandboxEditorSelector.Height < num)
                ++Menu.SandboxEditorSelector.Height;
            orig.Invoke(self, menu, owner, overlayOwner);
            for (int index = self.subObjects.Count - 1; index >= 0; --index) {
                self.subObjects[index].RemoveSprites();
                self.RemoveSubObject(self.subObjects[index]);
            }

            self.lastPos = new Vector2(-1000f, -1000f);
            self.overlayOwner = overlayOwner;
            overlayOwner.selector = self;
            self.bkgRect = new RoundedRect(menu, self, new Vector2(-10f, -30f), self.size + new Vector2(20f, 60f), true);
            self.subObjects.Add(self.bkgRect);
            self.infoLabel = new MenuLabel(menu, self, string.Empty, new Vector2((float)(self.size.x / 2.0 - 100.0), 0.0f), new Vector2(200f, 20f), false);
            self.subObjects.Add(self.infoLabel);
            self.buttons = new Menu.SandboxEditorSelector.Button[Menu.SandboxEditorSelector.Width, Menu.SandboxEditorSelector.Height];
            int counter = 0;
            self.AddButton(new Menu.SandboxEditorSelector.RectButton(menu, self, Menu.SandboxEditorSelector.ActionButton.Action.ClearAll), ref counter);
            for (int index = 0; index < 2; ++index)
                self.AddButton(null, ref counter);
            for (int creaturesUnlocks = MultiplayerUnlocks.CreaturesUnlocks; creaturesUnlocks < MultiplayerUnlocks.CreaturesUnlocks + MultiplayerUnlocks.ItemsUnlocks; ++creaturesUnlocks) {
                if (self.unlocks.SandboxItemUnlocked((MultiplayerUnlocks.SandboxUnlockID)creaturesUnlocks))
                    self.AddButton(new Menu.SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock((MultiplayerUnlocks.SandboxUnlockID)creaturesUnlocks)), ref counter);
                else
                    self.AddButton(new Menu.SandboxEditorSelector.LockedButton(menu, self), ref counter);
            }

            foreach (var sUnlockID in items) {
                if (self.unlocks.SandboxItemUnlocked(sUnlockID))
                    self.AddButton(new Menu.SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock(sUnlockID)), ref counter);
                else
                    self.AddButton(new Menu.SandboxEditorSelector.LockedButton(menu, self), ref counter);
            }

            for (int index = 0; index < MultiplayerUnlocks.CreaturesUnlocks; ++index) {
                if (self.unlocks.SandboxItemUnlocked((MultiplayerUnlocks.SandboxUnlockID)index))
                    self.AddButton(new Menu.SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock((MultiplayerUnlocks.SandboxUnlockID)index)), ref counter);
                else
                    self.AddButton(new Menu.SandboxEditorSelector.LockedButton(menu, self), ref counter);
            }

            foreach (var sUnlockID in creatures) {
                if (self.unlocks.SandboxItemUnlocked(sUnlockID))
                    self.AddButton(new Menu.SandboxEditorSelector.CreatureOrItemButton(menu, self, MultiplayerUnlocks.SymbolDataForSandboxUnlock(sUnlockID)), ref counter);
                else
                    self.AddButton(new Menu.SandboxEditorSelector.LockedButton(menu, self), ref counter);
            }

            self.AddButton(new Menu.SandboxEditorSelector.RectButton(menu, self, Menu.SandboxEditorSelector.ActionButton.Action.Play), Menu.SandboxEditorSelector.Width - 1, 0);
            self.AddButton(new Menu.SandboxEditorSelector.RandomizeButton(menu, self), Menu.SandboxEditorSelector.Width - 6, 0);
            self.AddButton(new Menu.SandboxEditorSelector.ConfigButton(menu, self, Menu.SandboxEditorSelector.ActionButton.Action.ConfigA, 0), Menu.SandboxEditorSelector.Width - 5, 0);
            self.AddButton(new Menu.SandboxEditorSelector.ConfigButton(menu, self, Menu.SandboxEditorSelector.ActionButton.Action.ConfigB, 1), Menu.SandboxEditorSelector.Width - 4, 0);
            self.AddButton(new Menu.SandboxEditorSelector.ConfigButton(menu, self, Menu.SandboxEditorSelector.ActionButton.Action.ConfigC, 2), Menu.SandboxEditorSelector.Width - 3, 0);
            for (int p1 = 0; p1 < Menu.SandboxEditorSelector.Width; ++p1) {
                for (int p2 = 0; p2 < Menu.SandboxEditorSelector.Height; ++p2) {
                    if (self.buttons[p1, p2] != null)
                        self.buttons[p1, p2].Initiate(new IntVector2(p1, p2));
                }
            }

            self.cursors = new List<Menu.SandboxEditorSelector.ButtonCursor>();
        }
    }
}