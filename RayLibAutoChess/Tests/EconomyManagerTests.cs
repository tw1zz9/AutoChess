using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;

namespace AutoChess.Tests
{
    public class EconomyManagerTests
    {
        [Fact]
        public void GetUpgradeCost_Level1_Returns10()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            var cost = EconomyManager.GetUpgradeCost(mage);

            // Утверждение
            Assert.Equal(10, cost);
        }

        [Fact]
        public void GetUpgradeCost_Level2_Returns15()
        {
            // Подготовка
            var tank = new Tank(Team.Red);
            tank.LevelUp(); // Уровень 2

            // Действие
            var cost = EconomyManager.GetUpgradeCost(tank);

            // Утверждение
            Assert.Equal(15, cost);
        }

        [Fact]
        public void GetUpgradeCost_Level3_Returns20()
        {
            // Подготовка
            var healer = new Healer(Team.Blue);
            healer.LevelUp(); // Уровень 2
            healer.LevelUp(); // Уровень 3

            // Действие
            var cost = EconomyManager.GetUpgradeCost(healer);

            // Утверждение
            Assert.Equal(20, cost);
        }

        [Fact]
        public void GetUpgradeCost_MaxLevel_Returns0()
        {
            // Подготовка
            var trickster = new Trickster(Team.Red);
            for (int i = 1; i < 4; i++) // Максимальный уровень - 4
            {
                trickster.LevelUp();
            }

            // Действие
            var cost = EconomyManager.GetUpgradeCost(trickster);

            // Утверждение
            Assert.Equal(0, cost);
        }

        [Fact]
        public void GetUltimateCost_Mage_Returns10()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            var cost = EconomyManager.GetUltimateCost(mage);

            // Утверждение
            Assert.Equal(10, cost);
        }

        [Fact]
        public void GetUltimateCost_Tank_Returns10()
        {
            // Подготовка
            var tank = new Tank(Team.Red);

            // Действие
            var cost = EconomyManager.GetUltimateCost(tank);

            // Утверждение
            Assert.Equal(10, cost);
        }

        [Fact]
        public void CalculateRoundReward_Round1_Returns1()
        {
            // Действие
            var reward = EconomyManager.CalculateRoundReward(1);

            // Утверждение
            Assert.Equal(1, reward);
        }

        [Fact]
        public void CalculateRoundReward_Round5_Returns3()
        {
            // Действие
            var reward = EconomyManager.CalculateRoundReward(5);

            // Утверждение
            Assert.Equal(3, reward);
        }

        [Fact]
        public void CalculateRoundReward_Round10_Returns6()
        {
            // Действие
            var reward = EconomyManager.CalculateRoundReward(10);

            // Утверждение
            Assert.Equal(6, reward);
        }
    }
}
