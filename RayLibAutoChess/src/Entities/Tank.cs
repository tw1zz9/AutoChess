using System;

namespace RayLibAutoChess.Entities
{
    public class Tank : ICharacter, IDamager, IInformational, IUltimateActivatable, IUltimateResettable, ITargetSelectable
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

        private ICharacter? _selectedTarget;
        public bool IsUltimateActive { get; private set; }
        private bool _ultimateQueued;
        private bool _deadNameSet;

        public double ShieldAmount { get; private set; } = 100;

        public Tank(Team team)
        {
            Team = team;
            Name = "Tank";
            Health = 600;
            Armor = 15;
            Damage = 50;
            Level = 1;
            ID = Guid.NewGuid();
        }

        public void SelectTarget(ICharacter target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target), "Target cannot be null.");

            if (target.Team == Team)
                throw new ArgumentException("Cannot target allied units.", nameof(target));

            if (!target.IsAlive())
                throw new ArgumentException("Cannot target dead units.", nameof(target));

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

        public void ActivateTaunt(TurnManager turnManager)
        {
            if (turnManager == null)
                throw new ArgumentNullException(nameof(turnManager), "TurnManager cannot be null.");

            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            if (IsUltimateActive)
                throw new InvalidOperationException("Ultimate is already active.");

            IsUltimateActive = true;
            turnManager.SetTauntingTank(this);
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
            return 600 * Math.Pow(1.6, Level - 1);
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

            Armor += 5;
            Damage = (int)(Damage * 1.5);
            ShieldAmount = (int)(ShieldAmount * 1.3);

            Level++;
            Health = GetMaxHealth();
        }

        public string UltimateName => "Taunt";
        public string UltimateDescription => $"Forces all enemies to attack this tank for one turn";
        public int UltimateCost => 10;

        public bool CanUseUltimate() => IsAlive() && !_ultimateQueued && !IsUltimateActive;

        public void UseUltimate()
        {
            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            _ultimateQueued = true;
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
            _ultimateQueued = false;
        }

        public void ApplyActiveUltimateEffect()
        {
        }

        public void ApplyOneTurnUltimateEffect(TurnManager turnManager)
        {
            if (turnManager == null)
                throw new ArgumentNullException(nameof(turnManager), "TurnManager cannot be null.");

            if (_ultimateQueued)
            {
                _ultimateQueued = false;
                ActivateTaunt(turnManager);
            }
        }

        public ICharacter? GetSelectedTarget()
        {
            return _selectedTarget;
        }

        public string Description() => ToString();

        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nDamage: {Damage}\nShield: {ShieldAmount}\nLevel: {Level}";
    }
}
