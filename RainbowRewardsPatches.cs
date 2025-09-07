using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static RainbowRewards.Plugin;
using static RainbowRewards.CustomFunctions;
using static RainbowRewards.RainbowRewardsFunctions;
using System.Collections.Generic;
using static Functions;
using UnityEngine;
// using Photon.Pun;
using TMPro;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
// using Unity.TextMeshPro;

// Make sure your namespace is the same everywhere
namespace RainbowRewards
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class RainbowRewardsPatches
    {
        public static bool devMode = false; //DevMode.Value;
        public static bool bSelectingPerk = false;
        public static bool IsHost()
        {
            return GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();
        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(RewardsManager), "SetRewards")]
        public static void SetRewardsPrefix(ref int ___numCardsReward)
        {
            if (IncreaseCardsTo4.Value)
            {
                ___numCardsReward = 4;

            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RewardsManager), "ShowRewards")]
        public static void ShowRewardsPrefix(ref Dictionary<int, string[]> ___cardsByOrder,
            int ___numCardsReward,
            Hero[] ___theTeam,
            TierRewardData ___tierReward,
            TierRewardData ___tierRewardBase,
            TierRewardData ___tierRewardInf)
        {
            CardData _cardData = null;

            foreach (int key in ___cardsByOrder.Keys)
            {
                if (___cardsByOrder[key] == null || ___cardsByOrder[key].Length == 0)
                {
                    continue;
                }
                UnityEngine.Random.InitState((AtOManager.Instance.currentMapNode + key + AtOManager.Instance.GetGameId()).GetDeterministicHashCode());
                // int randInt = UnityEngine.Random.Range(0, 100); 
                Hero hero = ___theTeam[key];
                string[] arr = new string[___numCardsReward];
                Dictionary<Enums.CardClass, List<string>> dictionary = Globals.Instance.CardListNotUpgradedByClass;

                for (int index1 = 0; index1 < ___numCardsReward; ++index1)
                {
                    bool addRainbowCards = !AddRainbowItem.Value || AtOManager.Instance.TeamHaveItem("rainbowrewardsrainbowprism") || AtOManager.Instance.TeamHaveItem("rainbowrewardsrainbowprismrare");
                    if (!addRainbowCards)
                    {
                        continue;
                    }
                    Enums.CardClass cardClass = ForceOneOfEach.Value ? (Enums.CardClass)index1 : (Enums.CardClass)UnityEngine.Random.Range(0, 4);
                    _cardData = GetRandomCardByClass(cardClass, ___tierReward, arr);
                    arr[index1] = _cardData.Id;

                }
                ___cardsByOrder[key] = Functions.ShuffleArray<string>(arr);

            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetObeliskScore")]
        public static bool SetObeliskScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetScore")]
        public static bool SetScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetSingularityScore")]
        public static bool SetSingularityScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetObeliskScoreLeaderboard")]
        public static bool SetObeliskScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetScoreLeaderboard")]
        public static bool SetScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetSingularityScoreLeaderboard")]
        public static bool SetSingularityScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }


    }
}