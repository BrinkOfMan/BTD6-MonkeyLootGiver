using MelonLoader;
using BTD_Mod_Helper;
using MonkeyLootGiver;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Models.Store.Loot;
using Il2CppAssets.Scripts.Models.Store;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity;
using BTD_Mod_Helper.Extensions;
using System.Linq;

[assembly: MelonInfo(typeof(MonkeyLootGiver.MonkeyLootGiver), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace MonkeyLootGiver;

/// <summary>
/// [Overview of the mod]
/// This mod gives the player monkey loot on specific rounds.
/// The purpose of the mod is intended to make late game more worthwhile. Does not work in Sandbox.
/// </summary>
public class MonkeyLootGiver : BloonsTD6Mod
{

    public ModSettingInt knowledgePointsToGain = new ModSettingInt(1)
    {
        min = 0,
        max = 5,
        slider = true
    };
    
    public ModSettingInt monkeyMoneyToGain = new ModSettingInt(1)
    {
        min = 0,
        max = 500,
        slider = true
    };
    
    public ModSettingInt monkeyTrophiesToGain = new ModSettingInt(1)
    {
        min = 0,
        max = 100,
        slider = true
    };

    public ModSettingInt howManyRoundsBetweenRewards = new ModSettingInt(1)
    {
        min = 1,
        max = 25,
        slider = true
    };

    private bool isSandboxMode;
    
    public override void OnApplicationStart()
    {
        ModHelper.Msg<MonkeyLootGiver>("MonkeyLootGiver loaded!");
    }


    public override void OnMatchStart()
    {
        isSandboxMode = IsSandboxMode();
    }
    
    public override void OnRoundEnd()
    {
        TryAddLoot();
    }

    public void TryAddLoot()
    {
        // doesn't work in sandbox mode!
        if (isSandboxMode)
            return;

        int currentRound = InGame.instance.GetSimulation().GetCurrentRound();

        // don't run if it's not the correct round
        if (!CanEarnRewardThisRound(currentRound))
            return;

        // apply knowledge points.
        int knowledgeToGain = knowledgePointsToGain;
        GiveKnowledgePoints(knowledgeToGain);
        // apply money points.
        int moneyToGain = monkeyMoneyToGain;
        GiveMonkeyMoney(moneyToGain);
        // apply trophies.
        int trophiesToGain = monkeyTrophiesToGain;
        GiveTrophies(trophiesToGain);

        // show popup and debug message.
        NotifyKnowledgePointsGained(knowledgeToGain, moneyToGain, trophiesToGain, currentRound);
    }
    
    private bool IsSandboxMode()
    {
        return InGame.instance.mapEditorInSandboxMode || InGame.instance.GetSimulation().sandbox;
    }
    
    public bool IsDesiredRound(int currentRound, int roundsBetweenRewards)
    {
        const int firstRewardRound = 99;
        const int lastRewardRound = 199;

        if (currentRound < firstRewardRound || currentRound > lastRewardRound)
            return false;

        return (currentRound - firstRewardRound) % roundsBetweenRewards == 0;
    }

    
    private bool CanEarnRewardThisRound(int currentRound)
    {
        int roundsBetweenRewards = howManyRoundsBetweenRewards;
        bool isDesiredRound = IsDesiredRound(currentRound, roundsBetweenRewards);

        // can only earn rewards if the player beat the desired round.
        return isDesiredRound;
    }
    
    private void GiveKnowledgePoints(int amount)
    {
        KnowledgePointsLoot knowledgePointsLoot = new KnowledgePointsLoot(amount);
        knowledgePointsLoot.Apply(LootFrom.round100);
    }
    
    private void GiveMonkeyMoney(int amount)
    {
        MonkeyMoneyLoot monkeyMoneyLoot = new MonkeyMoneyLoot(amount);
        monkeyMoneyLoot.Apply(LootFrom.round100);
    }
    
    private void GiveTrophies(int amount)
    {
        TrophyLoot tophyLoot = new TrophyLoot(amount);
        tophyLoot.Apply(LootFrom.odyssey);
    }

    /// <summary>
    /// Notifies the player that they have gained knowledge points.
    /// </summary>
    /// <param name="amount">The number of knowledge points that the user gained.</param>
    private void NotifyKnowledgePointsGained(int knowledge, int money, int trophies, int currentRoundNumber)
    {
        string rewardText = $"Round {currentRoundNumber + 1} beaten. You gained {knowledge} knowledge, {money} money, {trophies} trophies.";
        ModHelper.Msg<MonkeyLootGiver>(rewardText);
        Game.instance.GetPopupScreen().ShowOkPopup(rewardText);
    }
}