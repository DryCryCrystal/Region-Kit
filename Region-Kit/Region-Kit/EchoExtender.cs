using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PastebinMachine.EnumExtender;
using RWCustom;
using UnityEngine;

namespace RegionKit {
    public static class EchoExtender {
        private static readonly List<GhostWorldPresence.GhostID> ExtendedEchoIDs = new List<GhostWorldPresence.GhostID>();
        private static readonly Dictionary<GhostWorldPresence.GhostID, string> EchoLocations = new Dictionary<GhostWorldPresence.GhostID, string>();
        private static readonly Dictionary<GhostWorldPresence.GhostID, EchoSettings> EchoSettingsMap = new Dictionary<GhostWorldPresence.GhostID, EchoSettings>();
        private static readonly Dictionary<Conversation.ID, string> EchoConversations = new Dictionary<Conversation.ID, string>();

        private static readonly Dictionary<string, string> EchoSongs = new Dictionary<string, string> {
            {"CC", "NA_32 - Else1"},
            {"SI", "NA_38 - Else7"},
            {"LF", "NA_36 - Else5"},
            {"SH", "NA_34 - Else3"},
            {"UW", "NA_35 - Else4"},
            {"SB", "NA_33 - Else2"},
            {"UNUSED_ECHO", "NA_37 - Else6"}
        };

        private static RainWorldGame _gameInstance;

        public static bool AmIGhostOwner(string regionStr, out GhostWorldPresence.GhostID ghostID) {
            try {
                ghostID = GetGhostID(regionStr);
                return ExtendedEchoIDs.Contains(ghostID);
            }
            catch (Exception) {
                ghostID = GhostWorldPresence.GhostID.NoGhost;
                return false;
            }
        }

        public static bool IsGhostID(string regionStr, out GhostWorldPresence.GhostID id) {
            try {
                id = GetGhostID(regionStr);
                return true;
            }
            catch (Exception) {
                id = GhostWorldPresence.GhostID.NoGhost;
                return false;
            }
        }

        public static GhostWorldPresence.GhostID GetGhostID(string regionStr) => (GhostWorldPresence.GhostID) Enum.Parse(typeof(GhostWorldPresence.GhostID), regionStr);

        public static Conversation.ID GetConversationID(string regionInitials) => (Conversation.ID) Enum.Parse(typeof(Conversation.ID), "Ghost_" + regionInitials);

        public static void ApplyHooks() {
            On.GhostWorldPresence.ctor += HookContainer.GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += HookContainer.GhostWorldPresenceOnGetGhostID;
            On.Ghost.ctor += HookContainer.GhostOnCtor;
            On.Ghost.StartConversation += HookContainer.GhostOnStartConversation;
            On.GhostConversation.AddEvents += HookContainer.GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += HookContainer.GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_1 += HookContainer.GhostWorldPresenceOnGhostMode;
            On.DeathPersistentSaveData.ctor += HookContainer.DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState += HookContainer.PlayerProgressionOnGetOrInitiateSaveState;
            On.RainWorld.Start += HookContainer.RainWorldOnStart;
        }

