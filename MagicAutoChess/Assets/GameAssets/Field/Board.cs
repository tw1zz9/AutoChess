using GameAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameAssets.Field
{
    /// <summary>
    /// Игровое поле 5x1 клеток для каждого игрока
    /// </summary>
    public class Board
    {
        private const int BOARD_WIDTH = 5;
        private const int BOARD_HEIGHT = 1;

        private readonly Dictionary<(int x, int y, Team team), Cell> _cells = new();

        public Board()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Создаем клетки для обеих команд
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    for (int y = 0; y < BOARD_HEIGHT; y++)
                    {
                        var position = (x, y, team);
                        _cells[position] = new Cell(position.x, position.y, team);
                    }
                }
            }
        }

        /// <summary>
        /// Получить клетку по позиции
        /// </summary>
        public Cell GetCell(int x, int y, Team team)
        {
            var position = (x, y, team);
            return _cells.ContainsKey(position) ? _cells[position] : null;
        }

        /// <summary>
        /// Получить все клетки команды
        /// </summary>
        public IEnumerable<Cell> GetTeamCells(Team team)
        {
            return _cells.Where(kvp => kvp.Key.team == team).Select(kvp => kvp.Value);
        }

        /// <summary>
        /// Получить все занятые клетки команды
        /// </summary>
        public IEnumerable<Cell> GetOccupiedCells(Team team)
        {
            return GetTeamCells(team).Where(cell => cell.IsOccupied());
        }

        /// <summary>
        /// Получить все свободные клетки команды
        /// </summary>
        public IEnumerable<Cell> GetFreeCells(Team team)
        {
            return GetTeamCells(team).Where(cell => !cell.IsOccupied());
        }

        /// <summary>
        /// Разместить юнит на клетке
        /// </summary>
        public bool PlaceUnit(ICharacter unit, int x, int y)
        {
            var cell = GetCell(x, y, unit.Team);
            if (cell == null || cell.IsOccupied()) return false;

            // Убираем юнит с предыдущей клетки если он был размещен
            RemoveUnit(unit);
            cell.SetCharacter(unit);
            return true;
        }

        /// <summary>
        /// Разместить юнит на клетке (перегрузка для совместимости)
        /// </summary>
        public bool PlaceUnit(ICharacter unit, int x, int y, Team team)
        {
            if (unit.Team != team) return false;
            return PlaceUnit(unit, x, y);
        }

        /// <summary>
        /// Убрать юнит с поля
        /// </summary>
        public void RemoveUnit(ICharacter unit)
        {
            var occupiedCell = GetTeamCells(unit.Team).FirstOrDefault(cell => cell.ExistingCharacter?.ID == unit.ID);
            occupiedCell?.RemoveCharacter();
        }

        /// <summary>
        /// Получить позицию юнита на поле
        /// </summary>
        public (int x, int y)? GetUnitPosition(ICharacter unit)
        {
            var cell = GetTeamCells(unit.Team).FirstOrDefault(c => c.ExistingCharacter?.ID == unit.ID);
            return cell != null ? (cell.X, cell.Y) : null;
        }

        /// <summary>
        /// Получить всех юнитов на поле для команды
        /// </summary>
        public IEnumerable<ICharacter> GetFieldUnits(Team team)
        {
            return GetOccupiedCells(team).Select(cell => cell.ExistingCharacter);
        }

        /// <summary>
        /// Очистить поле для нового раунда
        /// </summary>
        public void ClearBoard()
        {
            foreach (var cell in _cells.Values)
            {
                cell.RemoveCharacter();
            }
        }

        /// <summary>
        /// Очистить все юниты с поля (для загрузки сохранения)
        /// </summary>
        public void ClearAllUnits()
        {
            foreach (var cell in _cells.Values)
            {
                cell.RemoveCharacter();
            }
        }

        /// <summary>
        /// Получить все занятые клетки
        /// </summary>
        public IEnumerable<Cell> GetAllOccupiedCells()
        {
            return _cells.Values.Where(cell => cell.IsOccupied());
        }

        /// <summary>
        /// Получить все клетки
        /// </summary>
        public IEnumerable<Cell> GetAllCells()
        {
            return _cells.Values;
        }
    }
}