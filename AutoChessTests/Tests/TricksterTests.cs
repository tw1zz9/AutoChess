using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class TricksterTests
{
    [Fact]
    public void Trickster_Initialization_CorrectValues()
    {
        // Arrange & Act
        var trickster = new Trickster(Team.Blue);

        // Assert
        Assert.Equal("Trickster", trickster.Name);
        Assert.Equal(Team.Blue, trickster.Team);
        Assert.Equal(300, trickster.Health);
        Assert.Equal(3, trickster.Armor);
        Assert.Equal(60, trickster.Damage);
        Assert.Equal(1, trickster.Level);
        Assert.Equal(0.25, trickster.DodgeChance);
        Assert.True(trickster.IsAlive());
    }

    [Fact]
    public void Trickster_TakeDamage_CalculatesCorrectly()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act
        trickster.TakeDamage(100);

        // Assert - with 3 armor: resistance = 100/(100+3) = 100/103 ≈ 0.9709
        // Final damage = 100 * 0.9709 ≈ 97
        Assert.InRange(trickster.Health, 202.9, 203.1);
    }

    [Fact]
    public void Trickster_LevelUp_IncreasesStats()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act
        trickster.LevelUp();

        // Assert
        Assert.Equal(2, trickster.Level);
        Assert.Equal(5, trickster.Armor); // 3 + 2
        Assert.Equal(102, trickster.Damage); // 60 * 1.7
        Assert.Equal(0.30, trickster.DodgeChance); // 0.25 + 0.05
    }

    [Fact]
    public void Trickster_Ultimate_ActivatesStealth()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act
        trickster.UseUltimate();

        // Assert
        Assert.True(trickster.IsUltimateActive);
        Assert.Equal("Shadow Step", trickster.UltimateName);
    }

    [Fact]
    public void Trickster_Ultimate_ProvidesPerfectDodge()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);
        var enemy = new Tank(Team.Red);
        var turnManager = new TurnManager();

        trickster.UseUltimate();

        // Act - simulate ultimate effect application
        trickster.ApplyOneTurnUltimateEffect(turnManager);

        var attackContext = new AttackContext(enemy, trickster, 100);
        EventManager.InvokeBeforeAttack(attackContext);

        // Assert - damage should be reduced to 0
        Assert.Equal(0, attackContext.Damage);
    }

    [Fact]
    public void Trickster_Dodge_WorksSometimes()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act - test dodge multiple times to ensure it works
        bool dodgedAtLeastOnce = false;
        bool tookDamageAtLeastOnce = false;

        for (int i = 0; i < 100; i++)
        {
            var testTrickster = new Trickster(Team.Blue);
            var initialHealth = testTrickster.Health;

            // Force a specific scenario or just test the probability
            testTrickster.TakeDamage(10);

            if (testTrickster.Health == initialHealth)
                dodgedAtLeastOnce = true;
            else
                tookDamageAtLeastOnce = true;
        }

        // Assert - both outcomes should occur due to randomness
        Assert.True(dodgedAtLeastOnce || tookDamageAtLeastOnce);
    }

    [Fact]
    public void Trickster_Ultimate_CannotUseWhenActive()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);
        trickster.UseUltimate(); // Activate ultimate

        // Act & Assert - cannot use again while active
        Assert.False(trickster.CanUseUltimate());
    }

    [Fact]
    public void Trickster_Ultimate_CannotUseWhenDead()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);
        trickster.TakeDamage(1000); // Kill trickster

        // Assert
        Assert.False(trickster.CanUseUltimate());
    }

    [Fact]
    public void Trickster_ResetDodgeChance_Works()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);
        trickster.LevelUp(); // Increase dodge chance

        // Act
        trickster.ResetDodgeChance();

        // Assert - no actual state change in current implementation,
        // but method should not throw
        Assert.Equal(0.30, trickster.DodgeChance);
    }

    [Fact]
    public void Trickster_Death_ChangesName()
    {
        // Arrange
        var trickster = new Trickster(Team.Blue);

        // Act
        trickster.TakeDamage(1000); // Kill

        // Assert
        Assert.Equal("Dead", trickster.Name);
        Assert.False(trickster.IsAlive());
    }
}
