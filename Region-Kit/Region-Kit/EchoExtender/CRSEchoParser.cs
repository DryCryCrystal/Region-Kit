using System;
using System.Collections.Generic;
using System.IO;
using PastebinMachine.EnumExtender;
using RWCustom;
using UnityEngine;
using RegionKit.Utils;


namespace RegionKit.EchoExtender {
    public static class CRSEchoParser {
        public static readonly Dictionary<Conversation.ID, string> EchoConversations = new Dictionary<Conversation.ID, string>();
        public static readonly HashSet<GhostWorldPresence.GhostID> ExtendedEchoIDs = new HashSet<GhostWorldPresence.GhostID>();
        public static readonly Dictionary<string, string> EchoLocations = new Dictionary<string, string>();
        public static readonly Dictionary<GhostWorldPresence.GhostID, EchoSettings> EchoSettings = new Dictionary<GhostWorldPresence.GhostID, EchoSettings>();

        public static readonly Dictionary<string, string> EchoSongs = new Dictionary<string, string> {
            { "CC", "NA_32 - Else1" },
            { "SI", "NA_38 - Else7" },
            { "LF", "NA_36 - Else5" },
            { "SH", "NA_34 - Else3" },
            { "UW", "NA_35 - Else4" },
            { "SB", "NA_33 - Else2" },
            { "UNUSED", "NA_37 - Else6" }
        };

        public static GhostWorldPresence.GhostID GetEchoID(string regionShort) => (GhostWorldPresence.GhostID)Enum.Parse(typeof(GhostWorldPresence.GhostID), regionShort);
        public static Conversation.ID GetConversationID(string regionShort) => (Conversation.ID)Enum.Parse(typeof(Conversation.ID), "Ghost_" + regionShort);

        public static bool EchoIDExists(string regionShort) {
            try {
                GetEchoID(regionShort);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }


        // ReSharper disable once InconsistentNaming
        public static void LoadAllCRSPacks() {
            foreach (var kvp in CustomRegions.Mod.CustomWorldMod.activatedPacks) {
                PetrifiedWood.WriteLine($"[Echo Extender : Info] Checking pack {kvp.Key} for Echo.");
                var resPath = CustomRegions.Mod.CustomWorldMod.resourcePath + kvp.Value + Path.DirectorySeparatorChar;
                var regPath = resPath + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                if (Directory.Exists(regPath)) {
                    foreach (var region in Directory.GetDirectories(regPath)) {
                        string regInitials = region.Substring(region.Length - 2);
                        string convPath = region + Path.DirectorySeparatorChar + "echoConv.txt";
                        PetrifiedWood.WriteLine($"[Echo Extender : Info] Checking region {regInitials} for Echo.");
                        if (File.Exists(convPath)) {
                            string convText = File.ReadAllText(convPath);
                            convText = ManageXOREncryption(convText, convPath);
                            string settingsPath = region + Path.DirectorySeparatorChar + "echoSettings.txt";
                            var settings = File.Exists(settingsPath) ? RegionKit.EchoExtender.EchoSettings.FromFile(settingsPath) : RegionKit.EchoExtender.EchoSettings.Default;
                            if (!EchoIDExists(regInitials)) {
                                EnumExtender.AddDeclaration(typeof(GhostWorldPresence.GhostID), regInitials);
                                EnumExtender.AddDeclaration(typeof(Conversation.ID), "Ghost_" + regInitials);
                                EnumExtender.ExtendEnumsAgain();
                                ExtendedEchoIDs.Add(GetEchoID(regInitials));
                                EchoConversations.Add(GetConversationID(regInitials), convText);
                                PetrifiedWood.WriteLine("[Echo Extender : Info] Added conversation for echo in region " + regInitials);
                            }
                            else {
                                PetrifiedWood.WriteLine("[Echo Extender : Warning] An echo for this region already exists, skipping.");
                            }

                            EchoSettings.SetKey(GetEchoID(regInitials), settings);
                        }
                        else {
                            PetrifiedWood.WriteLine("[Echo Extender : Info] No conversation file found!");
                        }
                    }
                }
                else {
                    PetrifiedWood.WriteLine("[Echo Extender : Info] Pack doesn't have a regions folder, skipping.");
                }
            }
        }


        public static string ManageXOREncryption(string text, string path) {
            PetrifiedWood.WriteLine("[Echo Extender : Info] Managing XOR Encryption, only supports English so far");
            string xor = Custom.xorEncrypt(text, 54 + 1 + (int)InGameTranslator.LanguageID.English * 7);
            if (xor.StartsWith("###ENCRYPTED")) return xor.Substring("###ENCRYPTED".Length);
            File.WriteAllText(path, Custom.xorEncrypt("###ENCRYPTED" + text, 54 + 1 + (int)InGameTranslator.LanguageID.English * 7));
            return text;
        }
    }
}