using System;
using System.Collections.Generic;
using System.Linq;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Healer : ICharacter, IHealer, IUltimate, IInformational
    {
        private readonly int _maximumObtainableLevel = 4;

        public Guid ID { get; private set; }
        public Team Team { get; }
        public string Name { get; private set; }

        public double HealPower { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }

        private ICharacter _selectedAlly;
        public bool IsUltimateActive { get; private set; }

        public Healer(Team team)
        {
            Team = team;
            Name = "Angel";
            HealPower = 60;
            Health = 500;
            Armor = 15; // 13% сопротивления
            Level = 1;
            ID = Guid.NewGuid();
        }

        public void SelectTarget(ICharacter ally)
        {
            if (ally == null || !ally.IsAlive()) return;
            if (ally.Team != Team) return;
            _selectedAlly = ally;
        }

        public void ResetTarget()
        {
            _selectedAlly = null;
        }

        public ICharacter GetSelectedTarget()
        {
            return _selectedAlly;
        }

        public void Heal()
        {
            if (_selectedAlly == null || !_selectedAlly.IsAlive()) return;
            _selectedAlly.TakeDamage(-HealPower);
        }

        public void HealAll(IEnumerable<ICharacter> allies)
        {
            foreach (var ally in allies.Where(a => a.Team == Team && a.IsAlive()))
            {
                ally.TakeDamage(-HealPower * 0.5); // Массовое лечение слабее
            }
        }

        public void TakeDamage(double damage)
        {
            if (!IsAlive()) return;
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
            if (Level >= _maximumObtainableLevel) return;

            HealPower = (int)(HealPower * 1.7); // +70% лечения
            Health = (int)(Health * 1.8); // +80% здоровья
            Armor += 8; // +8 брони
            Level++;
        }

        public string UltimateName => "Divine Light";
        public string UltimateDescription => $"Heals all allies for 50% of normal healing power";
        public int UltimateCost => 5;

        public bool CanUseUltimate() => IsAlive();

        public void UseUltimate()
        {
            // Ультимейт Healer активируется в фазе подготовки
            // В бою применяется массовое лечение через HealAll
            IsUltimateActive = true;
        }

        public void ResetUltimateState()
        {
            IsUltimateActive = false;
        }

        public string Description() => ToString();
        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nHealing Power: {HealPower}\nLevel: {Level}";
    }
}
