using GameAssets.Interfaces;
using System;

namespace GameAssets.Entities 
{
    public class Tank : IDamager, IUtilitable, IInformational
    {
        /// <summary>
        /// Максимальный получаемый уровень персонажа.
        /// </summary>
        private readonly int _maximumObtainableLevel = 5;

        /// <summary>
        /// Флаг для определения состояния персонажа (принимает ли Tank часть урона союзников или нет).
        /// </summary>
        private bool _isTaunting;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid ID { get; private set; }
        
        public string Name { get; private set; }

        public int Damage { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }

        public bool Taunt { get => _isTaunting; private set => _isTaunting = value; }

        public int Level { get; set; }

        public Tank()
        {
            Name = "Great Paladin";
            Damage = 100;
            Health = 1000;
            Armor = 15;
            Level = 1;
            _isTaunting = false;
            ID = new Guid();
        }

        public void Fight(ICharacter target)
        {
            if (target.IsAlive())
            {
                target.Health -= Damage / Armor;
            }
            else return;
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

            var _multiplicator = 2;
            var _enhancedMultiplicator = 3;

            Damage = Damage * _multiplicator;
            Health *= _enhancedMultiplicator;
            Armor *= _enhancedMultiplicator;

            Level++;
        }
        
        public string Description() => ToString();
        
        public override string ToString() => $"Troop: {Name}\nCurrent health: {Health}\n" +
            $"Damage: {Damage}\nCurrent level: {Level}\nCurrent state: {Taunt}";

        public void SpecialUtility(int _numberOfTeammates)
        {
            _isTaunting = true;
        }
    }
}
