using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using RegionKit.Utils;


namespace RegionKit.EchoExtender {
    public static class EchoExtender {

        public static int SlugcatNumber { get; private set; }
        public static void ApplyHooks() {

            // Tests for spawn
            On.World.LoadWorld += WorldOnLoadWorld;
            On.GhostWorldPresence.ctor += GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += GhostWorldPresenceOnGetGhostID;

            // Spawn and customization
            On.Room.Loaded += RoomOnLoaded;
            On.Ghost.ctor += GhostOnCtor;
            On.Ghost.StartConversation += GhostOnStartConversation;
            On.GhostConversation.AddEvents += GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_AbstractRoom_Vector2 += GhostWorldPresenceOnGhostMode;

            // Save stuff
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveDataOnCtor;
            On.StoryGameSession.ctor += StoryGameSessionOnCtor;
        }

        public static void RemoveHooks() {
            On.GhostWorldPresence.ctor -= GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID -= GhostWorldPresenceOnGetGhostID;

            // Spawn and customization
            On.Room.Loaded -= RoomOnLoaded;
            On.Ghost.ctor -= GhostOnCtor;
            On.Ghost.StartConversation -= GhostOnStartConversation;
            On.GhostConversation.AddEvents -= GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost -= GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_AbstractRoom_Vector2 -= GhostWorldPresenceOnGhostMode;

            // Save stuff
            On.DeathPersistentSaveData.ctor -= DeathPersistentSaveDataOnCtor;
            On.StoryGameSession.ctor -= StoryGameSessionOnCtor;
        }
        
        
        private static void StoryGameSessionOnCtor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, int savestatenumber, RainWorldGame game) {
            PetrifiedWood.WriteLine("[Echo Extender : Info] Loading Echoes from Region Mods...");
            CRSEchoParser.LoadAllCRSPacks();
            orig(self, savestatenumber, game);
        }

        private static void WorldOnLoadWorld(On.World.orig_LoadWorld orig, World self, int slugcatnumber, List<AbstractRoom> abstractroomslist, int[] swarmrooms, int[] shelters, int[] gates) {
            SlugcatNumber = slugcatnumber;
            orig(self, slugcatnumber, abstractroomslist, swarmrooms, shelters, gates);
        }
        private static float GhostWorldPresenceOnGhostMode(On.GhostWorldPresence.orig_GhostMode_AbstractRoom_Vector2 orig, GhostWorldPresence self, AbstractRoom testRoom, Vector2 worldPos) {
            var result = orig(self, testRoom, worldPos);
            if (!CRSEchoParser.EchoSettings.TryGetValue(self.ghostID, out var settings)) return result;
            if (testRoom.index == self.ghostRoom.index) return 1f;
            var echoEffectLimit = settings.GetRadius(SlugcatNumber) * 1000f; //I think 1 screen is like a 1000 so I'm going with that
            Vector2 globalDistance = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(self.world.RoomToWorldPos(new Vector2(), self.ghostRoom.index), self.world.RoomToWorldPos(self.ghostRoom.size.ToVector2() * 20f, self.ghostRoom.index)));
            if (!Custom.DistLess(worldPos, globalDistance, echoEffectLimit)) return 0;
            var someValue = self.DegreesOfSeparation(testRoom); //No clue what this number does
            return someValue == -1
                ? 0.0f
                : (float)(Mathf.Pow(Mathf.InverseLerp(echoEffectLimit, echoEffectLimit / 8f, Vector2.Distance(worldPos, globalDistance)), 2f) * (double)Custom.LerpMap(someValue, 1f, 3f, 0.6f, 0.15f) * (testRoom.layer != self.ghostRoom.layer ? 0.600000023841858 : 1.0));
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

        private static  void DeathPersistentSaveDataOnCtor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, int slugcat) {
            orig(self, slugcat);
            self.ghostsTalkedTo = new int[Enum.GetValues(typeof(GhostWorldPresence.GhostID)).Length];
        }

        private static bool GhostWorldPresenceOnSpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostid, int karma, int karmacap, int ghostpreviouslyencountered, bool playingasred) {
            var result = orig(ghostid, karma, karmacap, ghostpreviouslyencountered, playingasred);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(ghostid)) return result;
            EchoSettings settings = CRSEchoParser.EchoSettings[ghostid];
            bool SODcondition = settings.SpawnOnThisDifficulty(SlugcatNumber);
            bool karmaCondition = settings.KarmaCondition(karma, karmacap, SlugcatNumber);
            bool karmaCapCondition = settings.GetMinimumKarmaCap(SlugcatNumber) <= karmacap;
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Getting echo conditions for {ghostid}");
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Using difficulty {SlugcatNumber}");
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Spawn On Difficulty : {(SODcondition ? "Met" : "Not Met")} [Required: <{string.Join(", ", (settings.SpawnOnDifficulty.Length > 0 ? settings.SpawnOnDifficulty : EchoSettings.Default.SpawnOnDifficulty).Select(i => i.ToString()).ToArray())}>]");
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Minimum Karma : {(karmaCondition ? "Met" : "Not Met")} [Required: {(settings.GetMinimumKarma(SlugcatNumber) == -2 ? "Dynamic" : settings.GetMinimumKarma(SlugcatNumber).ToString())}, Having: {karma}]");
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Minimum Karma Cap : {(karmaCapCondition ? "Met" : "Not Met")} [Required: {settings.GetMinimumKarmaCap(SlugcatNumber)}, Having: {karmacap}]");
            bool prime = settings.GetPriming(SlugcatNumber);
            bool primedCond = prime ? ghostpreviouslyencountered == 1 : ghostpreviouslyencountered != 2;
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Primed : {(primedCond ? "Met" : "Not Met")} [Required: {(prime ? 1 : 0)}, Having {ghostpreviouslyencountered}]");
            PetrifiedWood.WriteLine($"[Echo Extender : Info] Spawning Echo : {primedCond && SODcondition && karmaCondition && karmaCapCondition}");
            return
                primedCond &&
                SODcondition &&
                karmaCondition &&
                karmaCapCondition;
        }

        private static void GhostConversationOnAddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self) {
            orig(self);
            if (CRSEchoParser.EchoConversations.ContainsKey(self.id)) {
                foreach (string line in Regex.Split(CRSEchoParser.EchoConversations[self.id], "(\r|\n)+")) {
                    if (line.All(c => char.IsSeparator(c) || c == '\n' || c == '\r')) continue;
                    if (line.StartsWith("(")) {
                        var difficulties = line.Substring(1, line.IndexOf(")", StringComparison.Ordinal) - 1);
                        foreach (string s in difficulties.Split(',')) {
                            if (int.Parse(s) == SlugcatNumber) {
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
                self.ghostRoom = world.GetAbstractRoom(CRSEchoParser.EchoSettings[ghostid].GetEchoRoom(SlugcatNumber));
                self.songName = CRSEchoParser.EchoSettings[ghostid].GetEchoSong(SlugcatNumber);
                PetrifiedWood.WriteLine($"[Echo Extender : Info] Set Song: {self.songName}");
                PetrifiedWood.WriteLine($"[Echo Extender : Info] Set Room {self.ghostRoom?.name}");
            }
        }

        private static void GhostOnCtor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost) {
            orig(self, room, placedobject, worldghost);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            var settings = CRSEchoParser.EchoSettings[self.worldGhost.ghostID];
            self.scale = settings.GetSizeMultiplier(SlugcatNumber) * 0.75f;
            self.defaultFlip = settings.GetDefaultFlip(SlugcatNumber);
        }
    }
}