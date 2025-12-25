using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Mage : ICharacter, IDamager, IInformational
    {
        private readonly int _maximumObtainableLevel = 3;

        public Guid ID { get; }
        public Team Team { get; }
        public string Name { get; private set; }

        private double _health;
        public double Health
        {
            get => _health;
            private set => _health = value < 0 ? 0 : value;
        }

        public double Armor { get; private set; }
        public int Level { get; private set; }
        public double Damage { get; private set; }

        private ICharacter _selectedTarget;

        // Множитель бафа на 1 ход
        public double BuffMultiplier { get; private set; } = 1.5;

        public Mage(Team team)
        {
            Team = team;
            Name = "Mage";
            Health = 500;
            Armor = 3;
            Damage = 150;
            Level = 1;
            ID = Guid.NewGuid();
        }

        #region Targeting

        public void SelectTarget(ICharacter target)
        {
            if (target.Team == Team) return;
            _selectedTarget = target;
        }

        public void ResetTarget()
        {
            _selectedTarget = null;
        }

        #endregion

        #region IDamager

        public void PerformAttack()
        {
            if (_selectedTarget == null) return;

            var context = new AttackContext(this, _selectedTarget, Damage);
            EventManager.InvokeBeforeAttack(context);

            if (context.Target.Team == Team || !context.Target.IsAlive()) return;

            context.Target.TakeDamage(context.Damage);
        }

        #endregion

        #region Ult

        public void ActivateDamageBuff(TurnManager turnManager)
        {
            turnManager.RegisterOneTurnEffect(ApplyDamageBuff);
        }

        private void ApplyDamageBuff(AttackContext context)
        {
            if (context.Attacker.Team != Team) return;
            context.Damage *= BuffMultiplier;
        }

        #endregion

        #region ICharacter

        public void TakeDamage(double damage)
        {
            Health -= damage;
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

            Health *= 1.8;
            Armor *= 1.8;
            Damage *= 1.5;
            BuffMultiplier *= 1.3;

            Level++;
        }

        #endregion

        public string Description() => ToString();
        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nDamage: {Damage}\nBuff x{BuffMultiplier}\nLevel: {Level}";
    }
}
