using System;
using System.Collections.Generic;
using GameAssets.Interfaces;

namespace GameAssets.Economy
{
    /// <summary>
    /// Управляет экономикой игры: золотом, апгрейдами, ультимейтами
    /// </summary>
    public class EconomyManager
    {
        public static readonly Dictionary<int, int> UpgradeCosts = new()
        {
            {1, 10}, // 1→2 уровень: 10 золота
            {2, 20}, // 2→3 уровень: 20 золота
            {3, 30}  // 3→4 уровень: 30 золота
        };

        public const int UltimateCost = 5;

        /// <summary>
        /// Вознаграждение за победу в раунде
        /// </summary>
        public static int CalculateRoundReward(int roundNumber)
        {
            // Базовое вознаграждение + бонус за раунд
            return 5 + (roundNumber - 1) * 2;
        }

        /// <summary>
        /// Стартовое золото для игроков
        /// </summary>
        public const int StartingGold = 0;

        /// <summary>
        /// Проверяет, может ли игрок апгрейднуть юнит
        /// </summary>
        public static bool CanUpgradeUnit(ICharacter unit, int playerGold)
        {
            if (unit.Level >= 4) return false; // Максимальный уровень
            return playerGold >= UpgradeCosts[unit.Level];
        }

        /// <summary>
        /// Проверяет, может ли игрок использовать ультимейт
        /// </summary>
        public static bool CanUseUltimate(IUltimate ultimateUser, int playerGold)
        {
            return ultimateUser.CanUseUltimate() && playerGold >= ultimateUser.UltimateCost;
        }

        /// <summary>
        /// Выполняет апгрейд юнита
        /// </summary>
        public static bool UpgradeUnit(ICharacter unit, ref int playerGold)
        {
            if (!CanUpgradeUnit(unit, playerGold)) return false;

            int cost = UpgradeCosts[unit.Level];
            playerGold -= cost;
            unit.LevelUp();
            return true;
        }

        /// <summary>
        /// Использует ультимейт
        /// </summary>
        public static bool UseUltimate(IUltimate ultimateUser, ref int playerGold)
        {
            if (!CanUseUltimate(ultimateUser, playerGold)) return false;

            playerGold -= ultimateUser.UltimateCost;
            ultimateUser.UseUltimate();
            return true;
        }
    }
}
