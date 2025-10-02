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
		[HarmonyPatch(typeof(RewardsManager), "Start")]
		public static void StartPrefix(ref int ___numCardsReward)
		{
			if (IncreaseCardsTo4.Value)
			{
				LogDebug("StartPrefix - Increasing cards to 4");
				___numCardsReward = 4;
			}
		}

		// public static int key = 0;
		// [HarmonyPrefix]
		// [HarmonyPatch(typeof(RewardsManager), "ShowRewards")]
		// public static void ShowRewardsPrefix()
		// {
		// 	key = 0;
		// 	LogDebug("ShowRewardsPrefix - Init");
		// }

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterReward), "Init")]
		public static void InitPrefix()
		{
			CardData _cardData = null;
			if (RewardsManager.Instance == null)
				return;

			int ___numCardsReward = Traverse.Create(RewardsManager.Instance).Field("numCardsReward").GetValue<int>();
			Hero[] ___theTeam = Traverse.Create(RewardsManager.Instance).Field("theTeam").GetValue<Hero[]>();
			TierRewardData ___tierReward = Traverse.Create(RewardsManager.Instance).Field("tierReward").GetValue<TierRewardData>();
			TierRewardData ___tierRewardBase = Traverse.Create(RewardsManager.Instance).Field("tierRewardBase").GetValue<TierRewardData>();
			TierRewardData ___tierRewardInf = Traverse.Create(RewardsManager.Instance).Field("tierRewardInf").GetValue<TierRewardData>();
			Dictionary<int, string[]> cardsByOrderLocal = Traverse.Create(RewardsManager.Instance).Field("cardsByOrder").GetValue<Dictionary<int, string[]>>();

			LogDebug($"ShowRewardsPrefix - ___cardsByOrder {string.Join(", ", cardsByOrderLocal.Values.SelectMany(x => x))}");
			LogDebug($"ShowRewardsPrefix - ___tierReward {___tierReward?.TierNum ?? -1}");
			LogDebug($"ShowRewardsPrefix - ___tierRewardBase {___tierRewardBase?.TierNum ?? -1}");
			LogDebug($"ShowRewardsPrefix - ___tierRewardInf {___tierRewardInf?.TierNum ?? -1}");
			Dictionary<int, string[]> newCardsByOrder = new Dictionary<int, string[]>();

			foreach (int key in cardsByOrderLocal.Keys)
			{
				LogDebug($"ShowRewardsPrefix - key {key}");
				if (cardsByOrderLocal[key] == null || cardsByOrderLocal[key].Length == 0)
				{
					continue;
				}
				LogDebug("ShowRewardsPrefix - Setting State");
				// key++;
				UnityEngine.Random.InitState((AtOManager.Instance.currentMapNode + key + AtOManager.Instance.GetGameId()).GetDeterministicHashCode());
				// int randInt = UnityEngine.Random.Range(0, 100); 
				Hero hero = ___theTeam[key];
				string[] arr2 = new string[___numCardsReward];
				LogDebug($"ShowRewardsPrefix - ___numCardsReward {___numCardsReward}");
				Dictionary<Enums.CardClass, List<string>> dictionary = Globals.Instance.CardListNotUpgradedByClass;

				for (int index1 = 0; index1 < ___numCardsReward; ++index1)
				{
					bool addRainbowCards = !AddRainbowItem.Value || AtOManager.Instance.TeamHaveItem("rainbowrewardsrainbowprism") || AtOManager.Instance.TeamHaveItem("rainbowrewardsrainbowprismrare");
					if (!addRainbowCards)
					{
						continue;
					}
					Enums.CardClass cardClass = ForceOneOfEach.Value ? (Enums.CardClass)index1 : (Enums.CardClass)UnityEngine.Random.Range(0, 4);
					_cardData = GetRandomCardByClass(cardClass, ___tierReward, arr2);
					LogDebug($"ShowRewardsPrefix - Adding  {_cardData?.Id ?? "null card"}");
					if (_cardData == null)
					{
						if(cardsByOrderLocal[key].Length < index1)
						{
							arr2[index1] = cardsByOrderLocal[key][index1];
						}
						else
						{
							arr2[index1] = cardsByOrderLocal[key][0];
						}
						LogDebug($"ShowRewardsPrefix - Null CardData, replaced with {cardsByOrderLocal[key][index1]}");
					}
					arr2[index1] = _cardData.Id;

				}
				newCardsByOrder[key] = Functions.ShuffleArray<string>(arr2);

			}
			LogDebug($"ShowRewardsPrefix - newCardsByOrder {string.Join(", ", newCardsByOrder.Values.SelectMany(x => x))}");
			Traverse.Create(RewardsManager.Instance).Field("cardsByOrder").SetValue(newCardsByOrder);
		}



		[HarmonyPrefix]
		[HarmonyPatch(typeof(AtOManager), "GetTeamNPCReward")]
		public static bool GetTeamNPCRewardPrefix(ref TierRewardData __result, ref AtOManager __instance)
		{
			int num = 0;
			string[] teamNPCAtO = __instance.GetTeamNPC();
			for (int index = 0; index < teamNPCAtO.Length; ++index)
			{
				if (teamNPCAtO[index] != null && teamNPCAtO[index] != "")
				{
					NPCData npcData = Globals.Instance.GetNPC(teamNPCAtO[index]);
					if (npcData != null)
					{
						if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier2")) && (UnityEngine.Object)npcData.UpgradedMob != (UnityEngine.Object)null)
							npcData = npcData.UpgradedMob;
						if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.GetNgPlus() > 0 && npcData.NgPlusMob != null)
							npcData = npcData.NgPlusMob;
						if (npcData != null && MadnessManager.Instance.IsMadnessTraitActive("despair") && (UnityEngine.Object)npcData.HellModeMob != (UnityEngine.Object)null)
							npcData = npcData.HellModeMob;
						if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && (UnityEngine.Object)npcData.TierReward != (UnityEngine.Object)null && npcData.TierReward.TierNum > num)
							num = npcData.TierReward.TierNum;
					}
				}
			}
			__result = Globals.Instance.GetTierRewardData(num);
			return false; // do not run original method
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AtOManager), "GetGoldFromCombat")]
		public static bool GetGoldFromCombatPrefix(ref int __result, ref AtOManager __instance)
		{
			int goldFromCombat = 0;
			string[] teamNPCAtO = __instance.GetTeamNPC();
			if (teamNPCAtO != null)
			{
				for (int index = 0; index < teamNPCAtO.Length; ++index)
				{
					if (teamNPCAtO[index] != null && teamNPCAtO[index] != "")
					{
						NPCData npcData = Globals.Instance.GetNPC(teamNPCAtO[index]);
						if (npcData != null)
						{
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier2")) && (UnityEngine.Object)npcData.UpgradedMob != (UnityEngine.Object)null)
								npcData = npcData.UpgradedMob;
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.GetNgPlus() > 0 && npcData.NgPlusMob != null)
								npcData = npcData.NgPlusMob;
							if (npcData != null && MadnessManager.Instance.IsMadnessTraitActive("despair") && (UnityEngine.Object)npcData.HellModeMob != (UnityEngine.Object)null)
								npcData = npcData.HellModeMob;
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && npcData.GoldReward > 0)
								goldFromCombat += npcData.GoldReward;
						}
					}
				}
			}
			__result = goldFromCombat;
			return false; // do not run original method
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AtOManager), "GetExperienceFromCombat")]
		public static bool GetExperienceFromCombatPrefix(ref int __result, ref AtOManager __instance)
		{
			int experienceFromCombat = 0;
			string[] teamNPCAtO = __instance.GetTeamNPC();
			if (teamNPCAtO != null)
			{
				for (int index = 0; index < teamNPCAtO.Length; ++index)
				{
					if (teamNPCAtO[index] != null && teamNPCAtO[index] != "")
					{
						NPCData npcData = Globals.Instance.GetNPC(teamNPCAtO[index]);
						if (npcData != null)
						{
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier2")) && (UnityEngine.Object)npcData.UpgradedMob != (UnityEngine.Object)null)
								npcData = npcData.UpgradedMob;
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && __instance.GetNgPlus() > 0 && npcData.NgPlusMob != null)
								npcData = npcData.NgPlusMob;
							if (npcData != null && MadnessManager.Instance.IsMadnessTraitActive("despair") && (UnityEngine.Object)npcData.HellModeMob != (UnityEngine.Object)null)
								npcData = npcData.HellModeMob;
							if ((UnityEngine.Object)npcData != (UnityEngine.Object)null && npcData.ExperienceReward > 0)
								experienceFromCombat += npcData.ExperienceReward;
						}
					}
				}
			}
			__result = experienceFromCombat;
			return false; // do not run original method
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