using System.Collections.Generic;
using System.Linq;

namespace RayLibAutoChess
{
    public class Board
    {
        private readonly Cell[,] _cells;
        public int Width => _cells.GetLength(0);
        public int Height => _cells.GetLength(1);

        // Shared board: 5 columns x 2 rows (Blue row + Red row)
        public Board(int width = 5, int height = 2)
        {
            _cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new Cell(x, y);
                }
            }
        }

        public bool PlaceUnit(ICharacter unit, int x, int y)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;

            var cell = _cells[x, y];
            if (cell.ExistingCharacter != null)
                return false;

            cell.SetCharacter(unit);
            return true;
        }

        public void RemoveUnit(ICharacter unit)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = _cells[x, y];
                    if (cell.ExistingCharacter?.ID == unit.ID)
                    {
                        cell.RemoveCharacter();
                        return;
                    }
                }
            }
        }

        public void ClearBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _cells[x, y].RemoveCharacter();
                }
            }
        }

        public IEnumerable<ICharacter> GetFieldUnits(Team team)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var character = _cells[x, y].ExistingCharacter;
                    if (character != null && character.Team == team)
                    {
                        yield return character;
                    }
                }
            }
        }

        public Cell? GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return _cells[x, y];
        }

        public IEnumerable<Cell> GetAllCells()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return _cells[x, y];
                }
            }
        }
    }
}
