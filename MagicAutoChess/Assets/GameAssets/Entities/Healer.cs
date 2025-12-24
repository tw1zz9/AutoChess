using GameAssets.Interfaces;
using System;

namespace GameAssets.Entities 
{
    public class Healer : IHealer, IInformational
    {
        /// <summary>
        /// Максимальный получаемый уровень этого конкретного класса.
        /// </summary>
        private readonly int _maximumObtainableLevel = 3;

        public Guid ID { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Реализация свойства интерфейса IHealer
        /// </summary>
        public double HealPower { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }
        public int Level { get; private set; }

        public Healer()
        {
            Name = "Angel";
            HealPower = 100;
            Health = 800;
            Armor = 5;
            Level = 1;
        }

        public void TakeDamage(double damage)
        {
            if (this.IsAlive()) Health -= damage / Armor;
        }

        /// <summary>
        /// Реализация метода интерфейса IHealer
        /// Этим методом его нельзя воскресить, только увеличить ненулевое здоровье.
        /// </summary>
        /// <param name="unit">Юнит, которому наносится урон</param>
        public void Heal(ICharacter unit)
        {
            if (unit.IsAlive() || unit != null) unit.TakeDamage(-HealPower);
        }

        public bool IsAlive()
        {
            if (Health > 0) return true;
            else
            {
                Name = "(Dead) " + Name;
                return false;
            }
        }

        public void LevelUp()
        {
            if (Level == _maximumObtainableLevel) return;

            var _multiplicator = 1.8;
            var _enhancedMultiplicator = 2;

            HealPower *= _multiplicator;
            Health *= _enhancedMultiplicator;
            Armor *= _enhancedMultiplicator;

            Level++;
        }

        public string Description() => ToString();

        public override string ToString() => $"Troop: {Name}\nCurrent health: {Health}\n" +
                $"Healing: {HealPower}\nCurrent level: {Level}\n";

    }
}
