using System.Text.Json;

namespace RayLibAutoChess.SaveSystem
{
    public static class SaveManager
    {
        private const string SaveFileName = "savegame.json";
        private static readonly string SaveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaveFileName);

        public static void SaveGame(GameManager gameManager)
        {
            try
            {
                var gameState = new GameState(gameManager);
                var json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SaveFilePath, json);
                Console.WriteLine("Game saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game: {ex.Message}");
            }
        }

        public static bool LoadGame(GameManager gameManager)
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Console.WriteLine("No save file found.");
                    return false;
                }

                var json = File.ReadAllText(SaveFilePath);
                var gameState = JsonSerializer.Deserialize<GameState>(json);

                if (gameState == null)
                {
                    Console.WriteLine("Failed to deserialize save file.");
                    return false;
                }

                // Restore game state
                gameManager.SetRoundNumber(gameState.RoundNumber);
                gameManager.SetGamePhase(gameState.CurrentPhase);

                // Restore players
                RestorePlayer(gameManager.Player1Inventory, gameState.Player1State);
                RestorePlayer(gameManager.Player2Inventory, gameState.Player2State);

                Console.WriteLine("Game loaded successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
                return false;
            }
        }

        private static void RestorePlayer(PlayersInventory inventory, PlayerState playerState)
        {
            // Clear current inventory
            var currentUnits = inventory.GetAllUnits().ToList();
            inventory.RemoveUnits(currentUnits);

            // Restore gold
            inventory.AddAmount(playerState.Gold - inventory.Gold);

            // Restore units
            var units = playerState.Units.Select(u => u.ToUnit()).ToList();
            inventory.AddUnits(units);
        }

        public static bool SaveExists()
        {
            return File.Exists(SaveFilePath);
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Console.WriteLine("Save file deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete save file: {ex.Message}");
            }
        }
    }
}
