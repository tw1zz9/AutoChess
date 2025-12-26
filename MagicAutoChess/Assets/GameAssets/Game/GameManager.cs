using System;
using System.Collections.Generic;
using System.Linq;
using GameAssets.Combat;
using GameAssets.Economy;
using GameAssets.Entities;
using GameAssets.Factories;
using GameAssets.Field;
using GameAssets.Interfaces;
using GameAssets.Player;
using UnityEngine;
using GameAssets.Views;
using GameAssets;

namespace GameAssets.Game
{
    public enum GamePhase
    {
        Preparation, // Фаза подготовки (расстановка юнитов)
        Combat,      // Фаза боя
        GameOver     // Игра окончена
    }

    public enum PlayerTurn
    {
        Player1,
        Player2
    }

    /// <summary>
    /// Главный менеджер игры Auto Chess
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Игровые компоненты
        public Board GameBoard { get; private set; }
        public TurnManager CombatManager { get; private set; }
        public Views.BoardVisualizer BoardVisualizer { get; private set; }

        // Игроки
        public PlayersInventory Player1Inventory { get; private set; }
        public PlayersInventory Player2Inventory { get; private set; }

        // Состояние игры
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Preparation;
        public PlayerTurn CurrentTurn { get; private set; } = PlayerTurn.Player1;
        public int RoundNumber { get; private set; } = 1;

        // Флаги готовности игроков
        private bool _player1Ready = false;
        private bool _player2Ready = false;

        // UI и отображение
        private UnitView _selectedUnitView;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            // Создаем игровые компоненты
            GameBoard = new Board();
            CombatManager = new TurnManager();
            BoardVisualizer = FindFirstObjectByType<Views.BoardVisualizer>();

            // Создаем инвентари игроков со стартовым золотом
            Player1Inventory = new PlayersInventory(EconomyManager.StartingGold);
            Player2Inventory = new PlayersInventory(EconomyManager.StartingGold);

            // Создаем стартовые наборы юнитов
            InitializePlayerUnits(Team.Blue, Player1Inventory);
            InitializePlayerUnits(Team.Red, Player2Inventory);

