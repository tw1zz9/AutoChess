using GameAssets.Interfaces;
using System;

namespace GameAssets.Entities 
{
    public class Tank : ICharacter, IDamager, IUtilitable, IInformational
    {
        /// <summary>
        /// Максимальный получаемый уровень этого конкретного класса.
        /// </summary>
        private readonly int _maximumObtainableLevel = 5;

        public event Action<ICharacter, double> OnDamageRecieved = null;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public double Damage { get; private set; }
        public double Health { get; private set; }
        public double Armor { get; private set; }

        /// <summary>
        /// Флаг для определения состояния персонажа (принимает ли Tank часть урона союзников или нет).
        /// </summary>
        public bool Taunt { get; private set; }

        public int Level { get; set; }

        public Tank()
        {
            Name = "Great Paladin";
            Damage = 100;
            Health = 1000;
            Armor = 15;
            Level = 1;
            Taunt = false;
            ID = new Guid();
        }

        public void TakeDamage(double damage)
        {
            var finalHealth = Health - damage / Armor;
            Health = finalHealth > 0 ? finalHealth : 0;
        }

        public void Fight(ICharacter target)
        {
            if (target.IsAlive() || target != null) 
                target.TakeDamage(Damage);
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

            Damage *= _multiplicator;
            Health *= _enhancedMultiplicator;
            Armor *= _enhancedMultiplicator;

            Level++;
        }
        
        public string Description() => ToString();
        
        public override string ToString() => $"Troop: {Name}\nCurrent health: {Health}\n" +
            $"Damage: {Damage}\nCurrent level: {Level}\nCurrent state (Defence stance): {Taunt}";

        public void SpecialUtility(bool OnOrOff)
        {
            if (OnOrOff == true && Taunt == true) return;
            if (OnOrOff == false && Taunt == false) return;

            switch (OnOrOff) 
            {
                case (true): { Taunt = true; break; }
                case (false): { Taunt = false; break; }
            }
        }
    }
}
