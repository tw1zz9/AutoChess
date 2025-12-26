using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GameAssets.Game;
using GameAssets.Player;
using GameAssets.Entities;
using GameAssets.Factories;

namespace GameAssets.SaveSystem
{
    /// <summary>
    /// Менеджер сохранения и загрузки игры
    /// </summary>
    public static class SaveManager
    {
        private const string SAVE_FILE_NAME = "autosave.dat";
        private const string SAVE_DIRECTORY = "Saves";

        /// <summary>
        /// Сохраняет текущее состояние игры
        /// </summary>
        public static void SaveGame(GameManager gameManager)
        {
            try
            {
                // Создаем директорию если не существует
                string savePath = GetSavePath();
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Создаем состояние игры
                GameState gameState = GameState.CreateFromGameManager(gameManager);

                // Сериализуем и сохраняем
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    formatter.Serialize(stream, gameState);
                }

                Debug.Log($"Игра сохранена в: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при сохранении: {e.Message}");
            }
        }

        /// <summary>
        /// Загружает сохраненное состояние игры
        /// </summary>
        public static bool LoadGame(GameManager gameManager)
        {
            try
            {
                string savePath = GetSavePath();
                if (!File.Exists(savePath))
                {
                    Debug.Log("Файл сохранения не найден");
                    return false;
                }

                // Десериализуем состояние игры
                BinaryFormatter formatter = new BinaryFormatter();
                GameState gameState;

                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    gameState = formatter.Deserialize(stream) as GameState;
                }

                if (gameState == null)
                {
                    Debug.LogError("Ошибка десериализации");
                    return false;
                }

                // Восстанавливаем состояние игры
                RestoreGameState(gameManager, gameState);

                Debug.Log("Игра загружена успешно");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при загрузке: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Проверяет, существует ли файл сохранения
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(GetSavePath());
        }

        /// <summary>
        /// Удаляет файл сохранения
        /// </summary>
        public static void DeleteSave()
        {
            try
            {
                string savePath = GetSavePath();
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log("Файл сохранения удален");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при удалении сохранения: {e.Message}");
            }
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY, SAVE_FILE_NAME);
        }

        private static void RestoreGameState(GameManager gameManager, GameState gameState)
        {
            // Восстанавливаем фазу и раунд
            gameManager.SetGamePhase(gameState.CurrentPhase);
            gameManager.SetRoundNumber(gameState.CurrentRound);

            // Восстанавливаем инвентари игроков
            RestorePlayerInventory(gameManager.Player1Inventory, gameState.BluePlayer);
            RestorePlayerInventory(gameManager.Player2Inventory, gameState.RedPlayer);

            // Очищаем поле боя
            gameManager.GameBoard.ClearAllUnits();

            // Восстанавливаем юнитов на поле
            foreach (var unitState in gameState.UnitsOnBoard)
            {
                RestoreUnitOnBoard(gameManager, unitState);
            }
        }

        private static void RestorePlayerInventory(PlayersInventory inventory, PlayerState playerState)
        {
            // Восстанавливаем золото
            inventory.SetGold(playerState.Gold);

            // Очищаем текущие юниты
            inventory.ClearInventory();

            // Восстанавливаем юнитов из сохранения
            foreach (var unitState in playerState.UnitsInInventory)
            {
                var unit = CreateUnitFromState(unitState);
                if (unit != null)
                {
                    inventory.AddUnit(unit);
                }
            }
        }

        private static void RestoreUnitOnBoard(GameManager gameManager, UnitState unitState)
        {
            var unit = CreateUnitFromState(unitState);
            if (unit != null)
            {
                gameManager.PlaceUnitOnBoard(unit, unitState.BoardX, unitState.BoardY);
            }
        }

        private static Interfaces.ICharacter CreateUnitFromState(UnitState unitState)
        {
            Interfaces.ICharacter unit = null;

            // Создаем юнит нужного типа
            switch (unitState.UnitType)
            {
                case "Tank":
                    unit = UnitFactory.CreateTank(unitState.Team);
                    break;
                case "Mage":
                    unit = UnitFactory.CreateMage(unitState.Team);
                    break;
                case "Healer":
                    unit = UnitFactory.CreateHealer(unitState.Team);
                    break;
                case "Trickster":
                    unit = UnitFactory.CreateTrickster(unitState.Team);
                    break;
            }

            if (unit != null)
            {
                // Восстанавливаем здоровье (лечим до нужного уровня)
                double healthDifference = unitState.Health - unit.Health;
                if (healthDifference > 0)
                {
                    unit.TakeDamage(-healthDifference); // Отрицательный урон = лечение
                }

                // Апгрейдим до нужного уровня
                for (int i = 1; i < unitState.Level; i++)
                {
                    unit.LevelUp();
                }

                // Восстанавливаем состояние ультимейта
                if (unitState.IsUltimateActive)
                {
                    if (unit is Entities.Tank tank) tank.UseUltimate();
                    else if (unit is Entities.Mage mage) mage.UseUltimate();
                    else if (unit is Entities.Healer healer) healer.UseUltimate();
                    else if (unit is Entities.Trickster trickster) trickster.UseUltimate();
                }
            }

            return unit;
        }
    }
}
