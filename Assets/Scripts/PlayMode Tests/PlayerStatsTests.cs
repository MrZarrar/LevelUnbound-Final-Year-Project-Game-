using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerStatsTests
{
    [UnityTest]
    public IEnumerator LevelUp_GivesCorrectPoints()
    {

        PlayerStats stats = CreatePlayerStats(out GameObject player);
        stats.unspentPoints = 0;
        stats.xpToNextLevel = 100;

        stats.AddXP(100);

        Assert.AreEqual(1, stats.unspentPoints, "LevelUp should add 1 unspent stat point.");

        Object.Destroy(player);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LevelUp_CarriesOverRemainingXP()
    {
        PlayerStats stats = CreatePlayerStats(out GameObject player);
        stats.xpToNextLevel = 50;

        stats.AddXP(75);

        yield return null;

        Assert.AreEqual(2, stats.level, "Level should increase when reaching XP threshold.");
        Assert.AreEqual(25, stats.currentXP, "Excess XP should carry over after level up.");

        Object.Destroy(player);
    }

    [UnityTest]
    public IEnumerator IncreaseStrength_SpendsUnspentPoint()
    {
        PlayerStats stats = CreatePlayerStats(out GameObject player);
        stats.unspentPoints = 2;
        int startingStrength = stats.strength.baseValue;

        stats.IncreaseStrength();

        yield return null;

        Assert.AreEqual(1, stats.unspentPoints, "Spending a point should decrement the pool.");
        Assert.AreEqual(startingStrength + 1, stats.strength.baseValue, "Strength should increase by one.");

        Object.Destroy(player);
    }

    private PlayerStats CreatePlayerStats(out GameObject player)
    {
        player = new GameObject("TestPlayerStats");
        player.SetActive(false);

        player.AddComponent<Animator>();
        player.AddComponent<HealthSystem>();

        var stats = player.AddComponent<PlayerStats>();
        stats.strength = new Stat { name = "Strength", baseValue = 10 };
        stats.agility = new Stat { name = "Agility", baseValue = 10 };
        stats.intelligence = new Stat { name = "Intelligence", baseValue = 10 };
        stats.vitality = new Stat { name = "Vitality", baseValue = 10 };

        player.SetActive(true);
        return stats;
    }
}