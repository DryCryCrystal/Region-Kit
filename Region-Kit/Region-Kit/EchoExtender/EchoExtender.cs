using System;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace RegionKit.EchoExtender {
    public static class EchoExtender {
        public static void ApplyHooks() {
            // Tests for spawn
            On.World.SpawnGhost += WorldOnSpawnGhost;
            On.GhostWorldPresence.ctor += GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += GhostWorldPresenceOnGetGhostID;

            // Spawn and customization
            On.Room.Loaded += RoomOnLoaded;
            On.Ghost.ctor += GhostOnCtor;
            On.Ghost.StartConversation += GhostOnStartConversation;
            On.GhostConversation.AddEvents += GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_1 += GhostWorldPresenceOnGhostMode;

            // Save stuff
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgressionOnGetOrInitiateSaveState;
        }

        public static void RemoveHooks() {
            On.World.SpawnGhost -= WorldOnSpawnGhost;
            On.GhostWorldPresence.ctor -= GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID -= GhostWorldPresenceOnGetGhostID;

            // Spawn and customization
            On.Room.Loaded -= RoomOnLoaded;
            On.Ghost.ctor -= GhostOnCtor;
            On.Ghost.StartConversation -= GhostOnStartConversation;
            On.GhostConversation.AddEvents -= GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost -= GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_1 -= GhostWorldPresenceOnGhostMode;

            // Save stuff
            On.DeathPersistentSaveData.ctor -= DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState -= PlayerProgressionOnGetOrInitiateSaveState;
        }

        // A (weak) reference to the game that is required for GhostWorldPresence.GetGhostID static method
        private static WeakReference gameWeakRef;
        private static RainWorldGame gameReference => gameWeakRef?.Target as RainWorldGame;

        private static float GhostWorldPresenceOnGhostMode(On.GhostWorldPresence.orig_GhostMode_1 orig, GhostWorldPresence self, AbstractRoom testRoom, Vector2 worldPos) {
            var result = orig(self, testRoom, worldPos);
            if (!CRSEchoParser.EchoSettings.TryGetValue(self.ghostID, out var settings)) return result;
            var echoEffectLimit = settings.GetRadius(self.world.game.StoryCharacter) * 1000f; //I think 1 screen is like a 1000 so I'm going with that
            Vector2 globalDistance = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(self.world.RoomToWorldPos(new Vector2(), self.ghostRoom.index), self.world.RoomToWorldPos(self.ghostRoom.size.ToVector2() * 20f, self.ghostRoom.index)));
            if (!Custom.DistLess(worldPos, globalDistance, echoEffectLimit)) return 0;
            var someValue = self.DegreesOfSeparation(testRoom); //No clue what this number does
            return someValue == -1
                ? 0.0f
                : (float)(Mathf.Pow(Mathf.InverseLerp(echoEffectLimit, echoEffectLimit / 8f, Vector2.Distance(worldPos, globalDistance)), 2f) * (double)Custom.LerpMap(someValue, 1f, 3f, 0.6f, 0.15f) * (testRoom.layer != self.ghostRoom.layer ? 0.600000023841858 : 1.0));
        }

        // SpawnGhost calls static GhostWorldPresence.GetGhostID without a game reference so here we set it
        private static void WorldOnSpawnGhost(On.World.orig_SpawnGhost orig, World self) {
            gameWeakRef = new WeakReference(self.game);
            orig(self);
        }

        private static void RoomOnLoaded(On.Room.orig_Loaded orig, Room self) {
            // ReSharper disable once InconsistentNaming
            PlacedObject EEGhostSpot = null;
            if (self.game != null) { // Actual ingame loading
                EEGhostSpot = self.roomSettings.placedObjects.FirstOrDefault((v) => v.type == EnumExt_EchoExtender.EEGhostSpot && v.active);
                if (EEGhostSpot != null) EEGhostSpot.type = PlacedObject.Type.GhostSpot; // Temporary switcheroo to trigger vanilla code that handles ghosts
            }

            orig(self);
            // Unswitcheroo
            if (self.game != null && EEGhostSpot != null) EEGhostSpot.type = EnumExt_EchoExtender.EEGhostSpot;
        }

        private static SaveState PlayerProgressionOnGetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int savestatenumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveasdeathorquit) {
            CRSEchoParser.LoadAllCRSPacks();
            return orig(self, savestatenumber, game, setup, saveasdeathorquit);
        }

        private static  void DeathPersistentSaveDataOnCtor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, int slugcat) {
            orig(self, slugcat);
            self.ghostsTalkedTo = new int[Enum.GetValues(typeof(GhostWorldPresence.GhostID)).Length];
        }

        private static bool GhostWorldPresenceOnSpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostid, int karma, int karmacap, int ghostpreviouslyencountered, bool playingasred) {
            var result = orig(ghostid, karma, karmacap, ghostpreviouslyencountered, playingasred);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(ghostid)) return result;
            EchoSettings settings = CRSEchoParser.EchoSettings[ghostid];
            bool SODcondition = settings.SpawnOnThisDifficulty(gameReference.StoryCharacter);
            bool karmaCondition = settings.KarmaCondition(karma, karmacap, gameReference.StoryCharacter);
            bool karmaCapCondition = settings.GetMinimumKarmaCap(gameReference.StoryCharacter) <= karmacap;
            Debug.Log($"[Echo Extender : Info] Getting echo conditions for {ghostid}");
            Debug.Log($"[Echo Extender : Info] Using difficulty {gameReference.StoryCharacter}");
            Debug.Log($"[Echo Extender : Info] Spawn On Difficulty : {(SODcondition ? "Met" : "Not Met")} [Required: <{string.Join(", ", (settings.SpawnOnDifficulty.Length > 0 ? settings.SpawnOnDifficulty : EchoSettings.Default.SpawnOnDifficulty).Select(i => i.ToString()).ToArray())}>]");
            Debug.Log($"[Echo Extender : Info] Minimum Karma : {(karmaCondition ? "Met" : "Not Met")} [Required: {settings.GetMinimumKarma(gameReference.StoryCharacter)}, Having: {karma}]");
            Debug.Log($"[Echo Extender : Info] Minimum Karma Cap : {(karmaCapCondition ? "Met" : "Not Met")} [Required: {settings.GetMinimumKarmaCap(gameReference.StoryCharacter)}, Having: {karmacap}]");
            bool prime = settings.GetPriming(gameReference.StoryCharacter);
            bool primedCond = prime ? ghostpreviouslyencountered == 1 : ghostpreviouslyencountered != 2;
            Debug.Log($"[Echo Extender : Info] Primed : {(primedCond ? "Met" : "Not Met")} [Required: {(prime ? 1 : 0)}, Having {ghostpreviouslyencountered}]");
            Debug.Log($"[Echo Extender : Info] Spawning Echo : {primedCond && SODcondition && karmaCondition && karmaCapCondition}");
            return
                primedCond &&
                SODcondition &&
                karmaCondition &&
                karmaCapCondition;
        }

        private static void GhostConversationOnAddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self) {
            orig(self);
            if (CRSEchoParser.EchoConversations.ContainsKey(self.id)) {
                foreach (string line in CRSEchoParser.EchoConversations[self.id].Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (line.StartsWith("(")) {
                        var difficulties = line.Substring(1, line.IndexOf(")", StringComparison.Ordinal) - 1);
                        foreach (string s in difficulties.Split(',')) {
                            if (int.Parse(s) == self.ghost.room.world.game.StoryCharacter) {
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

        private static void GhostOnStartConversation(On.Ghost.orig_StartConversation orig, Ghost self) {
            orig(self);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            string echoRegionString = self.worldGhost.ghostID.ToString();
            self.currentConversation = new GhostConversation(CRSEchoParser.GetConversationID(echoRegionString), self, self.room.game.cameras[0].hud.dialogBox);
        }

        private static GhostWorldPresence.GhostID GhostWorldPresenceOnGetGhostID(On.GhostWorldPresence.orig_GetGhostID orig, string regionname) {
            var origResult = orig(regionname);
            return CRSEchoParser.EchoIDExists(regionname) ? CRSEchoParser.GetEchoID(regionname) : origResult;
        }

        private static void GhostWorldPresenceOnCtor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostid) {
            orig(self, world, ghostid);
            if (self.ghostRoom is null && CRSEchoParser.ExtendedEchoIDs.Contains(self.ghostID)) {
                self.ghostRoom = world.GetAbstractRoom(CRSEchoParser.EchoSettings[ghostid].GetEchoRoom(world.game.StoryCharacter));
                self.songName = CRSEchoParser.EchoSettings[ghostid].GetEchoSong(world.game.StoryCharacter);
                Debug.Log($"[Echo Extender : GWPCtor] Set Song: {self.songName}");
                Debug.Log($"[Echo Extender : GWPCtor] Set Room {self.ghostRoom?.name}");
            }
        }

        private static void GhostOnCtor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost) {
            orig(self, room, placedobject, worldghost);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            var settings = CRSEchoParser.EchoSettings[self.worldGhost.ghostID];
            self.scale = settings.GetSizeMultiplier(room.game.StoryCharacter) * 0.75f;
            self.defaultFlip = settings.GetDefaultFlip(room.game.StoryCharacter);
        }
    }
}