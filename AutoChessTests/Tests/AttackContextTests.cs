using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class AttackContextTests
{
    [Fact]
    public void AttackContext_Initialization_WorksCorrectly()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        double initialDamage = 100;

        // Act
        var context = new AttackContext(attacker, target, initialDamage);

        // Assert
        Assert.Equal(attacker, context.Attacker);
        Assert.Equal(target, context.Target);
        Assert.Equal(initialDamage, context.Damage);
    }

    [Fact]
    public void AttackContext_Damage_CanBeModified()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act
        context.Damage = 150;

        // Assert
        Assert.Equal(150, context.Damage);
    }

    [Fact]
    public void AttackContext_Damage_CanBeSetToZero()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act
        context.Damage = 0;

        // Assert
        Assert.Equal(0, context.Damage);
    }

    [Fact]
    public void AttackContext_Constructor_ThrowsOnNullAttacker()
    {
        // Arrange
        var target = new Tank(Team.Red);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AttackContext(null!, target, 100));
    }

    [Fact]
    public void AttackContext_Constructor_ThrowsOnNullTarget()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AttackContext(attacker, null!, 100));
    }

    [Fact]
    public void AttackContext_DamageReduction_WorksWithBuffs()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act - simulate damage reduction (e.g., from armor)
        context.Damage *= 0.8; // 20% reduction

        // Assert
        Assert.Equal(80, context.Damage);
    }

    [Fact]
    public void AttackContext_DamageIncrease_WorksWithBuffs()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act - simulate damage increase (e.g., from mage buff)
        context.Damage *= 1.5; // 50% increase

        // Assert
        Assert.Equal(150, context.Damage);
    }

    [Fact]
    public void AttackContext_CriticalHit_Simulation()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act - simulate critical hit
        context.Damage *= 2.0;

        // Assert
        Assert.Equal(200, context.Damage);
    }

    [Fact]
    public void AttackContext_CompleteDamageNegation()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Trickster(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act - simulate perfect dodge
        context.Damage = 0;

        // Assert
        Assert.Equal(0, context.Damage);
    }

    [Fact]
    public void AttackContext_DamageModification_Chain()
    {
        // Arrange
        var attacker = new Mage(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 80); // Mage base damage

        // Act - simulate multiple effects in chain
        context.Damage *= 1.5; // Mage buff
        context.Damage *= 0.87; // Tank armor reduction (approx)

        // Assert
        Assert.Equal(80 * 1.5 * 0.87, context.Damage, 1); // Allow small precision difference
    }
}
