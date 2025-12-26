using System;
using System.Collections.Generic;
using GameAssets;
using GameAssets.Player;

namespace GameAssets.SaveSystem
{
    /// <summary>
    /// Состояние игрока для сериализации
    /// </summary>
    [Serializable]
    public class PlayerState
    {
        public Team Team;
        public int Gold;
        public List<UnitState> UnitsInInventory;

        public PlayerState()
        {
            UnitsInInventory = new List<UnitState>();
        }

        /// <summary>
        /// Создает состояние игрока из инвентаря
        /// </summary>
        public static PlayerState CreateFromInventory(PlayersInventory inventory, Team team)
        {
            var state = new PlayerState
            {
                Team = team,
                Gold = inventory.Gold
            };

            // Сохраняем все юниты в инвентаре
            foreach (var unit in inventory.GetAllUnits())
            {
                state.UnitsInInventory.Add(UnitState.CreateFromCharacter(unit, -1, -1));
            }

            return state;
        }
    }
}
