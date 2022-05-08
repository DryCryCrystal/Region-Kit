using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using RegionKit.Utils;


namespace RegionKit.EchoExtender {
    public struct EchoSettings {
        public Dictionary<int, string> EchoRoom;
        public Dictionary<int, float> EchoSizeMultiplier;
        public Dictionary<int, float> EffectRadius;
        public Dictionary<int, bool> RequirePriming;
        public Dictionary<int, int> MinimumKarma;
        public Dictionary<int, int> MinimumKarmaCap;
        public Dictionary<int, float> DefaultFlip;
        public int[] SpawnOnDifficulty;
        public Dictionary<int, string> EchoSong;

        public string GetEchoRoom(int diff) {
            if (EchoRoom.ContainsKey(diff)) return EchoRoom[diff];
            return "";
        }

        public float GetSizeMultiplier(int diff) {
            if (EchoSizeMultiplier.ContainsKey(diff)) return EchoSizeMultiplier[diff];
            return Default.EchoSizeMultiplier[diff];
        }

        public float GetRadius(int diff) {
            if (EffectRadius.ContainsKey(diff)) return EffectRadius[diff];
            return Default.EffectRadius[diff];
        }

        public bool GetPriming(int diff) {
            if (RequirePriming.ContainsKey(diff)) return RequirePriming[diff];
            return Default.RequirePriming[diff];
        }

        public int GetMinimumKarma(int diff) {
            if (MinimumKarma.ContainsKey(diff)) return MinimumKarma[diff] - 1;
            return Default.MinimumKarma[diff] - 1;
        }

        public int GetMinimumKarmaCap(int diff) {
            if (MinimumKarmaCap.ContainsKey(diff)) return MinimumKarmaCap[diff] - 1;
            return Default.MinimumKarmaCap[diff] - 1;
        }

        public string GetEchoSong(int diff) {
            if (EchoSong.ContainsKey(diff)) return EchoSong[diff];
            return Default.EchoSong[diff];
        }

        public bool SpawnOnThisDifficulty(int diff) {
            if (SpawnOnDifficulty.Length > 0) return SpawnOnDifficulty.Contains(diff);
            return Default.SpawnOnDifficulty.Contains(diff);
        }

        public float GetDefaultFlip(int diff) {
            if (DefaultFlip.ContainsKey(diff)) return DefaultFlip[diff];
            return Default.DefaultFlip[diff];
        }

        public static EchoSettings Default;

        static EchoSettings() {
            Default = Empty;
            Default.EchoRoom.AddMultiple("", 0, 1, 2);
            Default.RequirePriming.AddMultiple(true, 0, 1);
            Default.RequirePriming.Add(2, false);
            Default.EffectRadius.AddMultiple(4, 0, 1, 2);
            Default.MinimumKarma.AddMultiple(-1, 0, 1, 2);
            Default.MinimumKarmaCap.AddMultiple(0, 0, 1, 2);
            Default.SpawnOnDifficulty = new[] { 0, 1, 2 };
            Default.EchoSong.AddMultiple("NA_32 - Else1", 0, 1, 2);
            Default.EchoSizeMultiplier.AddMultiple(1, 0, 1, 2);
            Default.DefaultFlip.AddMultiple(0, 0, 1, 2);
        }

        // Hook-friendly
        public static List<int> DefaultDifficulties() => new List<int> { 0, 1, 2 };

        public static EchoSettings Empty => new EchoSettings() {
            EchoRoom = new Dictionary<int, string>(),
            EchoSizeMultiplier = new Dictionary<int, float>(),
            EffectRadius = new Dictionary<int, float>(),
            MinimumKarma = new Dictionary<int, int>(),
            MinimumKarmaCap = new Dictionary<int, int>(),
            RequirePriming = new Dictionary<int, bool>(),
            EchoSong = new Dictionary<int, string>(),
            SpawnOnDifficulty = new int[0],
            DefaultFlip = new Dictionary<int, float>()
        };

        public static EchoSettings FromFile(string path) {
            PetrifiedWood.WriteLine("[Echo Extender : Info] Found settings file: " + path);
            string[] rows = File.ReadAllLines(path);
            EchoSettings settings = Empty;
            foreach (string row in rows) {
                if (row.StartsWith("#") || row.StartsWith("//")) continue;
                try {
                    string[] split = row.Split(':');
                    string pass = split[0].Trim();
                    List<int> difficulties = new List<int>();
                    if (pass.StartsWith("(")) {
                        foreach (string rawNum in pass.Substring(1, pass.IndexOf(')') - 1).SplitAndREE(",")) {
                            if (!int.TryParse(rawNum, out int result)) {
                                PetrifiedWood.WriteLine($"[Echo Extender : Warning] Found a non-integer difficulty '{rawNum}'! Skipping : " + row);
                                continue;
                            }

                            difficulties.Add(result);
                        }

                        pass = pass.Substring(pass.IndexOf(")", StringComparison.Ordinal) + 1);
                    }
                    else difficulties = DefaultDifficulties();

                    switch (pass.Trim().ToLower()) {
                        case "room":
                            settings.EchoRoom.AddMultiple(split[1].Trim(), difficulties);
                            break;
                        case "size":
                            settings.EchoSizeMultiplier.AddMultiple(float.Parse(split[1]), difficulties);
                            break;
                        case "radius":
                            settings.EffectRadius.AddMultiple(float.Parse(split[1]), difficulties);
                            break;
                        case "priming":
                            settings.RequirePriming.AddMultiple(bool.Parse(split[1]), difficulties);
                            break;
                        case "minkarma":
                            settings.MinimumKarma.AddMultiple(int.Parse(split[1]), difficulties);
                            break;
                        case "minkarmacap":
                            settings.MinimumKarmaCap.AddMultiple(int.Parse(split[1]), difficulties);
                            break;
                        case "difficulties":
                            settings.SpawnOnDifficulty = split[1].Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                            break;
                        case "echosong":
                            string trimmed = split[1].Trim();
                            string result = CRSEchoParser.EchoSongs.TryGetValue(trimmed, out string song) ? song : trimmed;
                            settings.EchoSong.AddMultiple(result, difficulties);
                            break;
                        case "defaultflip":
                            settings.DefaultFlip.AddMultiple(float.Parse(split[1]), difficulties);
                            break;
                        default:
                            PetrifiedWood.WriteLine($"[Echo Extender : Warning] Setting '{pass.Trim().ToLower()}' not found! Skipping : " + row);
                            break;
                    }
                }

                catch (Exception) {
                    PetrifiedWood.WriteLine("[Echo Extender : Error] Failed to parse line " + row);
                }
            }

            return settings;
        }

        public bool KarmaCondition(int karma, int karmaCap, int diff) {
            if (GetMinimumKarma(diff) == -2) {
                switch (karmaCap) {
                    case 4:
                        return karma >= 4;
                    case 6:
                        return karma >= 5;
                    default:
                        return karma >= 6;
                }
            }

            return karma >= GetMinimumKarma(diff);
        }
    }
}