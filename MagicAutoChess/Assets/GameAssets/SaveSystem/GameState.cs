using System;
using System.Collections.Generic;
using GameAssets;
using GameAssets.Game;

namespace GameAssets.SaveSystem
{
    /// <summary>
    /// Состояние игры для сериализации
    /// </summary>
    [Serializable]
    public class GameState
    {
        public GamePhase CurrentPhase;
        public int CurrentRound;
        public PlayerState BluePlayer;
        public PlayerState RedPlayer;
        public List<UnitState> UnitsOnBoard;

        public GameState()
        {
            BluePlayer = new PlayerState();
            RedPlayer = new PlayerState();
            UnitsOnBoard = new List<UnitState>();
        }

        /// <summary>
        /// Создает состояние игры из текущего GameManager
        /// </summary>
        public static GameState CreateFromGameManager(GameManager gameManager)
        {
            var state = new GameState
            {
                CurrentPhase = gameManager.CurrentPhase,
                CurrentRound = gameManager.RoundNumber,
                BluePlayer = PlayerState.CreateFromInventory(gameManager.Player1Inventory, Team.Blue),
                RedPlayer = PlayerState.CreateFromInventory(gameManager.Player2Inventory, Team.Red)
            };

            // Сохраняем юнитов на поле боя
            foreach (var cell in gameManager.GameBoard.GetAllOccupiedCells())
            {
                if (cell.ExistingCharacter != null)
                {
                    state.UnitsOnBoard.Add(UnitState.CreateFromCharacter(cell.ExistingCharacter, cell.X, cell.Y));
                }
            }

            return state;
        }
    }
}
