using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Trickster : ICharacter, IDamager, IEvading, IInformational
    {
        private readonly int _maximumObtainableLevel = 3;

        public Guid ID { get; }
        public Team Team { get; }
        public string Name { get; private set; }

        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }
        public double Damage { get; private set; }

        private ICharacter _selectedTarget;
        private double _dodgeChance;

        public double DodgeChance => _dodgeChance;

        public Trickster(Team team)
        {
            Team = team;
            Name = "Sneaky Trickster";
            Health = 650;
            Damage = 200;
            Armor = 5;
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
                Health = Math.Max(0, Health - damage / Armor);
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
            Damage *= 1.5;
            Health *= 1.5;
            Armor *= 1.5;
            Level++;
        }

        public string Description() => ToString();
        public override string ToString() => $"{Name} HP:{Health} DMG:{Damage}";
    }
}
