using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;
using System.Linq;

namespace AutoChess.Tests
{
    public class PlayersInventoryTests
    {
        [Fact]
        public void Constructor_StartingGold_SetsCorrectGold()
        {
            // Подготовка и действие
            var inventory = new PlayersInventory(15);

            // Утверждение
            Assert.Equal(15, inventory.Gold);
        }

        [Fact]
        public void AddUnits_IncreasesUnitCount()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var units = new ICharacter[] { new Mage(Team.Blue), new Tank(Team.Blue) };

            // Действие
            inventory.AddUnits(units);

            // Утверждение
            Assert.Equal(2, inventory.GetAllUnits().Count());
        }

        [Fact]
        public void AddUnits_EmptyCollection_DoesNotChangeCount()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var units = new ICharacter[0];

            // Действие
            inventory.AddUnits(units);

            // Утверждение
            Assert.Equal(0, inventory.GetAllUnits().Count());
        }

        [Fact]
        public void RemoveUnits_DecreasesUnitCount()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var mage = new Mage(Team.Blue);
            var tank = new Tank(Team.Blue);
            inventory.AddUnits(new ICharacter[] { mage, tank });

            // Действие
            inventory.RemoveUnits(new ICharacter[] { mage });

            // Утверждение
            Assert.Equal(1, inventory.GetAllUnits().Count());
            Assert.Contains(tank, inventory.GetAllUnits());
            Assert.DoesNotContain(mage, inventory.GetAllUnits());
        }

        [Fact]
        public void RemoveUnits_EmptyCollection_DoesNotChangeCount()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var units = new ICharacter[] { new Mage(Team.Blue), new Tank(Team.Blue) };
            inventory.AddUnits(units);

            // Действие
            inventory.RemoveUnits(System.Array.Empty<ICharacter>());

            // Утверждение
            Assert.Equal(2, inventory.GetAllUnits().Count());
        }

        [Fact]
        public void GetAllUnits_ReturnsAllAddedUnits()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var mage = new Mage(Team.Blue);
            var tank = new Tank(Team.Red);
            var healer = new Healer(Team.Blue);
            inventory.AddUnits(new ICharacter[] { mage, tank, healer });

            // Действие
            var allUnits = inventory.GetAllUnits().ToList();

            // Утверждение
            Assert.Equal(3, allUnits.Count);
            Assert.Contains(mage, allUnits);
            Assert.Contains(tank, allUnits);
            Assert.Contains(healer, allUnits);
        }

        [Fact]
        public void AddRemoveSequence_WorksCorrectly()
        {
            // Подготовка
            var inventory = new PlayersInventory(10);
            var unit1 = new Mage(Team.Blue);
            var unit2 = new Tank(Team.Blue);
            var unit3 = new Healer(Team.Blue);

            // Действие и утверждение
            inventory.AddUnits(new ICharacter[] { unit1 });
            Assert.Equal(1, inventory.GetAllUnits().Count());

            inventory.AddUnits(new ICharacter[] { unit2, unit3 });
            Assert.Equal(3, inventory.GetAllUnits().Count());

            inventory.RemoveUnits(new ICharacter[] { unit1 });
            Assert.Equal(2, inventory.GetAllUnits().Count());
            Assert.DoesNotContain(unit1, inventory.GetAllUnits());

            inventory.RemoveUnits(new ICharacter[] { unit2, unit3 });
            Assert.Equal(0, inventory.GetAllUnits().Count());
        }
    }
}
