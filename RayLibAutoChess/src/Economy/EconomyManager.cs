namespace RayLibAutoChess
{
    public static class EconomyManager
    {
        public const int StartingGold = 10;

        public static int CalculateRoundReward(int roundNumber)
        {
            // Базовая награда увеличивается с номером раунда
            return Math.Max(1, roundNumber / 2 + 1);
        }

        public static int GetUpgradeCost(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            // Фиксированные стоимости для улучшения уровней
            return unit.Level switch
            {
                1 => 10, // 1->2 уровень
                2 => 15, // 2->3 уровень
                3 => 20, // 3->4 уровень
                _ => 0   // Максимальный уровень или недействительно
            };
        }

        public static int GetUltimateCost(IUltimate ultimate)
        {
            if (ultimate == null)
                throw new ArgumentNullException(nameof(ultimate));

            return ultimate.UltimateCost;
        }
    }
}
