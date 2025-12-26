using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Tank : ICharacter, IDamager, IUltimate, IInformational
    {
        private readonly int _maximumObtainableLevel = 4;

        public Guid ID { get; }
        public Team Team { get; }
        public string Name { get; private set; }

        public double Damage { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }

        private ICharacter _selectedTarget;
        public bool IsUltimateActive { get; private set; }

        public Tank(Team team)
        {
            Team = team;
            Name = "Great Paladin";
            Damage = 50;
            Health = 600;
            Armor = 25; // 20% сопротивления урону
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

        public void ActivateTaunt(TurnManager turnManager)
        {
            IsUltimateActive = true;
            turnManager.RegisterOneTurnEffect(ApplyTaunt);
        }

        private void ApplyTaunt(AttackContext context)
        {
            if (context.Attacker.Team == Team)
                context.Target = this;
        }

        public void TakeDamage(double damage)
        {
            // Процентное сопротивление урону: final_damage = damage * (100 / (100 + armor))
            double resistance = 100.0 / (100.0 + Armor);
            double finalDamage = damage * resistance;
            Health = Math.Max(0, Health - finalDamage);
        }

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
            Damage = (int)(Damage * 1.5); // +50% урона
            Health = (int)(Health * 1.7); // +70% здоровья
            Armor += 10; // +10 брони
            Level++;
        }

        public string UltimateName => "Taunt";
        public string UltimateDescription => $"Forces all enemy attacks to target this unit for one turn";
        public int UltimateCost => 5;

        public bool CanUseUltimate() => IsAlive();

        public void UseUltimate()
        {
            // Ультимейт Tank активируется через ActivateTaunt в фазе подготовки
            // В бою просто проверяем флаг IsUltimateActive
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
        }

        public string Description() => ToString();
        public override string ToString() => $"{Name} HP:{Health} DMG:{Damage}";
    }
}
