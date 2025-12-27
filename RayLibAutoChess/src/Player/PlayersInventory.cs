using System.Collections.Generic;
using System.Linq;

namespace RayLibAutoChess
{
    public class PlayersInventory : IPlayersInventory
    {
        private readonly List<ICharacter> _units = new();
        private int _gold;

        public int Gold => _gold;
        public IEnumerable<ICharacter> GetAllUnits() => _units.AsReadOnly();

        public PlayersInventory(int startingGold)
        {
            _gold = startingGold;
        }

        public void AddAmount(int amount)
        {
            _gold = Math.Max(0, _gold + amount);
        }

        public void RemoveAmount(int amount)
        {
            _gold = Math.Max(0, _gold - amount);
        }

        public void AddUnit(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));
            _units.Add(unit);
        }

        public void AddUnits(IEnumerable<ICharacter> units)
        {
            if (units == null)
                throw new ArgumentNullException(nameof(units));
            _units.AddRange(units);
        }

        public void RemoveUnit(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));
            _units.Remove(unit);
        }

        public void RemoveUnits(IEnumerable<ICharacter> units)
        {
            if (units == null)
                throw new ArgumentNullException(nameof(units));
            foreach (var unit in units)
            {
                _units.Remove(unit);
            }
        }

        public bool TryUpgradeUnit(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            int cost = EconomyManager.GetUpgradeCost(unit);
            if (_gold < cost)
                return false;

            int beforeLevel = unit.Level;
            unit.LevelUp();
            if (unit.Level == beforeLevel)
                return false;

            RemoveAmount(cost);
            return true;
        }

        public bool TryUseUltimate(IUltimate ultimateUser)
        {
            if (ultimateUser == null)
                throw new ArgumentNullException(nameof(ultimateUser));

            int cost = EconomyManager.GetUltimateCost(ultimateUser);
            if (_gold < cost)
                return false;

            if (!ultimateUser.CanUseUltimate())
                return false;

            ultimateUser.UseUltimate();
            RemoveAmount(cost);
            return true;
        }

        public ICharacter? GetUnitById(Guid id)
        {
            return _units.FirstOrDefault(u => u.ID == id);
        }
    }
}
