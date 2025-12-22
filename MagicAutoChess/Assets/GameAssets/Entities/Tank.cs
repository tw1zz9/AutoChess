using GameAssets.Interfaces;

namespace GameAssets.Entities 
{
    public class Tank : IDamager, IUtilitable, IInformational
    {
        private readonly int _maximumObtainableLevel = 30;

        private bool _isTaunting;

        public string Name { get; private set; }

        public double Damage { get; set; }
        public double Health { get; set; }
        public double Armor { get; set; }

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

            var _multiplicator = 1.8;
            var _enhancedMultiplicator = 2;

            Damage *= _multiplicator;
            Health *= _enhancedMultiplicator;
            Armor *= _enhancedMultiplicator;

            Level++;
        }
        
        public string Description() => ToString();
        
        public override string ToString() => $"Troop: {Name}\nCurrent health: {Health}\n" +
            $"";

        public void SpecialUtility(int _numberOfTeammates)
        {
            _isTaunting = true;
        }
    }
}