        public static void LoadCRSPacks() {
            string crsInstallations = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar + "CustomResources";
            foreach (var crsPack in Directory.GetDirectories(crsInstallations)) {
                Debug.Log("[Region Kit Echo Extender : Info] Checking pack " + crsPack.Split(Path.DirectorySeparatorChar).Last() + " for custom Echoes");
                if (!CustomRegions.Mod.CustomWorldMod.activatedPacks.ContainsKey(crsPack.Split(Path.DirectorySeparatorChar).Last())) {
                    Debug.Log("[Region Kit Echo Extender : Info] CRS Pack is disabled, skipping");
                    continue;
                }
                string regions = crsPack + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                if (!Directory.Exists(regions)) continue;
                foreach (var region in Directory.GetDirectories(regions)) {
                    string regionShort = region.Split(Path.DirectorySeparatorChar).Last();
                    Debug.Log("[Region Kit Echo Extender : Info] Found region " + regionShort + "! Checking for Echo.");
                    string echoConv = region + Path.DirectorySeparatorChar + "echoConv.txt";
                    if (!File.Exists(echoConv)) {
                        Debug.Log("[Region Kit Echo Extender : Info] No echoConv.txt found, skipping region!");
                        continue;
                    }

                    string conversationText = File.ReadAllText(echoConv);
                    string settingsPath = region + Path.DirectorySeparatorChar + "echoSettings.txt";
                    EchoSettings settings = File.Exists(settingsPath) ? EchoSettings.FromFile(settingsPath) : EchoSettings.Default;
                    if (IsGhostID(regionShort, out _)) {
                        Debug.Log("[Region Kit Echo Extender : Warning] Region " + regionShort + " already has an echo assigned, skipping!");
                    }
                    else {
                        EnumExtender.AddDeclaration(typeof(GhostWorldPresence.GhostID), regionShort);
                        EnumExtender.AddDeclaration(typeof(Conversation.ID), "Ghost_" + regionShort);
                        EnumExtender.ExtendEnumsAgain();
                        ExtendedEchoIDs.Add(GetGhostID(regionShort));
                        EchoConversations.Add(GetConversationID(regionShort), conversationText);
                        Debug.Log("[Region Kit Echo Extender : Info] Added conversation for echo in region " + regionShort);
                    }

                    if (!EchoSettingsMap.ContainsKey(GetGhostID(regionShort))) EchoSettingsMap.Add(GetGhostID(regionShort), settings);
                    else EchoSettingsMap[GetGhostID(regionShort)] = settings;
                }
            }
        }

