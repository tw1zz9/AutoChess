using System;
using System.Collections.Generic;

namespace GameAssets.Interfaces
{
    public interface IDamager
    {
        double Damage { get; }
        void PerformAttack();
    }

    public interface IHealer
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
}