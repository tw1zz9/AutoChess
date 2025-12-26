using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChessTests.Tests;

public class EventManagerTests
{
    [Fact]
    public void EventManager_InvokeBeforeAttack_TriggersHandlers()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        bool handlerCalled = false;
        double originalDamage = context.Damage;

        // Subscribe to event
        EventManager.OnBeforeAttack += (ctx) =>
        {
            handlerCalled = true;
            ctx.Damage *= 2; // Double damage
        };

        // Act
        EventManager.InvokeBeforeAttack(context);

        // Assert
        Assert.True(handlerCalled);
        Assert.Equal(originalDamage * 2, context.Damage);

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_InvokeAfterAttack_TriggersHandlers()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        bool handlerCalled = false;

        // Subscribe to event
        EventManager.OnAfterAttack += (ctx) =>
        {
            handlerCalled = true;
        };

        // Act
        EventManager.InvokeAfterAttack(context);

        // Assert
        Assert.True(handlerCalled);

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_MultipleHandlers_AllExecute()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        int handlerCount = 0;

        // Subscribe multiple handlers
        EventManager.OnBeforeAttack += (ctx) => { handlerCount++; ctx.Damage += 10; };
        EventManager.OnBeforeAttack += (ctx) => { handlerCount++; ctx.Damage += 20; };
        EventManager.OnBeforeAttack += (ctx) => { handlerCount++; ctx.Damage += 30; };

        // Act
        EventManager.InvokeBeforeAttack(context);

        // Assert
        Assert.Equal(3, handlerCount);
        Assert.Equal(100 + 10 + 20 + 30, context.Damage);

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_Clear_RemovesAllHandlers()
    {
        // Arrange
        bool handlerCalled = false;

        EventManager.OnBeforeAttack += (ctx) => handlerCalled = true;

        // Act
        EventManager.Clear();

        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        EventManager.InvokeBeforeAttack(context);

        // Assert
        Assert.False(handlerCalled);
    }

    [Fact]
    public void EventManager_NoHandlers_DoesNotThrow()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Act & Assert - should not throw when no handlers are subscribed
        EventManager.InvokeBeforeAttack(context);
        EventManager.InvokeAfterAttack(context);
    }

    [Fact]
    public void EventManager_HandlerException_DoesNotPreventOtherHandlers()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        bool firstHandlerCalled = false;
        bool secondHandlerCalled = false;

        // Subscribe handlers - first throws, second should still execute
        EventManager.OnBeforeAttack += (ctx) => { firstHandlerCalled = true; throw new Exception("Test exception"); };
        EventManager.OnBeforeAttack += (ctx) => { secondHandlerCalled = true; ctx.Damage = 999; };

        // Act & Assert - should not throw, second handler should execute
        EventManager.InvokeBeforeAttack(context);

        Assert.True(firstHandlerCalled);
        Assert.True(secondHandlerCalled);
        Assert.Equal(999, context.Damage);

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_BeforeAttack_ModifiesDamageCorrectly()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        // Subscribe damage reduction handler (simulating armor)
        EventManager.OnBeforeAttack += (ctx) =>
        {
            // Simulate armor: 20 armor = 100/(100+20) = 0.833 resistance
            ctx.Damage = (int)(ctx.Damage * (100.0 / (100.0 + 20)));
        };

        // Act
        EventManager.InvokeBeforeAttack(context);

        // Assert
        Assert.Equal(83, context.Damage); // 100 * (100/120) ≈ 83.33, truncated to int?

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_AfterAttack_CanTriggerPostDamageEffects()
    {
        // Arrange
        var attacker = new Tank(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 100);

        bool effectTriggered = false;
        double finalDamage = 0;

        // Subscribe after-attack handler
        EventManager.OnAfterAttack += (ctx) =>
        {
            effectTriggered = true;
            finalDamage = ctx.Damage;
        };

        // Act
        EventManager.InvokeAfterAttack(context);

        // Assert
        Assert.True(effectTriggered);
        Assert.Equal(100, finalDamage);

        // Cleanup
        EventManager.Clear();
    }

    [Fact]
    public void EventManager_ComplexEffectChain_Works()
    {
        // Arrange - simulate complex combat effects
        var attacker = new Mage(Team.Blue);
        var target = new Tank(Team.Red);
        var context = new AttackContext(attacker, target, 80); // Mage base damage

        // Before attack effects
        EventManager.OnBeforeAttack += (ctx) => ctx.Damage *= 1.5; // Mage buff
        EventManager.OnBeforeAttack += (ctx) => ctx.Damage *= 0.87; // Armor reduction
        EventManager.OnBeforeAttack += (ctx) => ctx.Damage *= 1.2; // Critical hit

        // After attack effects
        bool damageApplied = false;
        EventManager.OnAfterAttack += (ctx) => damageApplied = true;

        // Act
        EventManager.InvokeBeforeAttack(context);
        EventManager.InvokeAfterAttack(context);

        // Assert
        Assert.True(damageApplied);
        // 80 * 1.5 * 0.87 * 1.2 = 80 * 1.566 ≈ 125.28
        Assert.Equal(125, (int)context.Damage);

        // Cleanup
        EventManager.Clear();
    }
}
