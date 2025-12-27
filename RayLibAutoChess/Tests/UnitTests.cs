using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;
using RayLibAutoChess.Entities;

namespace AutoChess.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Mage_Constructor_SetsCorrectInitialValues()
        {
            // Подготовка и действие
            var mage = new Mage(Team.Blue);

            // Утверждение
            Assert.Equal(Team.Blue, mage.Team);
            Assert.Equal("Mage", mage.Name);
            Assert.Equal(400, mage.Health);
            Assert.Equal(5, mage.Armor);
            Assert.Equal(1, mage.Level);
            Assert.Equal(80, mage.Damage);
            Assert.True(mage.IsAlive());
        }

        [Fact]
        public void Mage_LevelUp_IncreasesStats()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            mage.LevelUp();

            // Утверждение
            Assert.Equal(2, mage.Level);
            Assert.Equal(640, mage.Health); // 400 * 1.6
            Assert.Equal(8, mage.Armor); // 5 + 3 (броня)
            Assert.Equal(144, mage.Damage); // 80 * 1.8
        }

        [Fact]
        public void Mage_TakeDamage_WithArmor_ReduceHealthCorrectly()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            mage.TakeDamage(100);

            // Утверждение
            Assert.Equal(304.76190476190476, mage.Health); // 100 * (100/(100+5)) ≈ 95.24 урона
            Assert.True(mage.IsAlive());
        }

        [Fact]
        public void Mage_TakeDamage_Overkill_SetsHealthToZero()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            mage.TakeDamage(1000);

            // Утверждение
            Assert.Equal(0, mage.Health);
            Assert.False(mage.IsAlive());
            Assert.Equal("Dead", mage.Name);
        }

        [Fact]
        public void Mage_CanUseUltimate_Initially_ReturnsTrue()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            var canUse = mage.CanUseUltimate();

            // Утверждение
            Assert.True(canUse);
        }

        [Fact]
        public void Tank_Constructor_SetsCorrectInitialValues()
        {
            // Подготовка и действие
            var tank = new Tank(Team.Red);

            // Утверждение
            Assert.Equal(Team.Red, tank.Team);
            Assert.Equal("Tank", tank.Name);
            Assert.Equal(600, tank.Health);
            Assert.Equal(15, tank.Armor);
            Assert.Equal(1, tank.Level);
            Assert.True(tank.IsAlive());
        }

        [Fact]
        public void Tank_LevelUp_IncreasesStats()
        {
            // Подготовка
            var tank = new Tank(Team.Red);

            // Действие
            tank.LevelUp();

            // Утверждение
            Assert.Equal(2, tank.Level);
            Assert.Equal(960, tank.Health); // 600 * 1.6
            Assert.Equal(18, tank.Armor); // 15 + 3 (броня)
        }

        [Fact]
        public void Healer_Constructor_SetsCorrectInitialValues()
        {
            // Подготовка и действие
            var healer = new Healer(Team.Blue);

            // Утверждение
            Assert.Equal(Team.Blue, healer.Team);
            Assert.Equal("Healer", healer.Name);
            Assert.Equal(350, healer.Health);
            Assert.Equal(8, healer.Armor);
            Assert.Equal(1, healer.Level);
            Assert.Equal(70, healer.HealPower);
            Assert.True(healer.IsAlive());
        }

        [Fact]
        public void Healer_Heal_IncreasesTargetHealth()
        {
            // Подготовка
            var healer = new Healer(Team.Blue);
            var target = new Mage(Team.Blue);
            target.TakeDamage(100); // Здоровье = 400 - ~95.24 = ~304.76

            // Действие
            healer.Heal();

            // Утверждение
            Assert.Equal(374.76190476190476, target.Health); // ~304.76 + 70 (лечение)
        }

        [Fact]
        public void Trickster_Constructor_SetsCorrectInitialValues()
        {
            // Подготовка и действие
            var trickster = new Trickster(Team.Red);

            // Утверждение
            Assert.Equal(Team.Red, trickster.Team);
            Assert.Equal("Trickster", trickster.Name);
            Assert.Equal(300, trickster.Health);
            Assert.Equal(3, trickster.Armor);
            Assert.Equal(1, trickster.Level);
            Assert.True(trickster.IsAlive());
        }

        [Fact]
        public void Trickster_Dodge_Initially_ReturnsFalse()
        {
            // Подготовка
            var trickster = new Trickster(Team.Red);

            // Действие
            var dodged = trickster.Dodge();

            // Утверждение
            Assert.False(dodged);
        }

        [Fact]
        public void AllUnits_LevelUp_MaxLevel_PreventsFurtherLeveling()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);
            var tank = new Tank(Team.Red);
            var healer = new Healer(Team.Blue);
            var trickster = new Trickster(Team.Red);

            // Действие - повышение уровня до максимума (4 для большинства юнитов)
            for (int i = 1; i <= 4; i++)
            {
                mage.LevelUp();
                tank.LevelUp();
                healer.LevelUp();
                trickster.LevelUp();
            }

            // Утверждение - Должен быть на максимальном уровне
            Assert.Equal(4, mage.Level);
            Assert.Equal(4, tank.Level);
            Assert.Equal(4, healer.Level);
            Assert.Equal(4, trickster.Level);
        }


        [Fact]
        public void Unit_Heal_OverMaxHealth_ClampsToMax()
        {
            // Подготовка
            var mage = new Mage(Team.Blue);

            // Действие
            mage.Heal(100); // Попытка лечения сверх максимума

            // Утверждение
            Assert.Equal(400, mage.Health); // Не должно превышать максимум
        }
    }
}
