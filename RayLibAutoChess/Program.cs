using RayLibAutoChess.UI;

namespace RayLibAutoChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Auto Chess Game...");

            try
            {
                var gameManager = new GameManager();
                var renderer = new GameRenderer(gameManager);

                Console.WriteLine("Game initialized. Starting renderer...");
                renderer.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
