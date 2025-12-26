using System.Text.Json.Serialization;

namespace RayLibAutoChess.SaveSystem
{
    public class GameState
    {
        public int RoundNumber { get; set; }
        public GamePhase CurrentPhase { get; set; }
        public PlayerTurn CurrentTurn { get; set; }

        public PlayerState Player1State { get; set; }
        public PlayerState Player2State { get; set; }

        [JsonConstructor]
        public GameState()
        {
            Player1State = new PlayerState();
            Player2State = new PlayerState();
        }

        public GameState(GameManager gameManager)
        {
            RoundNumber = gameManager.RoundNumber;
            CurrentPhase = gameManager.CurrentPhase;
            CurrentTurn = gameManager.CurrentTurn;

            Player1State = new PlayerState(gameManager.Player1Inventory, Team.Blue);
            Player2State = new PlayerState(gameManager.Player2Inventory, Team.Red);
        }
    }
}
