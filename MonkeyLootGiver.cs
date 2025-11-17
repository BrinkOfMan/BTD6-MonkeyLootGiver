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
        if (isSandboxMode)
            return;

        int currentRound = InGame.instance.GetSimulation().GetCurrentRound();

        if (!CanEarnRewardThisRound(currentRound))
            return;

        int knowledgeToGain = knowledgePointsToGain;
        GiveKnowledgePoints(knowledgeToGain);
        
        int moneyToGain = monkeyMoneyToGain;
        GiveMonkeyMoney(moneyToGain);
        
        int trophiesToGain = monkeyTrophiesToGain;
        GiveTrophies(trophiesToGain);

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
        tophyLoot.Apply(LootFrom.round100);
    }
    
    private void NotifyKnowledgePointsGained(int knowledge, int money, int trophies, int currentRoundNumber)
    {
        string rewardText = $"Round {currentRoundNumber + 1} beaten. You gained {knowledge} knowledge, {money} money, {trophies} trophies.";
        ModHelper.Msg<MonkeyLootGiver>(rewardText);
        Game.instance.GetPopupScreen().ShowOkPopup(rewardText);
    }
}