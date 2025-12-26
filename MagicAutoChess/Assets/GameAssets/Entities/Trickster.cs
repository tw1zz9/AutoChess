using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Trickster : ICharacter, IDamager, IEvading, IUltimate, IInformational
    {
        private readonly int _maximumObtainableLevel = 4;

        public Guid ID { get; }
        public Team Team { get; }
        public string Name { get; private set; }

        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }
        public double Damage { get; private set; }

        private ICharacter _selectedTarget;
        private double _dodgeChance;
        private readonly double _baseDodgeChance = 0.25;
        public bool IsUltimateActive { get; private set; }

        public double DodgeChance => _dodgeChance;

        public Trickster(Team team)
        {
            Team = team;
            Name = "Sneaky Trickster";
            Health = 450;
            Damage = 90;
            Armor = 8; // 7.4% сопротивления
            _dodgeChance = _baseDodgeChance;
            Level = 1;
            ID = Guid.NewGuid();
        }

        public void SelectTarget(ICharacter target)
        {
            if (target.Team == Team) return;
            _selectedTarget = target;
        }

        public void ResetTarget()
        {
            _selectedTarget = null;
        }

        public ICharacter GetSelectedTarget()
        {
            return _selectedTarget;
        }

        public void PerformAttack()
        {
            if (_selectedTarget == null) return;

            var context = new AttackContext(this, _selectedTarget, Damage);
            EventManager.InvokeBeforeAttack(context);

            if (context.Target.Team == Team || !context.Target.IsAlive()) return;
            context.Target.TakeDamage(context.Damage);
        }

        public void TakeDamage(double damage)
        {
            if (!Dodge())
            {
                // Процентное сопротивление урону: final_damage = damage * (100 / (100 + armor))
                double resistance = 100.0 / (100.0 + Armor);
                double finalDamage = damage * resistance;
                Health = Math.Max(0, Health - finalDamage);
            }
        }

        public bool Dodge() => new Random().NextDouble() < DodgeChance;

        public bool IsAlive()
        {
            if (Health <= 0)
            {
                Name = "(Dead) " + Name;
                return false;
            }
            return true;
        }

        public void LevelUp()
        {
            if (Level == _maximumObtainableLevel) return;
            Damage = (int)(Damage * 1.7); // +70% урона
            Health = (int)(Health * 1.6); // +60% здоровья
            Armor += 5; // +5 брони
            Level++;
        }

        public string UltimateName => "Shadow Step";
        public string UltimateDescription => $"Increases dodge chance to 100% for one turn";
        public int UltimateCost => 5;

        public bool CanUseUltimate() => IsAlive();

        public void UseUltimate()
        {
            // Ультимейт Trickster активируется в фазе подготовки
            // В бою применяется 100% уклонение
            IsUltimateActive = true;
            _dodgeChance = 1.0;
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
        }

        public void ResetDodgeChance()
        {
            _dodgeChance = _baseDodgeChance;
        }

        public string Description() => ToString();
        public override string ToString() => $"{Name} HP:{Health} DMG:{Damage} Dodge:{DodgeChance:P0}";
    }
}
