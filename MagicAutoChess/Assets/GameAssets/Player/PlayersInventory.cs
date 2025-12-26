using System;
using System.Collections.Generic;
using System.Linq;
using GameAssets.Economy;
using GameAssets.Interfaces;

namespace GameAssets.Player
{
    public class PlayersInventory : IPlayersInventory
    {
        private readonly List<ICharacter> _units = new();
        private int _gold;

        public int Gold 
        { 
            get => _gold;
            private set
            {
                if (value < 0) return;
                _gold = value;
            }
        }

        public PlayersInventory(int startingGold = 100)
        {
            _gold = startingGold;
        }

        public void AddUnits(IEnumerable<ICharacter> units)
        {
            if (units == null) return;
            _units.AddRange(units);
        }

        /// <summary>
        /// Удаляет юнит из инвентаря
        /// </summary>
        public void RemoveUnit(ICharacter unit)
        {
            if (unit != null)
            {
                _units.Remove(unit);
            }
        }

        /// <summary>
        /// Удаляет несколько юнитов из инвентаря
        /// </summary>
        public void RemoveUnits(IEnumerable<ICharacter> units)
        {
            if (units == null) return;
            foreach (var unit in units)
            {
                _units.Remove(unit);
            }
        }

        public IReadOnlyList<ICharacter> GetAllUnits() => _units.AsReadOnly();

        public void AddAmount(int amount) => Gold += amount;
        public void RemoveAmount(int amount)
        {
            if (amount < 0) return;
            else if (_gold <= amount)
            {
                Gold = 0;
                return;
            }
            else Gold -= amount;
        }

        /// <summary>
        /// Пытается апгрейднуть юнит
        /// </summary>
        public bool TryUpgradeUnit(ICharacter unit)
        {
            return EconomyManager.UpgradeUnit(unit, ref _gold);
        }

        /// <summary>
        /// Проверяет, может ли апгрейднуть юнит
        /// </summary>
        public bool CanUpgradeUnit(ICharacter unit)
        {
            return EconomyManager.CanUpgradeUnit(unit, Gold);
        }

        /// <summary>
        /// Использует ультимейт
        /// </summary>
        public bool TryUseUltimate(IUltimate ultimateUser)
        {
            return EconomyManager.UseUltimate(ultimateUser, ref _gold);
        }

        /// <summary>
        /// Получает юнит по ID
        /// </summary>
        public ICharacter GetUnitById(Guid id)
        {
            return _units.FirstOrDefault(u => u.ID == id);
        }

        /// <summary>
        /// Устанавливает количество золота (для загрузки сохранения)
        /// </summary>
        public void SetGold(int amount)
        {
            _gold = amount;
        }

        /// <summary>
        /// Очищает инвентарь (для загрузки сохранения)
        /// </summary>
        public void ClearInventory()
        {
            _units.Clear();
        }

        /// <summary>
        /// Добавляет юнит в инвентарь (для загрузки сохранения)
        /// </summary>
        public void AddUnit(ICharacter unit)
        {
            if (unit != null && !_units.Contains(unit))
            {
                _units.Add(unit);
            }
        }
    }
}