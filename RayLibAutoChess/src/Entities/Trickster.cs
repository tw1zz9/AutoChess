using System;

namespace RayLibAutoChess.Entities
{
    public class Trickster : ICharacter, IDamager, IEvading, IUltimate, IInformational, IUltimateActivatable, IUltimateResettable, ITargetSelectable
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
        public double DodgeChance { get; private set; } = 0.25;
        private bool _ultimateQueued;
        private bool _deadNameSet;

        public Trickster(Team team)
        {
            Team = team;
            Name = "Trickster";
            Health = 300;
            Armor = 3;
            Damage = 60;
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

        public bool Dodge()
        {
            return new Random().NextDouble() < DodgeChance;
        }

        public void ActivateStealth()
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

            // Ультимейт дает 100% шанс уклонения
            if (IsUltimateActive)
            {
                Console.WriteLine($"{Name} perfectly dodged the attack with ultimate!");
                return;
            }

            // Проверяем обычное уклонение
            if (Dodge())
            {
                Console.WriteLine($"{Name} dodged the attack!");
                return;
            }

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
            return 300 * Math.Pow(1.6, Level - 1);
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

            Armor += 2;
            Damage = (int)(Damage * 1.7);
            DodgeChance += 0.05;

            Level++;
            // При повышении уровня, восстанавливаем здоровье до максимального.
            Health = GetMaxHealth();
        }

        public string UltimateName => "Shadow Step";
        public string UltimateDescription => $"Grants 100% dodge chance for one turn";
        public int UltimateCost => 10;

        public bool CanUseUltimate() => IsAlive() && !_ultimateQueued && !IsUltimateActive;

        public void UseUltimate()
        {
            if (!IsAlive())
                throw new InvalidOperationException("Cannot activate ultimate while dead.");

            // Помещаем ультимейт в очередь для применения в следующем раунде боя
            _ultimateQueued = true;
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
            _ultimateQueued = false;
        }

        public void ApplyActiveUltimateEffect()
        {
            // Не используется для одноходовых эффектов
        }

        public void ApplyOneTurnUltimateEffect(TurnManager turnManager)
        {
            if (turnManager == null)
                throw new ArgumentNullException(nameof(turnManager), "TurnManager cannot be null.");

            if (_ultimateQueued)
            {
                _ultimateQueued = false;
                IsUltimateActive = true;
                turnManager.RegisterOneTurnEffect(ApplyPerfectDodge);
            }
        }

        private void ApplyPerfectDodge(AttackContext context)
        {
            if (context.Target == this)
            {
                // 100% шанс уклонения на этот ход
                context.Damage = 0;
                Console.WriteLine($"{Name} perfectly dodged the attack!");
            }
        }

        public void ResetDodgeChance()
        {
            // Сбрасываем временные эффекты уклонения, если они есть
        }

        public ICharacter? GetSelectedTarget()
        {
            return _selectedTarget;
        }

        public string Description() => ToString();

        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nDamage: {Damage}\nDodge: {DodgeChance:P1}\nLevel: {Level}";
    }
}
