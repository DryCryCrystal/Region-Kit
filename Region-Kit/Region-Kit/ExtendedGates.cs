//extended gates by Henpemaz

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RegionKit
{
    public static class ExtendedGates
    {
        public const string ModID = "ExtendedGates";
        public const string Version = "1.1";
        public const string author = "Henpemaz";
        // 1.0 initial release
        // 1.1 13/06/2021 bugfix 6 karma at 5 cap; fix showing open side over karma for inregion minimap

        static Type uwu;

        public static void Enable()
        {
            // Hooking code goose hre

            On.RainWorld.LoadResources += RainWorld_LoadResources;

            On.GateKarmaGlyph.DrawSprites += GateKarmaGlyph_DrawSprites;

            On.RegionGate.ctor += RegionGate_ctor;
            On.RegionGate.Update += RegionGate_Update;
            On.RegionGate.KarmaBlinkRed += RegionGate_KarmaBlinkRed;

            On.RegionGateGraphics.Update += RegionGateGraphics_Update;
            On.RegionGateGraphics.DrawSprites += RegionGateGraphics_DrawSprites;

            On.HUD.Map.GateMarker.ctor += GateMarker_ctor;
            On.HUD.Map.MapData.KarmaOfGate += MapData_KarmaOfGate;


            uwu = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "UwUMod")
                {
                    uwu = asm.GetType("UwUMod.UwUMod");
                }
            }

            On.VirtualMicrophone.NewRoom += VirtualMicrophone_NewRoom;
        }
        public static void Disable()
        {
            On.RainWorld.LoadResources -= RainWorld_LoadResources;

            On.GateKarmaGlyph.DrawSprites -= GateKarmaGlyph_DrawSprites;

            On.RegionGate.ctor -= RegionGate_ctor;
            On.RegionGate.Update -= RegionGate_Update;
            On.RegionGate.KarmaBlinkRed -= RegionGate_KarmaBlinkRed;

            On.RegionGateGraphics.Update -= RegionGateGraphics_Update;
            On.RegionGateGraphics.DrawSprites -= RegionGateGraphics_DrawSprites;

            On.HUD.Map.GateMarker.ctor -= GateMarker_ctor;
            On.HUD.Map.MapData.KarmaOfGate -= MapData_KarmaOfGate;

            On.VirtualMicrophone.NewRoom -= VirtualMicrophone_NewRoom;
        }

        private static void RegionGateGraphics_DrawSprites(On.RegionGateGraphics.orig_DrawSprites orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.gate is ElectricGate && (self.gate as ElectricGate).batteryLeft > 1.1f)
            {
                ElectricGate egg = (self.gate as ElectricGate);
                sLeaser.sprites[self.BatteryMeterSprite].scaleX = 420f;
                float num4 = (!(self.gate as ElectricGate).batteryChanging) ? 0f : 1f;
                sLeaser.sprites[self.BatteryMeterSprite].color = Color.Lerp(RWCustom.Custom.HSL2RGB(0.03f + 0.3f*(Mathf.InverseLerp(1.1f, 30f, egg.batteryLeft)) + UnityEngine.Random.value * (0.035f * num4 + 0.025f), 1f, (0.5f + UnityEngine.Random.value * 0.2f * num4) * Mathf.Lerp(1f, 0.25f, self.darkness)), self.blackColor, 0.5f);
            }
        }

        private static void RegionGateGraphics_Update(On.RegionGateGraphics.orig_Update orig, RegionGateGraphics self)
        {
            orig(self);
            if (self.gate is WaterGate && (self.gate as WaterGate).waterLeft > 2f && self.water != null) self.WaterLevel = 1f; // Caps max display water so it doesnt look silly dark
        }

        /// <summary>
        /// Fix gate noises following the player through rooms
        /// </summary>
        private static void VirtualMicrophone_NewRoom(On.VirtualMicrophone.orig_NewRoom orig, VirtualMicrophone self, Room room)
        {
            orig(self, room);
            for (int i = self.soundObjects.Count - 1; i >= 0; i--)
            {
                if (self.soundObjects[i] is VirtualMicrophone.PositionedSound) // Doesn't make sense that this carries over
                {
                    // I was going to do somehtin supercomplicated like test if controller as loop was in the same room but screw it
                    //VirtualMicrophone.ObjectSound obj = (self.soundObjects[i] as VirtualMicrophone.ObjectSound);
                    //if (obj.controller != null && )
                    self.soundObjects[i].Destroy();
                    self.soundObjects.RemoveAt(i);
                }
            }
        }

        private static void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            LoadAditionalResources(); // Don't want to overwrite, wants to be overwritable, load first :)
            orig(self);
        }

        internal static string[] specialRequirements = // all lowercase here, compared against tolower
        {
            "open",
            "10reinforced",
            "forbidden",
            "comsmark",
            "uwu",
            "glow"
        };

        private static int ParseOrWarn(string req)
        {
            int val = 0;
            if (specialRequirements.Contains(req.ToLower()))
            {
                val = 100 + System.Array.IndexOf(specialRequirements, req.ToLower());
            }
            else
            {
                try
                {
                    if (req.ToLower().EndsWith("alt"))
                    {
                        val = 1000;
                        req = req.Substring(0, req.Length - 3);
                    }
                    val += int.Parse(req) - 1;
                    // Why even clamp it :)
                    // let people try and adapt this for usage with modded karma values
                    //if (result < 1 || result > 10) throw new FormatException("Karma outside of 1-10 range");
                    //result = RWCustom.Custom.IntClamp(result - 1, 0, 9);
                }
                catch (FormatException e)
                {
                    Debug.LogError("Parse error in ExtendedGates");
                    Debug.LogError("Got: '" + req + "'");
                    Debug.LogError("Expected: [" + string.Join(",", specialRequirements) + "] or positive integer");
                    throw e;
                }
            }
            return val;
        }

        #region MAPHOOKS
        private static void GateMarker_ctor(On.HUD.Map.GateMarker.orig_ctor orig, HUD.Map.GateMarker self, HUD.Map map, int room, int karma, bool showAsOpen)
        {
            if (karma > 4) // above 5 karma support
            {
                orig(self, map, room, 0, showAsOpen);
                // Debug.Log("ExtendedGates: Map.GateMarker_ctor got karma" + karma);
                switch (karma)
                {
                    case 100: // open
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaOpen"); // Custom
                        break;
                    case 101: // 10reinforced
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarma10reinforced"); // Custom
                        break;
                    case 102: // forbidden
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaForbidden"); // Custom
                        break;
                    case 103: // comsmark
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaComsmark"); // Custom
                        break;
                    case 104: // uwu
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaUwu"); // Custom
                        break;
                    case 105: // glow
                        self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaGlow"); // Custom
                        break;

                    default: // Alt art gates
                        if (karma >= 1000)
                        {
                            karma -= 1000;
                            //altArt = true; // irrelevant in this case
                        }
                        if (karma > 4)
                        {
                            int? cap = map.hud.rainWorld.progression?.currentSaveState?.deathPersistentSaveData?.karmaCap;
                            if (!cap.HasValue || cap.Value < karma) cap = Mathf.Max(6, karma); ;
                            self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarma" + karma.ToString() + "-" + cap.Value.ToString()); // Vanilla, zero-indexed
                        }
                        else
                        {
                            self.symbolSprite.element = Futile.atlasManager.GetElementWithName("smallKarmaNoRing" + karma);
                        }
                        break;
                }
            }
            else
            {
                orig(self, map, room, karma, showAsOpen);
            }
        }
       
        private static int MapData_KarmaOfGate(On.HUD.Map.MapData.orig_KarmaOfGate orig, HUD.Map.MapData self, PlayerProgression progression, World initWorld, string roomName)
        {
            // Gotta scan it all, progression was loaded on game-start and doesnt account for enabled/disabled regions ?
            foreach (KeyValuePair<string, string> keyValues in CustomRegions.Mod.CustomWorldMod.activatedPacks)
            {
                string path = CustomRegions.Mod.CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string path2 = path + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "extendedLocks.txt";
                if (File.Exists(path2))
                {
                    string[] array = File.ReadAllLines(path2);

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (string.IsNullOrEmpty(array[i]) || string.IsNullOrEmpty(array[i].Trim())) continue;
                        if (array[i].IndexOf("//") > -1)
                        {
                            array[i] = array[i].Substring(array[i].IndexOf("//"));
                        }
                        if (string.IsNullOrEmpty(array[i]) || string.IsNullOrEmpty(array[i].Trim())) continue;
                        string[] array2 = Regex.Split(array[i], " : ");
                        if (array2[0] == roomName)
                        {

                            string req1 = array2[1];
                            string req2 = array2[2];
                            int result = ParseOrWarn(req1);
                            int result2 = ParseOrWarn(req2);
                            if (result > 1000) result -= 1000; // alt art mode irrelevant here
                            if (result2 > 1000) result2 -= 1000; // alt art mode irrelevant here

                            bool thisGateIsFlippedForWhateverReason = false;
                            if (roomName == "GATE_LF_SB" || roomName == "GATE_DS_SB" || roomName == "GATE_HI_CC" || roomName == "GATE_SS_UW")
                            {
                                thisGateIsFlippedForWhateverReason = true;
                            }

                            string[] namearray = Regex.Split(roomName, "_");
                            if (namearray.Length == 3)
                            {
                                for (int j = 0; j < namearray.Length; j++)
                                {
                                    if (namearray[j] == "UX")
                                    {
                                        namearray[j] = "UW";
                                    }
                                    else if (namearray[j] == "SX")
                                    {
                                        namearray[j] = "SS";
                                    }
                                }
                            }
                            if (namearray.Length != 3 || (namearray[1] == namearray[2]) || (namearray[1] != initWorld.region.name && namearray[2] != initWorld.region.name)) // In-region gate support
                            {
                                // Not worht the trouble of telling which "side" the player is looking from the minimap, pick max
                                if (result == 1000) result = 0; // open is less important than any karma values
                                if (result2 == 1000) result2 = 0;
                                return Mathf.Max(result, result2);
                            }

                            if (namearray[1] == initWorld.region.name != thisGateIsFlippedForWhateverReason)
                            {
                                return result;
                            }
                            return result2;
                        }
                    }
                }
            }
            return orig(self, progression, initWorld, roomName);
        }

        #endregion MAPHOOKS

        #region GATEHOOKS

        /// <summary>
        /// Loads karmaGate requirements
        /// </summary>
        private static void RegionGate_ctor(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {
            orig(self, room);

            foreach (KeyValuePair<string, string> keyValues in CustomRegions.Mod.CustomWorldMod.activatedPacks)
            {
                string path = CustomRegions.Mod.CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string path2 = path + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "extendedLocks.txt";
                if (File.Exists(path2))
                {
                    string[] array = File.ReadAllLines(path2);

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (string.IsNullOrEmpty(array[i]) || string.IsNullOrEmpty(array[i].Trim())) continue;
                        if (array[i].IndexOf("//") > -1)
                        {
                            array[i] = array[i].Substring(array[i].IndexOf("//"));
                        }
                        if (string.IsNullOrEmpty(array[i]) || string.IsNullOrEmpty(array[i].Trim())) continue;
                        string[] array2 = Regex.Split(array[i], " : ");
                        if (array2[0] == room.abstractRoom.name)
                        {
                            self.karmaGlyphs[0].Destroy();
                            self.karmaGlyphs[1].Destroy();

                            string req1 = array2[1];
                            string req2 = array2[2];
                            self.karmaRequirements[0] = ParseOrWarn(req1);
                            self.karmaRequirements[1] = ParseOrWarn(req2);
                            bool alt1 = false;
                            if (self.karmaRequirements[0] >= 1000)
                            {
                                self.karmaRequirements[0] -= 1000;
                                alt1 = true;
                            }
                            bool alt2 = false;
                            if (self.karmaRequirements[1] >= 1000)
                            {
                                self.karmaRequirements[1] -= 1000;
                                alt2 = true;
                            }

                            self.karmaGlyphs = new GateKarmaGlyph[2];
                            self.karmaGlyphs[0] = new GateKarmaGlyph(false, self, self.karmaRequirements[0] + (alt1 ? 1000 : 0));
                            room.AddObject(self.karmaGlyphs[0]);
                            self.karmaGlyphs[1] = new GateKarmaGlyph(true, self, self.karmaRequirements[1] + (alt2 ? 1000 : 0));
                            room.AddObject(self.karmaGlyphs[1]);
                            // Above was just this
                            //for (int j = 0; j < 2; j++)
                            //{
                            //    self.karmaGlyphs[j] = new GateKarmaGlyph(j == 1, self, self.karmaRequirements[j]);
                            //    room.AddObject(self.karmaGlyphs[j]);
                            //}

                            if (array2.Length > 3 && array2[3].ToLower() == "multi") // "Infinite" uses
                            {
                                if (self is WaterGate) // sets water level, don't want to get into crazy float craze
                                {
                                    (self as WaterGate).waterLeft = 30f; // ((!room.world.regionState.gatesPassedThrough[room.abstractRoom.gateIndex]) ? 2f : 1f);
                                }
                                else if (self is ElectricGate)
                                {
                                    (self as ElectricGate).batteryLeft = 30f; // ((!room.world.regionState.gatesPassedThrough[room.abstractRoom.gateIndex]) ? 2f : 1f);
                                }
                            }

                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds support for special gate types UwU
        /// </summary>
        private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate self, bool eu)
        {
            int preupdateCounter = self.startCounter;
            orig(self, eu);
            if (!self.room.game.IsStorySession) return; // Paranoid, just like in the base game
            switch (self.mode)
            {
                case RegionGate.Mode.MiddleClosed:
                    int num = self.PlayersInZone();
                    if (num > 0 && num < 3 && !self.dontOpen && self.PlayersStandingStill() && self.EnergyEnoughToOpen && PlayersMeetSpecialRequirements(self))
                    {
                        self.startCounter = preupdateCounter + 1;
                    }

                    if (self.startCounter == 69)
                    {
                        // OPEN THE GATES on the next frame
                        if (self.room.game.GetStorySession.saveStateNumber == 1)
                        {
                            self.Unlock(); // sets savestate thing for monk
                        }
                        self.unlocked = true;
                    }
                    break;

                case RegionGate.Mode.ClosingAirLock:
                    if (preupdateCounter == 69) // We did it... last frame
                    {
                        // if it shouldn't be unlocked, lock it back
                        self.unlocked = (self.room.game.GetStorySession.saveStateNumber == 1 && self.room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates.Contains(self.room.abstractRoom.name)) || (self.room.game.StoryCharacter != 2 && File.Exists(RWCustom.Custom.RootFolderDirectory() + "nifflasmode.txt"));
                    }

                    if (self.room.game.overWorld.worldLoader == null) // In-region gate support
                    {
                        self.waitingForWorldLoader = false;
                    }
                    break;
                case RegionGate.Mode.Closed: // Support for multi-usage gates
                    if (self.EnergyEnoughToOpen) self.mode = RegionGate.Mode.MiddleClosed;
                    break;
            }
        }

        private static bool RegionGate_KarmaBlinkRed(On.RegionGate.orig_KarmaBlinkRed orig, RegionGate self)
        {
            if (self.mode != RegionGate.Mode.MiddleClosed)
            {
                int num = self.PlayersInZone();
                if (num > 0 && num < 3)
                {
                    //self.letThroughDir = (num == 1);
                    if (!self.dontOpen && self.karmaRequirements[(! (num == 1)) ? 1 : 0] == 102) // Forbidden
                    {
                        return true;
                    }
                }
            }
            // Orig doesn't blink if "unlocked", but we know better, forbiden shall stay forbidden
            return orig(self) && !PlayersMeetSpecialRequirements(self);
        }

        private static bool PlayersMeetSpecialRequirements(RegionGate self)
        {
            switch (self.karmaRequirements[(!self.letThroughDir) ? 1 : 0])
            {
                case 100: // open
                    return true;
                case 101: // 10reinforced
                    if (((self.room.game.Players[0].realizedCreature as Player).Karma == 9 && (self.room.game.Players[0].realizedCreature as Player).KarmaIsReinforced) || self.unlocked)
                        return true;
                    break;
                case 102: // forbidden
                    self.startCounter = 0;
                    // caused problems with karmablinkred
                    // self.dontOpen = true; // Hope this works against MONK players smh.
                    break;
                case 103: // comsmark
                    if (self.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark || self.unlocked)
                        return true;
                    break;
                case 104: // uwu
                    if (uwu != null || self.unlocked)
                        return true;
                    break;
                case 105: // glow
                    if (self.room.game.GetStorySession.saveState.theGlow || self.unlocked)
                        return true;
                    break;
                default: // default karma gate handled by the game
                    break;
            }

            return false;
        }

        /// <summary>
        /// Adds support to karma 6 thru 10 for gates, also special gates
        /// </summary>
        private static void GateKarmaGlyph_DrawSprites(On.GateKarmaGlyph.orig_DrawSprites orig, GateKarmaGlyph self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            
            if (self.symbolDirty) // redraw
            {
                bool altArt = DoesPlayerDeserveAltArt(self); // this was probably too costly to call every frame, moved
                if ((!self.gate.unlocked || self.requirement == 102) && (self.requirement > 4 || altArt)) // Custom
                {
                    switch (self.requirement)
                    {
                        case 100: // open
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol0"); // its vanilla
                            break;
                        case 101: // 10reinforced
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol10reinforced"); // Custom
                            break;
                        case 102: // forbidden
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbolForbidden"); // Custom
                            break;
                        case 103: // comsmark
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbolComsmark"); // Custom
                            break;
                        case 104: // uwu
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbolUwu"); // Custom
                            break;
                        case 105: // glow
                            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbolGlow"); // Custom
                            break;

                        default:
                            int trueReq = self.requirement;
                            if (trueReq >= 1000) // alt art
                            {
                                trueReq -= 1000;
                                altArt = true;
                            }
                            if (trueReq > 4)
                            {
                                int cap = (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
                                if (cap < trueReq) cap = Mathf.Max(6, trueReq);
                                sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol" + (trueReq + 1).ToString() + "-" + (cap + 1).ToString() + (altArt ? "alt" : "")); // Custom, 1-indexed
                            }
                            else
                            {
                                sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol" + (trueReq + 1).ToString() + (altArt ? "alt" : "")); // Alt art for vanilla gates
                            }
                            break;
                    }
                    self.symbolDirty = false;
                }
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        private static bool DoesPlayerDeserveAltArt(GateKarmaGlyph self)
        {

            SaveState saveState = (self.room?.game?.session as StoryGameSession)?.saveState;
            if (saveState == null) return false;
            WinState winState = saveState.deathPersistentSaveData?.winState;
            if (winState == null) return false;

            float chieftain = self.room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0);
            if (self.room.game.StoryCharacter == 1)
            {
                chieftain = Mathf.InverseLerp(0.42f, 0.9f, chieftain);
            }
            else
            {
                chieftain = Mathf.InverseLerp(0.1f, 0.8f, chieftain);
            }
            chieftain = Mathf.Floor(chieftain * 20f) / 20f;
            if (chieftain < 0.5f) return false;

            int passages = 0;
            for (int i = 0; i < winState.endgameTrackers.Count; i++)
            {
                if (winState.endgameTrackers[i].GoalFullfilled)
                {
                    passages++;
                }
            }
            if (passages < 6) return false;

            float lizfrend = self.room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Lizards, -1, 0);
            if (lizfrend < -0.45f) return false;

            if (saveState.cycleNumber < 43) return false;

            WinState.BoolArrayTracker wanderer = winState.GetTracker(WinState.EndgameID.Traveller, false) as WinState.BoolArrayTracker;
            if (wanderer == null || wanderer.progress.Count(c => c) < 5) return false;

            return true;
        }

        #endregion GATEHOOKS

        #region ATLASES

        internal static void LoadAditionalResources()
        {
            LoadCustomAtlas("ExtendedGateSymbols", Assembly.GetExecutingAssembly().GetManifestResourceStream("RegionKit.Atlases.ExtendedGates.ExtendedGateSymbols.png"), Assembly.GetExecutingAssembly().GetManifestResourceStream("RegionKit.Atlases.ExtendedGates.ExtendedGateSymbols.json"));
        }

        internal static KeyValuePair<string, string> MetaEntryToKeyVal(string input)
        {
            if (string.IsNullOrEmpty(input)) return new KeyValuePair<string, string>("", "");
            string[] pieces = input.Split(new char[] { ':' }, 2); // No trim option in framework 3.5
            if (pieces.Length == 0) return new KeyValuePair<string, string>("", "");
            if (pieces.Length == 1) return new KeyValuePair<string, string>(pieces[0].Trim(), "");
            return new KeyValuePair<string, string>(pieces[0].Trim(), pieces[1].Trim());
        }

        internal static FAtlas LoadCustomAtlas(string atlasName, Stream textureStream, Stream slicerStream = null, Stream metaStream = null)
        {
            try
            {
                Texture2D imageData = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                byte[] bytes = new byte[textureStream.Length];
                textureStream.Read(bytes, 0, (int)textureStream.Length);
                imageData.LoadImage(bytes);
                Dictionary<string, object> slicerData = null;
                if (slicerStream != null)
                {
                    StreamReader sr = new StreamReader(slicerStream, Encoding.UTF8);
                    slicerData = sr.ReadToEnd().dictionaryFromJson();
                }
                Dictionary<string, string> metaData = null;
                if (metaStream != null)
                {
                    StreamReader sr = new StreamReader(metaStream, Encoding.UTF8);
                    metaData = new Dictionary<string, string>(); // Boooooo no linq and no splitlines, shame on you c#
                    for (string fullLine = sr.ReadLine(); fullLine != null; fullLine = sr.ReadLine())
                    {
                        (metaData as IDictionary<string, string>).Add(MetaEntryToKeyVal(fullLine));
                    }
                }

                return LoadCustomAtlas(atlasName, imageData, slicerData, metaData);
            }
            finally
            {
                textureStream.Close();
                slicerStream?.Close();
                metaStream?.Close();
            }
        }

        internal static FAtlas LoadCustomAtlas(string atlasName, Texture2D imageData, Dictionary<string, object> slicerData, Dictionary<string, string> metaData)
        {
            // Some defaults, metadata can overwrite
            // common snense
            if (slicerData != null) // sprite atlases are mostly unaliesed
            {
                imageData.anisoLevel = 1;
                imageData.filterMode = 0;
            }
            else // Single-image should clamp
            {
                imageData.wrapMode = TextureWrapMode.Clamp;
            }

            if (metaData != null)
            {
                metaData.TryGetValue("aniso", out string anisoValue);
                if (!string.IsNullOrEmpty(anisoValue) && int.Parse(anisoValue) > -1) imageData.anisoLevel = int.Parse(anisoValue);
                metaData.TryGetValue("filterMode", out string filterMode);
                if (!string.IsNullOrEmpty(filterMode) && int.Parse(filterMode) > -1) imageData.filterMode = (FilterMode)int.Parse(filterMode);
                metaData.TryGetValue("wrapMode", out string wrapMode);
                if (!string.IsNullOrEmpty(wrapMode) && int.Parse(wrapMode) > -1) imageData.wrapMode = (TextureWrapMode)int.Parse(wrapMode);
                // Todo -  the other 100 useless params
            }

            // make singleimage atlas
            FAtlas fatlas = new FAtlas(atlasName, imageData, FAtlasManager._nextAtlasIndex);

            if (slicerData == null) // was actually singleimage
            {
                // Done
                if (Futile.atlasManager.DoesContainAtlas(atlasName))
                {
                    Debug.Log("Single-image atlas '" + atlasName + "' being replaced.");
                    Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName); // Unload previous version if present
                }
                if (Futile.atlasManager._allElementsByName.Remove(atlasName)) Debug.Log("Element '" + atlasName + "' being replaced with new one from atlas " + atlasName);
                FAtlasManager._nextAtlasIndex++; // is this guy even used
                Futile.atlasManager.AddAtlas(fatlas); // Simple
                return fatlas;
            }

            // convert to full atlas
            fatlas._elements.Clear();
            fatlas._elementsByName.Clear();
            fatlas._isSingleImage = false;


            //ctrl c
            //ctrl v

            Dictionary<string, object> dictionary2 = (Dictionary<string, object>)slicerData["frames"];
            float resourceScaleInverse = Futile.resourceScaleInverse;
            int num = 0;
            foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
            {
                FAtlasElement fatlasElement = new FAtlasElement();
                fatlasElement.indexInAtlas = num++;
                string text = keyValuePair.Key;
                if (Futile.shouldRemoveAtlasElementFileExtensions)
                {
                    int num2 = text.LastIndexOf(".");
                    if (num2 >= 0)
                    {
                        text = text.Substring(0, num2);
                    }
                }
                fatlasElement.name = text;
                IDictionary dictionary3 = (IDictionary)keyValuePair.Value;
                fatlasElement.isTrimmed = (bool)dictionary3["trimmed"];
                if ((bool)dictionary3["rotated"])
                {
                    throw new NotSupportedException("Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the " + fatlas._dataPath + " atlas.");
                }
                IDictionary dictionary4 = (IDictionary)dictionary3["frame"];
                float num3 = float.Parse(dictionary4["x"].ToString());
                float num4 = float.Parse(dictionary4["y"].ToString());
                float num5 = float.Parse(dictionary4["w"].ToString());
                float num6 = float.Parse(dictionary4["h"].ToString());
                Rect uvRect = new Rect(num3 / fatlas._textureSize.x, (fatlas._textureSize.y - num4 - num6) / fatlas._textureSize.y, num5 / fatlas._textureSize.x, num6 / fatlas._textureSize.y);
                fatlasElement.uvRect = uvRect;
                fatlasElement.uvTopLeft.Set(uvRect.xMin, uvRect.yMax);
                fatlasElement.uvTopRight.Set(uvRect.xMax, uvRect.yMax);
                fatlasElement.uvBottomRight.Set(uvRect.xMax, uvRect.yMin);
                fatlasElement.uvBottomLeft.Set(uvRect.xMin, uvRect.yMin);
                IDictionary dictionary5 = (IDictionary)dictionary3["sourceSize"];
                fatlasElement.sourcePixelSize.x = float.Parse(dictionary5["w"].ToString());
                fatlasElement.sourcePixelSize.y = float.Parse(dictionary5["h"].ToString());
                fatlasElement.sourceSize.x = fatlasElement.sourcePixelSize.x * resourceScaleInverse;
                fatlasElement.sourceSize.y = fatlasElement.sourcePixelSize.y * resourceScaleInverse;
                IDictionary dictionary6 = (IDictionary)dictionary3["spriteSourceSize"];
                float left = float.Parse(dictionary6["x"].ToString()) * resourceScaleInverse;
                float top = float.Parse(dictionary6["y"].ToString()) * resourceScaleInverse;
                float width = float.Parse(dictionary6["w"].ToString()) * resourceScaleInverse;
                float height = float.Parse(dictionary6["h"].ToString()) * resourceScaleInverse;
                fatlasElement.sourceRect = new Rect(left, top, width, height);
                fatlas._elements.Add(fatlasElement);
                fatlas._elementsByName.Add(fatlasElement.name, fatlasElement);
            }

            // This currently doesn't remove elements from old atlases, just removes elements from the manager.
            bool nameInUse = Futile.atlasManager.DoesContainAtlas(atlasName);
            if (!nameInUse)
            {
                // remove duplicated elements and add atlas
                foreach (FAtlasElement fae in fatlas._elements)
                {
                    if (Futile.atlasManager._allElementsByName.Remove(fae.name)) Debug.Log("Element '" + fae.name + "' being replaced with new one from atlas " + atlasName);
                }
                FAtlasManager._nextAtlasIndex++;
                Futile.atlasManager.AddAtlas(fatlas);
            }
            else
            {
                FAtlas other = Futile.atlasManager.GetAtlasWithName(atlasName);
                bool isFullReplacement = true;
                foreach (FAtlasElement fae in other.elements)
                {
                    if (!fatlas._elementsByName.ContainsKey(fae.name)) isFullReplacement = false;
                }
                if (isFullReplacement)
                {
                    // Done, we're good, unload the old and load the new
                    Debug.Log("Atlas '" + atlasName + "' being fully replaced with custom one");
                    Futile.atlasManager.ActuallyUnloadAtlasOrImage(atlasName); // Unload previous version if present
                    FAtlasManager._nextAtlasIndex++;
                    Futile.atlasManager.AddAtlas(fatlas); // Simple
                }
                else
                {
                    // uuuugh
                    // partially unload the old
                    foreach (FAtlasElement fae in fatlas._elements)
                    {
                        if (Futile.atlasManager._allElementsByName.Remove(fae.name)) Debug.Log("Element '" + fae.name + "' being replaced with new one from atlas " + atlasName);
                    }
                    // load the new with a salted name
                    do
                    {
                        atlasName += UnityEngine.Random.Range(0, 9);
                    }
                    while (Futile.atlasManager.DoesContainAtlas(atlasName));
                    fatlas._name = atlasName;
                    FAtlasManager._nextAtlasIndex++;
                    Futile.atlasManager.AddAtlas(fatlas); // Finally
                }
            }
            return fatlas;
        }
        #endregion ATLASES
    }
}
