using GameAssets.Interfaces;
using NUnit.Framework.Constraints;
using System;
using System.Xml.Linq;

namespace GameAssets.Entities
{
    public class Trickster : ICharacter, IDamager, IEvading, IInformational
    {
        private readonly int _maximumObtainableLevel = 4;

        private double _dodgeChance;

        #region Описание стандартных свойств класса Trickster

        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }
        public double Damage { get; private set; }

        #endregion 

        #region Описание свойств для уклонения Trickster
        /// <summary>
        /// Реализация IEvading, с установленными границами значений конкретно для этого класса.
        /// </summary>
        public double DodgeChance
        {
            get => _dodgeChance;
            private set
            {
                if (value < 0 || value > 0.4) return;
                _dodgeChance = value;
            }
        }
        #endregion

        public Trickster()
        {
            Name = "Sneaky Trickster";
            ID = Guid.NewGuid();
            Health = 650;
            Damage = 200;
            Level = 1;
            Armor = 5;
        }

        public void TakeDamage(double damage)
        {
            if (!Dodge())
            {
                var finalHealth = Health - damage / Armor;
                Health = finalHealth > 0 ? finalHealth : 0;
            }
        }

        public bool Dodge()
        {
            var random = new Random();
            var probability = random.NextDouble();
            if (probability < DodgeChance) return true;
            return false;
        }


        public void Fight(ICharacter target)
        {
            if (target != null && target.IsAlive())
                target.TakeDamage(Damage);
        }

        public bool IsAlive()
        {
            if (Health > 0) return true;
            else
            {
                Name = "(Dead)" + Name;
                return false;
            }
        }

        public void LevelUp()
        {
            if (Level == _maximumObtainableLevel) return;

            var multiplicator = 1.5;
            var enhancedMultiplicator = 2;

            Damage *= multiplicator;
            Armor *= multiplicator;
            Health *= multiplicator;

            var newChance = DodgeChance * enhancedMultiplicator;

            DodgeChance = newChance;

            Level++;
        }

        public string Description() => ToString();

        public override string ToString() => $"Troop: {Name}\nCurrent health: {Health}\n" +
            $"Damage: {Damage}\nCurrent level: {Level}";
    }
}