using System;
using GameAssets;
using GameAssets.Combat;
using GameAssets.Interfaces;

namespace GameAssets.Entities
{
    public class Healer : ICharacter, IHealer, IInformational
    {
        private readonly int _maximumObtainableLevel = 3;

        public Guid ID { get; private set; }
        public Team Team { get; }
        public string Name { get; private set; }

        public double HealPower { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }

        private ICharacter _selectedAlly;

        public Healer(Team team)
        {
            Team = team;
            Name = "Angel";
            HealPower = 100;
            Health = 800;
            Armor = 8;
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

        public void Heal()
        {
            if (_selectedAlly == null || !_selectedAlly.IsAlive()) return;
            _selectedAlly.TakeDamage(-HealPower);
        }

        public void TakeDamage(double damage)
        {
            if (!IsAlive()) return;
            Health -= damage / Armor;
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

            HealPower *= 1.8;
            Health *= 2;
            Armor *= 2;
            Level++;
        }

        public string Description() => ToString();
        public override string ToString() =>
            $"Troop: {Name}\nHealth: {Health}\nHealing Power: {HealPower}\nLevel: {Level}";
    }
}
