using System;

namespace RayLibAutoChess.Entities
{
    public class Mage : ICharacter, IDamager, ITargetedUltimate, IInformational, IUltimateActivatable, IUltimateResettable, ITargetSelectable
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
        private ICharacter? _ultimateTarget;
        private Guid? _ultimateTargetId;

        public double BuffMultiplier { get; private set; } = 1.5;

        public Mage(Team team)
        {
            Team = team;
            Name = "Mage";
            Health = 400;
            Armor = 5;
            Damage = 80;
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

        public void ActivateDamageBuff(TurnManager turnManager)
        {
            if (turnManager == null)
                throw new ArgumentNullException(nameof(turnManager), "TurnManager cannot be null.");

            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            if (IsUltimateActive)
                throw new InvalidOperationException("Ultimate is already active.");

            IsUltimateActive = true;
            turnManager.RegisterOneTurnEffect(ApplyDamageBuff);
        }

        private void ApplyDamageBuff(AttackContext context)
        {
            if (context.Attacker.Team != Team) return;
            if (context.Attacker.ID == ID) return;
            // Buff only the chosen ally (not enemies, and not the mage itself)
            if (_ultimateTargetId == null || context.Attacker.ID != _ultimateTargetId.Value) return;
            context.Damage *= BuffMultiplier;
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
            return 400 * Math.Pow(1.6, Level - 1);
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

            Armor += 3;
            Damage = (int)(Damage * 1.8);
            BuffMultiplier += 0.2;

            Level++;
            // On level up, heal to full new max HP.
            Health = GetMaxHealth();
        }

        public string UltimateName => "Arcane Surge";
        public string UltimateDescription => $"Doubles damage buff multiplier for one turn";
        public int UltimateCost => 10;

        public bool CanUseUltimate() => IsAlive() && !_ultimateQueued && !IsUltimateActive;

        public void UseUltimate()
        {
            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            if (_ultimateTarget == null)
                throw new InvalidOperationException("Mage ultimate requires selecting a target first.");

            // Queue ultimate to be applied on the next combat round (TurnManager will pick it up).
            _ultimateQueued = true;
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
            _ultimateQueued = false;
            _ultimateTarget = null;
            _ultimateTargetId = null;
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
                if (_ultimateTarget == null || !_ultimateTarget.IsAlive())
                {
                    _ultimateQueued = false;
                    _ultimateTarget = null;
                    _ultimateTargetId = null;
                    return;
                }

                _ultimateQueued = false;
                _ultimateTargetId = _ultimateTarget.ID;
                _ultimateTarget = null;
                ActivateDamageBuff(turnManager);
            }
        }

        public void SetUltimateTarget(ICharacter target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target), "Target cannot be null.");

            // Mage ultimate is a buff: only allies, and not self.
            if (target.Team != Team)
                throw new ArgumentException("Mage ultimate can only target allied units.", nameof(target));

            if (target.ID == ID)
                throw new ArgumentException("Mage ultimate cannot target the mage itself.", nameof(target));

            if (!target.IsAlive())
                throw new ArgumentException("Cannot target dead units.", nameof(target));

            _ultimateTarget = target;
        }

        public ICharacter? GetUltimateTarget() => _ultimateTarget;

        public ICharacter? GetSelectedTarget()
        {
            return _selectedTarget;
        }

        public string Description() => ToString();

        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nDamage: {Damage}\nBuff x{BuffMultiplier}\nLevel: {Level}";
    }
}
