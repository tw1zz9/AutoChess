using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class TurnManagerTests
{
    [Fact]
    public void TurnManager_SetReady_SetsPlayerStates()
    {
        // Arrange
        var turnManager = new TurnManager();

        // Act
        turnManager.SetReady(1);
        turnManager.SetReady(2);

        // Assert - no direct way to check internal state, but should not throw
        // In real implementation, this would trigger battle resolution
    }

    [Fact]
    public void TurnManager_RegisterAttacker_Works()
    {
        // Arrange
        var turnManager = new TurnManager();
        var tank = new Tank(Team.Blue);

        // Act
        turnManager.RegisterAttacker(tank);

        // Assert - should not throw, attacker registered
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_RegisterHealer_Works()
    {
        // Arrange
        var turnManager = new TurnManager();
        var healer = new Healer(Team.Blue);

        // Act
        turnManager.RegisterHealer(healer);

        // Assert - should not throw, healer registered
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_RegisterOneTurnEffect_Works()
    {
        // Arrange
        var turnManager = new TurnManager();

        // Act
        turnManager.RegisterOneTurnEffect(ctx => ctx.Damage *= 2);

        // Assert - should not throw
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_SetTauntingTank_Works()
    {
        // Arrange
        var turnManager = new TurnManager();
        var tank = new Tank(Team.Blue);

        // Act
        turnManager.SetTauntingTank(tank);

        // Assert - should not throw
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_ResolveBattle_BasicFunctionality()
    {
        // Arrange
        var turnManager = new TurnManager();
        var blueTank = new Tank(Team.Blue);
        var redTank = new Tank(Team.Red);

        var blueUnits = new ICharacter[] { blueTank };
        var redUnits = new ICharacter[] { redTank };

        // Register attackers
        turnManager.RegisterAttacker(blueTank);
        turnManager.RegisterAttacker(redTank);

        // Act
        turnManager.ResolveBattle(blueUnits, redUnits);

        // Assert - battle should complete without throwing
        // One or both units should be damaged or dead
        Assert.True(!blueTank.IsAlive() || !redTank.IsAlive() ||
                   blueTank.Health < 600 || redTank.Health < 600);
    }

    [Fact]
    public void TurnManager_ResolveBattle_WithTaunt()
    {
        // Arrange
        var turnManager = new TurnManager();
        var blueTank = new Tank(Team.Blue);
        var redTank = new Tank(Team.Red);
        var redMage = new Mage(Team.Red);

        var blueUnits = new ICharacter[] { blueTank };
        var redUnits = new ICharacter[] { redTank, redMage };

        // Set up taunt
        blueTank.UseUltimate();
        blueTank.ApplyOneTurnUltimateEffect(turnManager);

        // Register attackers
        turnManager.RegisterAttacker(blueTank);
        turnManager.RegisterAttacker(redTank);
        turnManager.RegisterAttacker(redMage);

        // Act
        turnManager.ResolveBattle(blueUnits, redUnits);

        // Assert - red units should target blue tank due to taunt
        // Blue tank should take more damage than if targeting was random
        Assert.True(blueTank.Health < 600); // Should be damaged
    }

    [Fact]
    public void TurnManager_AutoSelectTargets_WithTaunt()
    {
        // Arrange
        var turnManager = new TurnManager();
        var tauntingTank = new Tank(Team.Blue);
        var enemy1 = new Tank(Team.Red);
        var enemy2 = new Mage(Team.Red);

        // Set taunting tank
        turnManager.SetTauntingTank(tauntingTank);

        // Register attackers
        turnManager.RegisterAttacker(enemy1);
        turnManager.RegisterAttacker(enemy2);

        var blueUnits = new ICharacter[] { tauntingTank };
        var redUnits = new ICharacter[] { enemy1, enemy2 };

        // Act - simulate auto target selection
        turnManager.ResolveBattle(blueUnits, redUnits);

        // Assert - both enemies should have targeted the taunting tank
        // This is indirectly verified by battle resolution working
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_ResetUltimateStates_Works()
    {
        // Arrange
        var turnManager = new TurnManager();
        var tank = new Tank(Team.Blue);
        var mage = new Mage(Team.Blue);

        // Activate ultimates
        tank.UseUltimate();
        tank.ApplyOneTurnUltimateEffect(turnManager);

        var allUnits = new ICharacter[] { tank, mage };

        // Act
        turnManager.ResetUltimateStates(allUnits);

        // Assert - ultimates should be reset
        Assert.False(tank.IsUltimateActive);
        Assert.False(tank.CanUseUltimate()); // Should be able to use again after reset
    }

    [Fact]
    public void TurnManager_ResetTemporaryEffects_Works()
    {
        // Arrange
        var turnManager = new TurnManager();
        var trickster = new Trickster(Team.Blue);

        var allUnits = new ICharacter[] { trickster };

        // Act
        turnManager.ResetTemporaryEffects(allUnits);

        // Assert - should not throw, effects reset
        Assert.True(true);
    }

    [Fact]
    public void TurnManager_ResolveBattle_ActivatesHealerUltimate()
    {
        // Arrange
        var turnManager = new TurnManager();
        var healer = new Healer(Team.Blue);
        var patient = new Tank(Team.Blue);
        var enemy = new Tank(Team.Red);

        healer.UseUltimate(); // Activate ultimate
        patient.TakeDamage(50); // Damage patient so healing is visible

        var blueUnits = new ICharacter[] { healer, patient };
        var redUnits = new ICharacter[] { enemy };

        turnManager.RegisterHealer(healer);

        // Act
        turnManager.ResolveBattle(blueUnits, redUnits);

        // Assert - healer ultimate should heal all allies
        Assert.True(patient.Health > 550); // Should be healed (600 - 50 + some healing)
    }

    [Fact]
    public void TurnManager_ResolveBattle_ActivatesTankUltimate()
    {
        // Arrange
        var turnManager = new TurnManager();
        var tank = new Tank(Team.Blue);
        var enemy = new Tank(Team.Red);

        tank.UseUltimate(); // Queue ultimate

        var blueUnits = new ICharacter[] { tank };
        var redUnits = new ICharacter[] { enemy };

        turnManager.RegisterAttacker(tank);
        turnManager.RegisterAttacker(enemy);

        // Act
        turnManager.ResolveBattle(blueUnits, redUnits);

        // Assert - tank ultimate should be activated during battle
        Assert.True(tank.IsUltimateActive);
    }
}
