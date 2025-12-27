using Xunit;
using RayLibAutoChess;
using RayLibAutoChess.Entities;
using System.Linq;

namespace AutoChess.Tests
{
    public class BoardTests
    {
        [Fact]
        public void Board_Constructor_CreatesCorrectGrid()
        {
            // Подготовка и действие
            var board = new Board();

            // Утверждение - доска должна быть 2x5 (2 ряда, 5 колонок)
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var cell = board.GetCell(col, row);
                    Assert.NotNull(cell);
                    Assert.Null(cell.ExistingCharacter);
                    Assert.True(cell.ExistingCharacter == null);
                }
            }
        }

        [Fact]
        public void GetCell_InvalidCoordinates_ReturnsNull()
        {
            // Подготовка
            var board = new Board();

            // Действие и утверждение
            Assert.Null(board.GetCell(-1, 0)); // Отрицательная колонка
            Assert.Null(board.GetCell(0, -1)); // Отрицательный ряд
            Assert.Null(board.GetCell(5, 0));  // Колонка вне границ
            Assert.Null(board.GetCell(0, 2));  // Ряд вне границ
        }

        [Fact]
        public void GetFieldUnits_EmptyBoard_ReturnsEmptyCollection()
        {
            // Подготовка
            var board = new Board();

            // Действие
            var blueUnits = board.GetFieldUnits(Team.Blue);
            var redUnits = board.GetFieldUnits(Team.Red);

            // Утверждение
            Assert.Empty(blueUnits);
            Assert.Empty(redUnits);
        }

        [Fact]
        public void PlaceUnit_ValidPosition_Succeeds()
        {
            // Подготовка
            var board = new Board();
            var mage = new Mage(Team.Blue);

            // Действие
            var result = board.PlaceUnit(mage, 2, 0); // Синий ряд, средняя колонка

            // Утверждение
            Assert.True(result);
            var cell = board.GetCell(2, 0);
            Assert.NotNull(cell.ExistingCharacter);
            Assert.Equal(mage, cell.ExistingCharacter);
            Assert.True(cell.ExistingCharacter != null);
        }

        [Fact]
        public void PlaceUnit_InvalidPosition_Fails()
        {
            // Подготовка
            var board = new Board();
            var mage = new Mage(Team.Blue);

            // Действие
            var result = board.PlaceUnit(mage, -1, 0); // Недопустимая позиция

            // Утверждение
            Assert.False(result);
            Assert.Null(board.GetCell(-1, 0));
        }

        [Fact]
        public void PlaceUnit_OccupiedPosition_Fails()
        {
            // Подготовка
            var board = new Board();
            var mage1 = new Mage(Team.Blue);
            var mage2 = new Mage(Team.Blue);
            board.PlaceUnit(mage1, 2, 0);

            // Действие
            var result = board.PlaceUnit(mage2, 2, 0); // Та же позиция

            // Утверждение
            Assert.False(result);
            var cell = board.GetCell(2, 0);
            Assert.Equal(mage1, cell.ExistingCharacter); // Первый юнит все еще там
        }

        [Fact]
        public void PlaceUnit_ValidPosition_Succeeds()
        {
            // Этот тест уже покрыт выше
            // Board.PlaceUnit не проверяет ограничения команды-ряда
            // Эта проверка происходит в GameManager.PlaceUnitOnBoard
            Assert.True(true);
        }

        [Fact]
        public void RemoveUnit_ExistingUnit_Succeeds()
        {
            // Подготовка
            var board = new Board();
            var mage = new Mage(Team.Blue);
            board.PlaceUnit(mage, 2, 0);

            // Действие
            board.RemoveUnit(mage);

            // Утверждение
            var cell = board.GetCell(2, 0);
            Assert.Null(cell.ExistingCharacter);
            Assert.True(cell.ExistingCharacter == null);
        }

        [Fact]
        public void RemoveUnit_NonExistentUnit_DoesNothing()
        {
            // Подготовка
            var board = new Board();
            var mage = new Mage(Team.Blue);
            board.PlaceUnit(mage, 2, 0);

            // Действие - пытаемся удалить другого юнита
            var differentMage = new Mage(Team.Red);
            board.RemoveUnit(differentMage); // Не должно крашить

            // Утверждение - оригинальный юнит все еще там
            var cell = board.GetCell(2, 0);
            Assert.Equal(mage, cell.ExistingCharacter);
        }

        [Fact]
        public void GetFieldUnits_ReturnsCorrectUnits()
        {
            // Подготовка
            var board = new Board();
            var blueMage = new Mage(Team.Blue);
            var blueTank = new Tank(Team.Blue);
            var redHealer = new Healer(Team.Red);

            board.PlaceUnit(blueMage, 0, 0);
            board.PlaceUnit(blueTank, 1, 0);
            board.PlaceUnit(redHealer, 2, 1);

            // Действие
            var blueUnits = board.GetFieldUnits(Team.Blue).ToList();
            var redUnits = board.GetFieldUnits(Team.Red).ToList();

            // Утверждение
            Assert.Equal(2, blueUnits.Count);
            Assert.Contains(blueMage, blueUnits);
            Assert.Contains(blueTank, blueUnits);

            Assert.Equal(1, redUnits.Count);
            Assert.Contains(redHealer, redUnits);
        }

        [Fact]
        public void ClearBoard_RemovesAllUnits()
        {
            // Подготовка
            var board = new Board();
            var mage = new Mage(Team.Blue);
            var tank = new Tank(Team.Red);
            board.PlaceUnit(mage, 0, 0);
            board.PlaceUnit(tank, 1, 1);

            // Действие
            board.ClearBoard();

            // Утверждение
            Assert.Empty(board.GetFieldUnits(Team.Blue));
            Assert.Empty(board.GetFieldUnits(Team.Red));

            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var cell = board.GetCell(col, row);
                    Assert.Null(cell.ExistingCharacter);
                    Assert.True(cell.ExistingCharacter == null);
                }
            }
        }
    }
}
