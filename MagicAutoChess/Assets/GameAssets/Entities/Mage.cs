using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Mage : ICharacter, IDamager, IUltimate, IInformational
    {
        private readonly int _maximumObtainableLevel = 4;

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
        public bool IsUltimateActive { get; private set; }

        // ��������� ���� �� 1 ���
        public double BuffMultiplier { get; private set; } = 1.5;

        public Mage(Team team)
        {
            Team = team;
            Name = "Mage";
            Health = 400;
            Armor = 5; // 5% сопротивления
            Damage = 80;
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

        public ICharacter GetSelectedTarget()
        {
            return _selectedTarget;
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
            IsUltimateActive = true;
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

            Health = (int)(Health * 1.6); // +60% здоровья
            Armor += 3; // +3 брони
            Damage = (int)(Damage * 1.8); // +80% урона
            BuffMultiplier += 0.2; // +0.2 к баффу

            Level++;
        }

        #endregion

        public string UltimateName => "Arcane Surge";
        public string UltimateDescription => $"Doubles damage buff multiplier for one turn";
        public int UltimateCost => 5;

        public bool CanUseUltimate() => IsAlive();

        public void UseUltimate()
        {
            // Ультимейт Mage активируется через ActivateDamageBuff в фазе подготовки
            // В бою просто проверяем флаг IsUltimateActive
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
        }

        public string Description() => ToString();
        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nDamage: {Damage}\nBuff x{BuffMultiplier}\nLevel: {Level}";
    }
}