            Debug.Log("Auto Chess game initialized!");
        }

        private void InitializePlayerUnits(Team team, PlayersInventory inventory)
        {
            // 3 Trickster, 2 Tank, 2 Mage, 1 Healer
            var units = new List<ICharacter>
            {
                UnitFactory.CreateTrickster(team),
                UnitFactory.CreateTrickster(team),
                UnitFactory.CreateTrickster(team),
                UnitFactory.CreateTank(team),
                UnitFactory.CreateTank(team),
                UnitFactory.CreateMage(team),
                UnitFactory.CreateMage(team),
                UnitFactory.CreateHealer(team)
            };

            inventory.AddUnits(units);
        }

        /// <summary>
        /// Игрок нажал кнопку "Готов"
        /// </summary>
        public void PlayerReady(int playerId)
        {
            if (CurrentPhase != GamePhase.Preparation) return;

            if (playerId == 1) _player1Ready = true;
            if (playerId == 2) _player2Ready = true;

            if (_player1Ready && _player2Ready)
            {
                StartCombatPhase();
            }
        }

        private void StartCombatPhase()
        {
            SetGamePhase(GamePhase.Combat);
            Debug.Log($"Round {RoundNumber}: Combat phase started!");

            // Автоматический бой
            ExecuteCombat();

            // Переход к следующему раунду
            EndRound();
        }

        private void ExecuteCombat()
        {
            // Получаем всех юнитов на поле
            var blueUnits = GameBoard.GetFieldUnits(Team.Blue).ToList();
            var redUnits = GameBoard.GetFieldUnits(Team.Red).ToList();

            // Регистрируем атакующих
            foreach (var unit in blueUnits.Concat(redUnits))
            {
                if (unit is IDamager damager)
                {
                    CombatManager.RegisterAttacker(damager);
                }
                if (unit is IHealer healer)
                {
                    CombatManager.RegisterHealer(healer);
                }
            }

            // Выполняем бой
            CombatManager.ResolveBattle(blueUnits, redUnits);
        }

        private void EndRound()
        {
            // Подсчитываем живых юнитов
            int blueAlive = GameBoard.GetFieldUnits(Team.Blue).Count(u => u.IsAlive());
            int redAlive = GameBoard.GetFieldUnits(Team.Red).Count(u => u.IsAlive());

            // Определяем победителя раунда
            Team roundWinner = blueAlive > redAlive ? Team.Blue :
                              redAlive > blueAlive ? Team.Red : Team.Blue; // Ничья = победа синих

            // Выдаем золото
            int reward = EconomyManager.CalculateRoundReward(RoundNumber);
            if (roundWinner == Team.Blue)
                Player1Inventory.AddAmount(reward);
            else
                Player2Inventory.AddAmount(reward);

            // Проверяем условия окончания игры
            bool blueHasLivingUnits = Player1Inventory.GetAllUnits().Any(u => u.IsAlive()) || blueAlive > 0;
            bool redHasLivingUnits = Player2Inventory.GetAllUnits().Any(u => u.IsAlive()) || redAlive > 0;

            if (!blueHasLivingUnits || !redHasLivingUnits)
            {
                EndGame(roundWinner);
                return;
            }

            // Возвращаем юнитов в инвентарь
            ReturnUnitsToInventory();

            // Начинаем новый раунд
            StartNewRound();
        }

        /// <summary>
        /// Возвращает всех юнитов с поля обратно в инвентарь
        /// </summary>
        private void ReturnUnitsToInventory()
        {
            // Возвращаем синих юнитов
            var blueUnits = GameBoard.GetFieldUnits(Team.Blue);
            foreach (var unit in blueUnits)
            {
                Player1Inventory.AddUnit(unit);
            }

            // Возвращаем красных юнитов
            var redUnits = GameBoard.GetFieldUnits(Team.Red);
            foreach (var unit in redUnits)
            {
                Player2Inventory.AddUnit(unit);
            }
        }

        private void StartNewRound()
        {
            // Проверяем, есть ли у игроков юниты для продолжения игры
            if (!Player1Inventory.GetAllUnits().Any())
            {
                EndGame(Team.Red); // Синий игрок проигрывает
                return;
            }
            if (!Player2Inventory.GetAllUnits().Any())
            {
                EndGame(Team.Blue); // Красный игрок проигрывает
                return;
            }

            RoundNumber++;
            SetGamePhase(GamePhase.Preparation);
            _player1Ready = false;
            _player2Ready = false;

            // Очищаем поле
            GameBoard.ClearBoard();
            if (BoardVisualizer != null)
            {
                BoardVisualizer.ClearBoardVisual();
            }

            // Сбрасываем цели всех юнитов
            ResetAllTargets(Player1Inventory.GetAllUnits().Concat(Player2Inventory.GetAllUnits()));

            Debug.Log($"Round {RoundNumber}: Preparation phase started!");
        }

        private void ResetAllTargets(IEnumerable<ICharacter> units)
        {
            foreach (var unit in units)
            {
                // ResetTarget определен в конкретных классах, используем приведение типов
                if (unit is Entities.Tank tank) tank.ResetTarget();
                else if (unit is Entities.Mage mage) mage.ResetTarget();
                else if (unit is Entities.Trickster trickster) trickster.ResetTarget();
                else if (unit is Entities.Healer healer) healer.ResetTarget();
            }
        }

        private void EndGame(Team winner)
        {
            SetGamePhase(GamePhase.GameOver);
            Debug.Log($"Game Over! Winner: {winner}");
        }

        #region Unit Management

        /// <summary>
        /// Разместить юнит на поле
        /// </summary>
        public bool PlaceUnitOnBoard(ICharacter unit, int x, int y)
        {
            if (CurrentPhase != GamePhase.Preparation) return false;

            bool success = GameBoard.PlaceUnit(unit, x, y);
            if (success)
            {
                // Удаляем юнит из инвентаря после размещения на поле
                var inventory = unit.Team == Team.Blue ? Player1Inventory : Player2Inventory;
                inventory.RemoveUnit(unit);

                // Обновляем визуализацию
                if (BoardVisualizer != null)
                {
                    BoardVisualizer.PlaceUnitVisual(unit, x, y);
                }
            }
            return success;
        }

        /// <summary>
        /// Убрать юнит с поля обратно в инвентарь
        /// </summary>
        public void RemoveUnitFromBoard(ICharacter unit)
        {
            if (CurrentPhase != GamePhase.Preparation)
                return;

            GameBoard.RemoveUnit(unit);
        }

        /// <summary>
        /// Апгрейднуть юнит
        /// </summary>
        public bool UpgradeUnit(ICharacter unit)
        {
            if (CurrentPhase != GamePhase.Preparation) return false;

            var inventory = unit.Team == Team.Blue ? Player1Inventory : Player2Inventory;
            return inventory.TryUpgradeUnit(unit);
        }

        /// <summary>
        /// Использовать ультимейт
        /// </summary>
        public bool UseUltimate(IUltimate ultimateUser)
        {
            if (CurrentPhase != GamePhase.Combat) return false;

            // IUltimate не содержит Team, приводим к ICharacter
            var character = (ICharacter)ultimateUser;
            var inventory = character.Team == Team.Blue ? Player1Inventory : Player2Inventory;
            return inventory.TryUseUltimate(ultimateUser);
        }

        #endregion

        #region UI Interaction

        public void UnitSelected(UnitView unitView)
        {
            if (CurrentPhase != GamePhase.Preparation) return;

            _selectedUnitView = unitView;
            unitView.Highlight(true);
        }

        public void UnitDeselected(UnitView unitView)
        {
            if (_selectedUnitView == unitView)
            {
                _selectedUnitView = null;
                unitView.Highlight(false);
            }
        }

        public void CellSelected(int x, int y)
        {
            if (CurrentPhase != GamePhase.Preparation || _selectedUnitView == null) return;

            if (GameBoard.PlaceUnit(_selectedUnitView.Character, x, y))
            {
                _selectedUnitView.Highlight(false);
                _selectedUnitView = null;
            }
        }

        /// <summary>
        /// Получить инвентарь игрока по команде
        /// </summary>
        public PlayersInventory GetPlayerInventory(Team team)
        {
            return team == Team.Blue ? Player1Inventory : Player2Inventory;
        }

        #endregion

        #region Save/Load System

        /// <summary>
        /// Сохранить текущее состояние игры
        /// </summary>
        public void SaveGame()
        {
            GameAssets.SaveSystem.SaveManager.SaveGame(this);
        }

        /// <summary>
        /// Загрузить сохраненное состояние игры
        /// </summary>
        public bool LoadGame()
        {
            return GameAssets.SaveSystem.SaveManager.LoadGame(this);
        }

        /// <summary>
        /// Проверить, существует ли сохранение
        /// </summary>
        public bool HasSaveGame()
        {
            return GameAssets.SaveSystem.SaveManager.SaveExists();
        }

        /// <summary>
        /// Удалить сохранение
        /// </summary>
        public void DeleteSaveGame()
        {
            GameAssets.SaveSystem.SaveManager.DeleteSave();
        }

        /// <summary>
        /// Установить фазу игры (с автосохранением)
        /// </summary>
        public void SetGamePhase(GamePhase newPhase)
        {
            CurrentPhase = newPhase;

            // Автосохранение при изменении фазы
            if (newPhase == GamePhase.Preparation || newPhase == GamePhase.Combat)
            {
                SaveGame();
            }

            Debug.Log($"Game phase changed to: {newPhase}");
        }

        /// <summary>
        /// Установить номер раунда (для загрузки сохранения)
        /// </summary>
        public void SetRoundNumber(int roundNumber)
        {
            RoundNumber = roundNumber;
        }

        #endregion
    }
}
