using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class EconomyManagerTests
{
    [Fact]
    public void EconomyManager_StartingGold_IsCorrect()
    {
        // Assert
        Assert.Equal(100, EconomyManager.StartingGold);
    }

    [Fact]
    public void EconomyManager_CalculateRoundReward_Works()
    {
        // Act & Assert
        Assert.Equal(10, EconomyManager.CalculateRoundReward(1));
        Assert.Equal(15, EconomyManager.CalculateRoundReward(2));
        Assert.Equal(20, EconomyManager.CalculateRoundReward(3));
        Assert.Equal(30, EconomyManager.CalculateRoundReward(4));
        Assert.Equal(50, EconomyManager.CalculateRoundReward(5));
    }

    [Fact]
    public void EconomyManager_GetUpgradeCost_Works()
    {
        // Arrange
        var tank = new Tank(Team.Blue);

        // Act & Assert
        Assert.Equal(20, EconomyManager.GetUpgradeCost(tank)); // Level 1

        tank.LevelUp();
        Assert.Equal(30, EconomyManager.GetUpgradeCost(tank)); // Level 2

        tank.LevelUp();
        Assert.Equal(40, EconomyManager.GetUpgradeCost(tank)); // Level 3
    }

    [Fact]
    public void EconomyManager_GetUltimateCost_IsConstant()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        var mage = new Mage(Team.Blue);
        var healer = new Healer(Team.Blue);
        var trickster = new Trickster(Team.Blue);

        // Act & Assert - all ultimates cost 10 gold
        Assert.Equal(10, EconomyManager.GetUltimateCost(tank));
        Assert.Equal(10, EconomyManager.GetUltimateCost(mage));
        Assert.Equal(10, EconomyManager.GetUltimateCost(healer));
        Assert.Equal(10, EconomyManager.GetUltimateCost(trickster));
    }

    [Fact]
    public void EconomyManager_GetUpgradeCost_IncreasesWithLevel()
    {
        // Arrange
        var mage = new Mage(Team.Blue);

        // Act - level up multiple times
        var costs = new List<int>();
        costs.Add(EconomyManager.GetUpgradeCost(mage)); // Level 1

        for (int i = 1; i < 4; i++)
        {
            mage.LevelUp();
            costs.Add(EconomyManager.GetUpgradeCost(mage)); // Level 2, 3, 4
        }

        // Assert - costs should increase
        Assert.True(costs[0] < costs[1]);
        Assert.True(costs[1] < costs[2]);
        Assert.True(costs[2] < costs[3]);
    }

    [Fact]
    public void EconomyManager_GetUpgradeCost_MaxLevelBehavior()
    {
        // Arrange
        var healer = new Healer(Team.Blue);

        // Act - level up to max
        for (int i = 1; i < 4; i++) // Level 1 -> 2 -> 3 -> 4
        {
            healer.LevelUp();
        }

        // Assert - should be able to get cost even at max level
        Assert.Equal(50, EconomyManager.GetUpgradeCost(healer)); // Level 4
    }

    [Fact]
    public void EconomyManager_CalculateRoundReward_IncreasesWithRound()
    {
        // Arrange
        var rewards = new List<int>();

        // Act - get rewards for multiple rounds
        for (int round = 1; round <= 10; round++)
        {
            rewards.Add(EconomyManager.CalculateRoundReward(round));
        }

        // Assert - rewards should generally increase with round number
        for (int i = 1; i < rewards.Count; i++)
        {
            Assert.True(rewards[i] >= rewards[i - 1]); // Non-decreasing
        }

        // Specific known values
        Assert.Equal(10, rewards[0]); // Round 1
        Assert.Equal(15, rewards[1]); // Round 2
        Assert.Equal(20, rewards[2]); // Round 3
        Assert.Equal(30, rewards[3]); // Round 4
        Assert.Equal(50, rewards[4]); // Round 5
    }

    [Fact]
    public void EconomyManager_AllCosts_ArePositive()
    {
        // Arrange
        var characters = new ICharacter[]
        {
            new Tank(Team.Blue),
            new Mage(Team.Blue),
            new Healer(Team.Blue),
            new Trickster(Team.Blue)
        };

        // Act & Assert - all costs should be positive
        foreach (var character in characters)
        {
            Assert.True(EconomyManager.GetUpgradeCost(character) > 0);
            if (character is IUltimate ultimate)
            {
                Assert.True(EconomyManager.GetUltimateCost(ultimate) > 0);
            }
        }

        Assert.True(EconomyManager.StartingGold > 0);
        Assert.True(EconomyManager.CalculateRoundReward(1) > 0);
    }

    [Fact]
    public void EconomyManager_UltimateCost_IndependentOfLevel()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act - level up and check ultimate cost stays the same
        var initialCost = EconomyManager.GetUltimateCost(trickster);

        for (int i = 1; i < 4; i++)
        {
            trickster.LevelUp();
            var currentCost = EconomyManager.GetUltimateCost(trickster);
            Assert.Equal(initialCost, currentCost);
        }
    }

    [Fact]
    public void EconomyManager_RoundRewards_ReasonableValues()
    {
        // Act & Assert - round rewards should be reasonable (not too high, not negative)
        for (int round = 1; round <= 20; round++)
        {
            var reward = EconomyManager.CalculateRoundReward(round);
            Assert.True(reward >= 0);
            Assert.True(reward <= 100); // Reasonable upper bound
        }
    }

    [Fact]
    public void EconomyManager_UpgradeCosts_ProgressiveIncrease()
    {
        // Arrange
        var tank = new Tank(Team.Blue);

        // Act - track upgrade costs across levels
        var level1Cost = EconomyManager.GetUpgradeCost(tank);
        tank.LevelUp();
        var level2Cost = EconomyManager.GetUpgradeCost(tank);
        tank.LevelUp();
        var level3Cost = EconomyManager.GetUpgradeCost(tank);

        // Assert - costs should increase by consistent amounts
        var increase1 = level2Cost - level1Cost;
        var increase2 = level3Cost - level2Cost;

        Assert.Equal(10, increase1); // 20 -> 30
        Assert.Equal(10, increase2); // 30 -> 40
    }

    [Fact]
    public void EconomyManager_StartingGold_SufficientForBasics()
    {
        // Assert - starting gold should be enough for at least one upgrade
        Assert.True(EconomyManager.StartingGold >= EconomyManager.GetUpgradeCost(new Tank(Team.Blue)));
        Assert.True(EconomyManager.StartingGold >= EconomyManager.GetUltimateCost(new Tank(Team.Blue)));
    }
}
