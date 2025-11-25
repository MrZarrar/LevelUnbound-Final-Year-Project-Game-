using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerStatsTests
{
    [UnityTest]
    public IEnumerator LevelUp_GivesCorrectPoints()
    {

        GameObject player = new GameObject("TestPlayer");
        player.SetActive(false);

        player.AddComponent<HealthSystem>();
        player.AddComponent<Animator>();

        var stats = player.AddComponent<PlayerStats>();


        stats.strength = new Stat { name = "Strength", baseValue = 10 };
        stats.agility = new Stat { name = "Agility", baseValue = 10 };
        stats.intelligence = new Stat { name = "Intelligence", baseValue = 10 };
        stats.vitality = new Stat { name = "Vitality", baseValue = 10 };

        player.SetActive(true);

        stats.unspentPoints = 0;
        stats.xpToNextLevel = 100;

        stats.AddXP(100);


        Assert.AreEqual(1, stats.unspentPoints, "LevelUp should add 1 unspent stat point.");

        Object.Destroy(player);
        yield return null;
    }
}