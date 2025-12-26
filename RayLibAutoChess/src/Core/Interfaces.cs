using System;
using System.Collections.Generic;

namespace RayLibAutoChess
{
    public interface ICellable
    {
        ICharacter? ExistingCharacter { get; }
        bool IsOccupied() => ExistingCharacter != null;
        void SetCharacter(ICharacter character);
        void RemoveCharacter();
    }

    public interface ICharacter
    {
        Guid ID { get; }
        Team Team { get; }
        string Name { get; }
        double Health { get; }
        double Armor { get; }
        int Level { get; }

        void LevelUp();
        bool IsAlive();
        void TakeDamage(double damage);
        void Heal(double healAmount);
        void SelectTarget(ICharacter target);
    }

    public interface IPlayersInventory
    {
        void AddUnits(IEnumerable<ICharacter> units);
        void RemoveUnits(IEnumerable<ICharacter> units);
        int Gold { get; }
    }

    public interface IDamager
    {
        double Damage { get; }
        void PerformAttack();
    }

    public interface IHealer : ITargetSelectable
    {
        double HealPower { get; }
        void SelectTarget(ICharacter ally);
        void Heal();
        void HealAll(IEnumerable<ICharacter> allies);
    }

    public interface IEvading
    {
        double DodgeChance { get; }
        bool Dodge();
    }

    public interface IInformational
    {
        string Description();
    }

    public interface IUltimate
    {
        string UltimateName { get; }
        string UltimateDescription { get; }
        int UltimateCost { get; }
        bool CanUseUltimate();
        void UseUltimate();
    }

    // Ультимейт, который требует выбора конкретной цели перед применением.
    public interface ITargetedUltimate : IUltimate
    {
        void SetUltimateTarget(ICharacter target);
        ICharacter? GetUltimateTarget();
    }

    public interface IUltimateActivatable
    {
        bool IsUltimateActive { get; }
        void ApplyActiveUltimateEffect();
        void ApplyOneTurnUltimateEffect(TurnManager turnManager);
    }

    public interface IUltimateResettable
    {
        void ResetUltimateState();
    }

    public interface ITargetSelectable
    {
        ICharacter? GetSelectedTarget();
    }
}
