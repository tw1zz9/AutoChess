using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class MageTests
{
    [Fact]
    public void Mage_Initialization_CorrectValues()
    {
        // Arrange & Act
        var mage = new Mage(Team.Blue);

        // Assert
        Assert.Equal("Mage", mage.Name);
        Assert.Equal(Team.Blue, mage.Team);
        Assert.Equal(400, mage.Health);
        Assert.Equal(5, mage.Armor);
        Assert.Equal(80, mage.Damage);
        Assert.Equal(1, mage.Level);
        Assert.True(mage.IsAlive());
        Assert.Equal(1.5, mage.BuffMultiplier);
    }

    [Fact]
    public void Mage_TakeDamage_CalculatesCorrectly()
    {
        // Arrange
        var mage = new Mage(Team.Blue);

        // Act
        mage.TakeDamage(100);

        // Assert - with 5 armor: resistance = 100/(100+5) = 100/105 ≈ 0.9524
        // Final damage = 100 * 0.9524 ≈ 95
        Assert.InRange(mage.Health, 304.9, 305.1);
    }

    [Fact]
    public void Mage_LevelUp_IncreasesStats()
    {
        // Arrange
        var mage = new Mage(Team.Blue);

        // Act
        mage.LevelUp();

        // Assert
        Assert.Equal(2, mage.Level);
        Assert.Equal(8, mage.Armor); // 5 + 3
        Assert.Equal(144, mage.Damage); // 80 * 1.8
        Assert.Equal(1.7, mage.BuffMultiplier); // 1.5 + 0.2
    }

    [Fact]
    public void Mage_Ultimate_RequiresTargetSelection()
    {
        // Arrange
        var mage = new Mage(Team.Blue);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mage.UseUltimate());
    }

    [Fact]
    public void Mage_UltimateTargetSelection_WorksCorrectly()
    {
        // Arrange
        var mage = new Mage(Team.Blue);
        var ally = new Tank(Team.Blue);

        // Act
        mage.SetUltimateTarget(ally);

        // Assert
        Assert.Equal(ally, mage.GetUltimateTarget());
    }

    [Fact]
    public void Mage_UltimateTarget_CannotTargetSelf()
    {
        // Arrange
        var mage = new Mage(Team.Blue);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => mage.SetUltimateTarget(mage));
    }

    [Fact]
    public void Mage_UltimateTarget_CannotTargetEnemy()
    {
        // Arrange
        var mage = new Mage(Team.Blue);
        var enemy = new Tank(Team.Red);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => mage.SetUltimateTarget(enemy));
    }

    [Fact]
    public void Mage_Ultimate_AppliesDamageBuff()
    {
        // Arrange
        var mage = new Mage(Team.Blue);
        var ally = new Tank(Team.Blue);
        var enemy = new Tank(Team.Red);
        var turnManager = new TurnManager();

        mage.SetUltimateTarget(ally);
        mage.UseUltimate();

        // Act - simulate combat turn
        var attackContext = new AttackContext(ally, enemy, 100);
        EventManager.InvokeBeforeAttack(attackContext);

        // Assert - damage should be buffed by 1.5x
        Assert.Equal(150, attackContext.Damage); // 100 * 1.5
    }

    [Fact]
    public void Mage_PerformAttack_CalculatesCorrectly()
    {
        // Arrange
        var mage = new Mage(Team.Blue);
        var enemy = new Tank(Team.Red);
        mage.SelectTarget(enemy);

        // Act
        mage.PerformAttack();

        // Assert - enemy should take ~70 damage, reduced by tank's armor
        Assert.InRange(enemy.Health, 530.4, 530.6); // 600 - 69.57 ≈ 530.43
    }
}
