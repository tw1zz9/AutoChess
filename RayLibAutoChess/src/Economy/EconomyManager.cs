namespace RayLibAutoChess
{
    public static class EconomyManager
    {
        public const int StartingGold = 10;

        public static int CalculateRoundReward(int roundNumber)
        {
            // Base reward increases with round number
            return Math.Max(1, roundNumber / 2 + 1);
        }

        public static int GetUpgradeCost(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            // Fixed costs for level upgrades
            return unit.Level switch
            {
                1 => 10, // 1->2 level
                2 => 15, // 2->3 level
                3 => 20, // 3->4 level
                _ => 0   // Max level or invalid
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
