using Xunit;
using RayLibAutoChess;

namespace AutoChess.Tests
{
    public class GameManagerTests
    {
        // Примечание: GameManager использует паттерн Singleton, поэтому мы не можем создавать новые экземпляры в тестах
        // Мы тестируем существующий экземпляр, который был создан при запуске приложения

        [Fact]
        public void GameManagerInstance_Exists()
        {
            // GameManager.Instance создается при запуске приложения
            // Мы просто проверяем, что он существует и имеет корректное начальное состояние
            var gameManager = GameManager.Instance;

            Assert.NotNull(gameManager);
            Assert.Equal(GamePhase.Preparation, gameManager.CurrentPhase);
            Assert.Equal(1, gameManager.RoundNumber);
        }
    }
}
