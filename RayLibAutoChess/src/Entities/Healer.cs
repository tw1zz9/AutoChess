using System;
using System.Collections.Generic;
using System.Linq;

namespace RayLibAutoChess.Entities
{
    public class Healer : ICharacter, IHealer, IUltimate, IInformational, IUltimateActivatable, IUltimateResettable, ITargetSelectable
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
        public double HealPower { get; private set; }

        private ICharacter? _selectedTarget;
        public bool IsUltimateActive { get; private set; }
        private bool _deadNameSet;

        public double AreaHealPower { get; private set; } = 50;

        public Healer(Team team)
        {
            Team = team;
            Name = "Healer";
            Health = 350;
            Armor = 8;
            HealPower = 70;
            Level = 1;
            ID = Guid.NewGuid();
        }

        public void SelectTarget(ICharacter target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target), "Target cannot be null.");

            if (target.Team != Team)
                throw new ArgumentException("Healer can only target allied units.", nameof(target));

            if (!target.IsAlive())
                throw new ArgumentException("Cannot target dead units.", nameof(target));

            _selectedTarget = target;
        }

        public void ResetTarget()
        {
            _selectedTarget = null;
        }

        public void Heal()
        {
            if (_selectedTarget == null || !_selectedTarget.IsAlive()) return;

            _selectedTarget.Heal(HealPower);
            Console.WriteLine($"{Name} healed {_selectedTarget.Name} for {HealPower} health");
        }

        public void HealAll(IEnumerable<ICharacter> allies)
        {
            foreach (var ally in allies.Where(a => a.IsAlive()))
            {
                ally.Heal(AreaHealPower);
            }
            Console.WriteLine($"{Name} healed all allies for {AreaHealPower} health each");
        }

        public void ActivateMassHeal()
        {
            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            if (IsUltimateActive)
                throw new InvalidOperationException("Ultimate is already active.");

            IsUltimateActive = true;
        }

        public void TakeDamage(double damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative. Use Heal() method instead.", nameof(damage));

            double resistance = 100.0 / (100.0 + Armor);
            double finalDamage = damage * resistance;
            Health = Math.Max(0, Health - finalDamage);
        }

        public void Heal(double healAmount)
        {
            if (healAmount <= 0)
                throw new ArgumentException("Heal amount must be positive.", nameof(healAmount));

            if (!IsAlive()) return;

            Health = Math.Min(GetMaxHealth(), Health + healAmount);
        }

        private double GetMaxHealth()
        {
            return 350 * Math.Pow(1.6, Level - 1);
        }

        public bool IsAlive()
        {
            if (Health <= 0)
            {
                if (!_deadNameSet)
                {
                    Name = "Dead";
                    _deadNameSet = true;
                }
                return false;
            }
            return true;
        }

        public void LevelUp()
        {
            if (Level == _maximumObtainableLevel) return;

            Armor += 4;
            HealPower = (int)(HealPower * 1.6);
            AreaHealPower = (int)(AreaHealPower * 1.4);

            Level++;
            // On level up, heal to full new max HP.
            Health = GetMaxHealth();
        }

        public string UltimateName => "Divine Light";
        public string UltimateDescription => $"Heals all allies for {HealPower * 0.5:F0} health during combat";
        public int UltimateCost => 10;

        public bool CanUseUltimate() => IsAlive() && !IsUltimateActive;

        public void UseUltimate()
        {
            ActivateMassHeal();
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
        }

        public void ApplyActiveUltimateEffect()
        {
        }

        public void ApplyOneTurnUltimateEffect(TurnManager turnManager)
        {
        }

        public ICharacter? GetSelectedTarget()
        {
            return _selectedTarget;
        }

        public string Description() => ToString();

        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nHeal Power: {HealPower}\nArea Heal: {AreaHealPower}\nLevel: {Level}";
    }
}
