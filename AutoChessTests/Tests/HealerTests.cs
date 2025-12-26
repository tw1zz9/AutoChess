using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class HealerTests
{
    [Fact]
    public void Healer_Initialization_CorrectValues()
    {
        // Arrange & Act
        var healer = new Healer(Team.Blue);

        // Assert
        Assert.Equal("Healer", healer.Name);
        Assert.Equal(Team.Blue, healer.Team);
        Assert.Equal(350, healer.Health);
        Assert.Equal(8, healer.Armor);
        Assert.Equal(70, healer.HealPower);
        Assert.Equal(1, healer.Level);
        Assert.True(healer.IsAlive());
    }

    [Fact]
    public void Healer_TakeDamage_CalculatesCorrectly()
    {
        // Arrange
        var healer = new Healer(Team.Blue);

        // Act
        healer.TakeDamage(100);

        // Assert - with 8 armor: resistance = 100/(100+8) = 100/108 ≈ 0.9259
        // Final damage = 100 * 0.9259 ≈ 93
        Assert.InRange(healer.Health, 256.9, 257.1);
    }

    [Fact]
    public void Healer_LevelUp_IncreasesStats()
    {
        // Arrange
        var healer = new Healer(Team.Blue);

        // Act
        healer.LevelUp();

        // Assert
        Assert.Equal(2, healer.Level);
        Assert.Equal(12, healer.Armor); // 8 + 4
        Assert.Equal(112, healer.HealPower); // 70 * 1.6
        Assert.Equal(70, healer.AreaHealPower); // 50 * 1.4
    }

    [Fact]
    public void Healer_Heal_SingleTarget_WorksCorrectly()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        var patient = new Tank(Team.Blue);
        patient.TakeDamage(100); // Damage patient first
        healer.SelectTarget(patient);

        // Act
        healer.Heal();

        // Assert - should heal for 70 HP
        Assert.InRange(patient.Health, 582.9, 583.1);
    }

    [Fact]
    public void Healer_HealAll_WorksCorrectly()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        var patient1 = new Tank(Team.Blue);
        var patient2 = new Mage(Team.Blue);

        patient1.TakeDamage(100);
        patient2.TakeDamage(100);

        var allies = new ICharacter[] { patient1, patient2 };

        // Act
        healer.HealAll(allies);

        // Assert - both should be healed for 50 HP (AreaHealPower)
        Assert.InRange(patient1.Health, 562.9, 563.1);
        Assert.InRange(patient2.Health, 354.9, 355.1);
    }

    [Fact]
    public void Healer_Ultimate_ActivatesMassHeal()
    {
        // Arrange
        var healer = new Healer(Team.Blue);

        // Act
        healer.UseUltimate();

        // Assert
        Assert.True(healer.IsUltimateActive);
        Assert.Equal("Divine Light", healer.UltimateName);
    }

    [Fact]
    public void Healer_Ultimate_CannotUseWhenDead()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        healer.TakeDamage(1000); // Kill healer

        // Assert
        Assert.False(healer.CanUseUltimate());
    }

    [Fact]
    public void Healer_SelectTarget_CannotTargetEnemies()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        var enemy = new Tank(Team.Red);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => healer.SelectTarget(enemy));
    }

    [Fact]
    public void Healer_SelectTarget_CannotTargetDead()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        var deadAlly = new Tank(Team.Blue);
        deadAlly.TakeDamage(1000); // Kill ally

        // Act & Assert
        Assert.Throws<ArgumentException>(() => healer.SelectTarget(deadAlly));
    }

    [Fact]
    public void Healer_Heal_CannotHealDead()
    {
        // Arrange
        var healer = new Healer(Team.Blue);
        var deadAlly = new Tank(Team.Blue);
        deadAlly.TakeDamage(1000); // Kill ally
        healer.SelectTarget(deadAlly);

        // Act - should not crash
        healer.Heal();

        // Assert - dead ally stays dead
        Assert.False(deadAlly.IsAlive());
    }
}
