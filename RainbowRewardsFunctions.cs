using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
// using Obeliskial_Content;
// using Obeliskial_Essentials;
using System.IO;
using static UnityEngine.Mathf;
using UnityEngine.TextCore.LowLevel;
using static RainbowRewards.Plugin;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RainbowRewards
{
    public class RainbowRewardsFunctions
    {
        public static CardData GetRandomCardByClass(Enums.CardClass cardClass, TierRewardData tierReward, string[] alreadyExistingCards)
        {
            CardData _cardData = null;
            List<string> stringList1 = new List<string>(Globals.Instance.CardListNotUpgradedByClass[cardClass]);
            int num10 = UnityEngine.Random.Range(0, 100);
            bool flag2 = true;
            if (tierReward == null)
            {
                tierReward = Globals.Instance.GetTierRewardData(GetTierRewardNum());
            }
            if (AtOManager.Instance?.TeamHaveItem("rainbowrewardsrainbowprismrare") ?? false)
            {
                tierReward = Globals.Instance?.GetTierRewardData(tierReward.TierNum + 1) ?? tierReward;
            }
            if (tierReward == null)
            {
                LogError("GetRandomCardByClass - Null Tier Reward");
                return null;
            }
            while (flag2)
            {
                flag2 = false;
                bool flag3 = false;
                while (!flag3)
                {
                    flag2 = false;
                    _cardData = Globals.Instance.GetCardData(stringList1[UnityEngine.Random.Range(0, stringList1.Count)], false);
                    if (!flag2)
                    {
                        if (num10 < tierReward.Common)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Common)
                                flag3 = true;
                        }
                        else if (num10 < tierReward.Common + tierReward.Uncommon)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Uncommon)
                                flag3 = true;
                        }
                        else if (num10 < tierReward.Common + tierReward.Uncommon + tierReward.Rare)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Rare)
                                flag3 = true;
                        }
                        else if (num10 < tierReward.Common + tierReward.Uncommon + tierReward.Rare + tierReward.Epic)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Epic)
                                flag3 = true;
                        }
                        else if (_cardData.CardRarity == Enums.CardRarity.Mythic)
                            flag3 = true;
                    }
                }
                LogDebug($"GetRandomCardByClass - Adding {_cardData.Id}");
                int rarity = UnityEngine.Random.Range(0, 100);
                string id = _cardData.Id;
                _cardData = Globals.Instance.GetCardData(Functions.GetCardByRarity(rarity, _cardData), false);
                LogDebug($"GetRandomCardByClass - Upgraded Version {_cardData.Id}");
                if ((UnityEngine.Object)_cardData == (UnityEngine.Object)null)
                {
                    flag2 = true;
                }
                else
                {
                    for (int index2 = 0; index2 < alreadyExistingCards.Length; ++index2)
                    {
                        if (alreadyExistingCards[index2] == _cardData.Id)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }

            }
            return _cardData;
        }


        public static int GetTierRewardNum()
        {
            if(RewardsManager.Instance == null)
            {
                TierRewardData tierReward = Traverse.Create(RewardsManager.Instance).Field("tierReward").GetValue<TierRewardData>();
                if(tierReward == null)
                {
                    return 0;
                }
                return tierReward.TierNum;
            }
            if(AtOManager.Instance.GetTownDivinationTier() != null)
            {
                return AtOManager.Instance.GetTownDivinationTier().TierNum;
            }
            if(AtOManager.Instance.GetEventRewardTier() != null)
            {
                return AtOManager.Instance.GetEventRewardTier().TierNum;
            }
            if(AtOManager.Instance.GetTeamNPC().Length != 0)
            {
                return AtOManager.Instance.GetTeamNPCReward().TierNum;
            }            
            return 0;
        }

    }
}

