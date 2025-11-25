using NUnit.Framework;
using UnityEngine; 

// You will need to copy your Stat class definition into this test file
// or ensure this file can access the assembly where Stat.cs lives.

public class StatTests
{
    [Test]
    public void Stat_GetValue_ReturnsBaseValue()
    {
        var strengthStat = new Stat
        {
            name = "Strength",
            baseValue = 15
        };

        int result = strengthStat.GetValue();

        // ASSERT: Check the result
        Assert.AreEqual(15, result, "The GetValue method should return the initialized baseValue.");
    }

    [Test]
    public void Stat_BaseValueIncrease_UpdatesValue()
    {
        var vitalityStat = new Stat
        {
            name = "Vitality",
            baseValue = 5
        };
        vitalityStat.baseValue = 6; // Simulate the LevelUp logic

        int result = vitalityStat.GetValue();

        Assert.AreEqual(6, result, "Increasing baseValue should immediately change GetValue.");
    }

    [Test]
    public void Stat_AllowsNegativeValues_ForDebuffs()
    {
        var agilityStat = new Stat
        {
            name = "Agility",
            baseValue = -3
        };

        Assert.AreEqual(-3, agilityStat.GetValue(), "Stats should be able to represent negative modifiers.");
    }
}