        public static void GetEchoLocationInRegion(string regionShort) {
            if (!AmIGhostOwner(regionShort, out var ghostID)) return;
            string crsInstallations = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar + "CustomResources";
            foreach (var crsPack in Directory.GetDirectories(crsInstallations)) {
                Debug.Log("[Region Kit Echo Extender : Info] Checking pack " + crsPack.Split(Path.DirectorySeparatorChar).Last() + " for Echo location");
                if (!CustomRegions.Mod.CustomWorldMod.activatedPacks.ContainsKey(crsPack.Split(Path.DirectorySeparatorChar).Last())) {
                    Debug.Log("[Region Kit Echo Extender : Info] CRS Pack is disabled, skipping");
                    continue;
                }
                string region = crsPack + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + regionShort;
                if (Directory.Exists(region)) {
                    Debug.Log("[Region Kit Echo Extender : Info] Region found! Checking for GhostSpot");
                    string rooms = region + Path.DirectorySeparatorChar + "Rooms";
                    foreach (var file in Directory.GetFiles(rooms)) {
                        string roomTxt = file.Split(Path.DirectorySeparatorChar).Last();
                        if (roomTxt.EndsWith("_Settings.txt")) {
                            if (File.ReadAllText(file).Contains("GhostSpot")) {
                                string roomName = roomTxt.Replace("_Settings.txt", "");
                                Debug.Log("[Region Kit Echo Extender : Info] Registering Echo room " + roomName);
                                if (!EchoLocations.ContainsKey(ghostID)) EchoLocations.Add(ghostID, roomName);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveHooks() {
            On.GhostWorldPresence.ctor -= HookContainer.GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID -= HookContainer.GhostWorldPresenceOnGetGhostID;
            On.Ghost.ctor -= HookContainer.GhostOnCtor;
            On.Ghost.StartConversation -= HookContainer.GhostOnStartConversation;
            On.GhostConversation.AddEvents -= HookContainer.GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost -= HookContainer.GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_1 -= HookContainer.GhostWorldPresenceOnGhostMode;
            On.DeathPersistentSaveData.ctor -= HookContainer.DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState -= HookContainer.PlayerProgressionOnGetOrInitiateSaveState;
            On.RainWorld.Start -= HookContainer.RainWorldOnStart;
            On.WorldLoader.ctor -= HookContainer.WorldLoaderOnCtor;

            _gameInstance = null;
        }

        private static class HookContainer {
            public static void GhostWorldPresenceOnCtor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostid) {
                orig(self, world, ghostid);
                if (!ExtendedEchoIDs.Contains(ghostid)) return;
                self.ghostRoom = EchoLocations.ContainsKey(ghostid) ? world.GetAbstractRoom(EchoLocations[ghostid]) : world.abstractRooms[0];
                self.songName = EchoSettingsMap.ContainsKey(ghostid) ? EchoSettingsMap[ghostid].EchoSong : EchoSettings.Default.EchoSong;
            }

            public static GhostWorldPresence.GhostID GhostWorldPresenceOnGetGhostID(On.GhostWorldPresence.orig_GetGhostID orig, string regionname) {
                GhostWorldPresence.GhostID result = orig(regionname);
                return AmIGhostOwner(regionname, out var ghostID) ? ghostID : result;
            }

            public static void GhostOnCtor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost) {
                orig(self, room, placedobject, worldghost);
                if (!ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
                var settings = EchoSettingsMap[self.worldGhost.ghostID];
                self.scale = settings.EchoSizeMultiplier * 0.75f;
            }

            public static void GhostOnStartConversation(On.Ghost.orig_StartConversation orig, Ghost self) {
                orig(self);
                if (!ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
                var convId = GetConversationID(self.worldGhost.ghostID.ToString());
                self.currentConversation = new GhostConversation(convId, self, self.room.game.cameras[0].hud.dialogBox);
            }

            public static void GhostConversationOnAddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self) {
                orig(self);
                if (EchoConversations.ContainsKey(self.id)) {
                    foreach (string line in EchoConversations[self.id].Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
                        if (line.StartsWith("(")) {
                            var difficulties = line.Substring(1, line.IndexOf(")", StringComparison.Ordinal) - 1);
                            foreach (string s in difficulties.Split(',')) {
                                if (int.Parse(s) == _gameInstance.StoryCharacter) {
                                    self.events.Add(new Conversation.TextEvent(self, 0, Regex.Replace(line, @"^\((\d|(\d+,)+\d)\)", ""), 0));
                                    break;
                                }
                            }

                            continue;
                        }

                        self.events.Add(new Conversation.TextEvent(self, 0, line, 0));
                    }
                }
            }

            public static bool GhostWorldPresenceOnSpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostid, int karma, int karmacap, int ghostpreviouslyencountered, bool playingasred) {
                bool result = orig(ghostid, karma, karmacap, ghostpreviouslyencountered, playingasred);
                if (!ExtendedEchoIDs.Contains(ghostid)) return result;
                EchoSettings settings = EchoSettingsMap.ContainsKey(ghostid) ? EchoSettingsMap[ghostid] : EchoSettings.Default;
                return
                    settings.SpawnOnDifficulty.Contains(_gameInstance.StoryCharacter) &&
                    karmacap >= settings.MinimumKarmaCap - 1 &&
                    karma >= settings.MinimumKarma - 1 &&
                    ghostpreviouslyencountered >= (settings.RequirePriming ? 1 : 0) &&
                    ghostpreviouslyencountered < 2;
            }

            public static float GhostWorldPresenceOnGhostMode(On.GhostWorldPresence.orig_GhostMode_1 orig, GhostWorldPresence self, AbstractRoom testRoom, Vector2 worldPos) {
                var result = orig(self, testRoom, worldPos);
                if (!EchoSettingsMap.TryGetValue(self.ghostID, out var settings)) return result;
                var echoEffectLimit = settings.EffectRadius * 1000f;
                Vector2 globalDistance = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(self.world.RoomToWorldPos(new Vector2(), self.ghostRoom.index), self.world.RoomToWorldPos(self.ghostRoom.size.ToVector2() * 20f, self.ghostRoom.index)));
                if (!Custom.DistLess(worldPos, globalDistance, echoEffectLimit)) return 0;
                var someValue = self.DegreesOfSeparation(testRoom);
                return someValue == -1
                    ? 0.0f
                    : (float) (Mathf.Pow(Mathf.InverseLerp(echoEffectLimit, echoEffectLimit / 8f, Vector2.Distance(worldPos, globalDistance)), 2f) * (double) Custom.LerpMap(someValue, 1f, 3f, 0.6f, 0.15f) * (testRoom.layer != self.ghostRoom.layer ? 0.600000023841858 : 1.0));
            }

            public static void DeathPersistentSaveDataOnCtor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, int slugcat) {
                orig(self, slugcat);
                self.ghostsTalkedTo = new int[Enum.GetValues(typeof(GhostWorldPresence.GhostID)).Length];
            }

            public static SaveState PlayerProgressionOnGetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int savestatenumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveasdeathorquit) {
                _gameInstance = game;
                LoadCRSPacks();
                return orig(self, savestatenumber, game, setup, saveasdeathorquit);
            }

            public static void RainWorldOnStart(On.RainWorld.orig_Start orig, RainWorld self) {
                On.WorldLoader.ctor += WorldLoaderOnCtor;
                orig(self);
            }

            public static void WorldLoaderOnCtor(On.WorldLoader.orig_ctor orig, WorldLoader self, RainWorldGame game, int playercharacter, bool singleroomworld, string worldname, Region region, RainWorldGame.SetupValues setupvalues) {
                orig(self, game, playercharacter, singleroomworld, worldname, region, setupvalues);
                if (region is null) {
                    Debug.Log("[Region Kit Echo Extender : Warning] Region is NULL, skipping getting echo location.");
                }
                else GetEchoLocationInRegion(region.name);
                if (game is null) return;
                _gameInstance = game;
            }
        }

        public struct EchoSettings {
            public float EchoSizeMultiplier;
            public float EffectRadius;
            public bool RequirePriming;
            public int MinimumKarma;
            public int MinimumKarmaCap;
            public int[] SpawnOnDifficulty;
            public string EchoSong;

            public static EchoSettings Default => new EchoSettings {
                EchoSizeMultiplier = 1f,
                EffectRadius = 4f,
                RequirePriming = false,
                MinimumKarma = 0,
                MinimumKarmaCap = 0,
                SpawnOnDifficulty = new[] {0, 1, 2},
                EchoSong = "NA_34 - Else3"
            };

            public static EchoSettings FromFile(string path) {
                Debug.Log("[Echo Extender : Info] Found settings file: " + path);
                string[] rows = File.ReadAllLines(path);
                EchoSettings settings = Default;
                foreach (string row in rows) {
                    if (row.StartsWith("#")) continue;
                    try {
                        string[] split = row.Split(':');
                        switch (split[0].Trim().ToLower()) {
                            case "size":
                                settings.EchoSizeMultiplier = float.Parse(split[1]);
                                Debug.Log("[Region Kit Echo Extender : Info] Settings size multiplier to " + settings.EchoSizeMultiplier);
                                break;
                            case "radius":
                                settings.EffectRadius = float.Parse(split[1]);
                                Debug.Log("[Region Kit Echo Extender : Info] Settings effect radius to " + settings.EffectRadius);
                                break;
                            case "priming":
                                settings.RequirePriming = bool.Parse(split[1]);
                                Debug.Log(settings.RequirePriming ? "[Region Kit Echo Extender : Info] Enabling priming" : "[Region Kit Echo Extender : Info] Disabling priming");
                                break;
                            case "minkarma":
                                settings.MinimumKarma = int.Parse(split[1]);
                                Debug.Log("[Region Kit Echo Extender : Info] Setting minimum karma to " + settings.MinimumKarma);
                                break;
                            case "minkarmacap":
                                settings.MinimumKarmaCap = int.Parse(split[1]);
                                Debug.Log("[Region Kit Echo Extender : Info] Setting minimum karma cap to " + settings.MinimumKarmaCap);
                                break;
                            case "difficulties":
                                settings.SpawnOnDifficulty = split[1].Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                                Debug.Log("[Region KitEcho Extender : Info] Difficulties set to " + string.Join(", ", settings.SpawnOnDifficulty.Select(i => i.ToString()).ToArray()));
                                break;
                            case "echosong":
                                string trimmed = split[1].Trim();
                                settings.EchoSong = EchoSongs.TryGetValue(trimmed, out string song) ? song : trimmed;
                                Debug.Log("[Region Kit Echo Extender : Info] Setting song to " + settings.EchoSong);
                                break;
                        }
                    }

                    catch (Exception) {
                        Debug.Log("[Region Kit Echo Extender : Error] Failed to parse line " + row);
                    }
                }

                return settings;
            }
        }
    }
}