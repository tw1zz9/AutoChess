using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class TankTests
{
    [Fact]
    public void Tank_Initialization_CorrectValues()
    {
        // Arrange & Act
        var tank = new Tank(Team.Blue);

        // Assert
        Assert.Equal("Tank", tank.Name);
        Assert.Equal(Team.Blue, tank.Team);
        Assert.Equal(600, tank.Health);
        Assert.Equal(15, tank.Armor);
        Assert.Equal(50, tank.Damage);
        Assert.Equal(1, tank.Level);
        Assert.True(tank.IsAlive());
    }

    [Fact]
    public void Tank_TakeDamage_CalculatesCorrectly()
    {
        // Arrange
        var tank = new Tank(Team.Blue);

        // Act
        tank.TakeDamage(100);

        // Assert - with 15 armor: resistance = 100/(100+15) = 100/115 ≈ 0.8696
        // Final damage = 100 * 0.8696 ≈ 87
        Assert.InRange(tank.Health, 512.9, 513.1); // Allow small floating point differences
    }

    [Fact]
    public void Tank_LevelUp_IncreasesStats()
    {
        // Arrange
        var tank = new Tank(Team.Blue);

        // Act
        tank.LevelUp();

        // Assert
        Assert.Equal(2, tank.Level);
        Assert.Equal(20, tank.Armor); // 15 + 5
        Assert.Equal(75, tank.Damage); // 50 * 1.5
        Assert.Equal(960, tank.Health); // 600 * 1.6^1
    }

    [Fact]
    public void Tank_Ultimate_CanUseWhenAlive()
    {
        // Arrange
        var tank = new Tank(Team.Blue);

        // Assert
        Assert.True(tank.CanUseUltimate());
        Assert.Equal("Taunt", tank.UltimateName);
    }

    [Fact]
    public void Tank_Ultimate_CannotUseWhenDead()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        tank.TakeDamage(1000); // Kill the tank

        // Assert
        Assert.False(tank.CanUseUltimate());
        Assert.False(tank.IsAlive());
    }

    [Fact]
    public void Tank_SelectTarget_WorksCorrectly()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        var enemy = new Tank(Team.Red);

        // Act
        tank.SelectTarget(enemy);

        // Assert
        Assert.Equal(enemy, tank.GetSelectedTarget());
    }

    [Fact]
    public void Tank_SelectTarget_ThrowsOnAlly()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        var ally = new Tank(Team.Blue);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => tank.SelectTarget(ally));
    }

    [Fact]
    public void Tank_Heal_IncreasesHealth()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        tank.TakeDamage(100); // Reduce health

        // Act
        tank.Heal(50);

        // Assert
        Assert.InRange(tank.Health, 562.9, 563.1);
    }

    [Fact]
    public void Tank_Heal_CannotExceedMaxHealth()
    {
        // Arrange
        var tank = new Tank(Team.Blue);
        tank.TakeDamage(10); // Small damage

        // Act
        tank.Heal(100); // Heal more than max

        // Assert
        Assert.Equal(600, tank.Health); // Should be capped at max
    }
}
