
namespace RayLibAutoChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск игры Auto Chess...");

            try
            {
                var gameManager = new GameManager();
                var renderer = new Render(gameManager);

                Console.WriteLine("Игра инициализирована. Запуск рендерера...");
                renderer.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
            }
        }
    }
}
