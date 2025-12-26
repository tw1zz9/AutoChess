using System;
using System.Collections.Generic;
using System.Linq;

namespace RayLibAutoChess
{
    public class GameManager
    {
        public static GameManager Instance { get; private set; } = null!;

        public Board GameBoard { get; private set; } = null!;
        public TurnManager CombatManager { get; private set; } = null!;

        public PlayersInventory Player1Inventory { get; private set; } = null!;
        public PlayersInventory Player2Inventory { get; private set; } = null!;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Preparation;
        public PlayerTurn CurrentTurn { get; private set; } = PlayerTurn.Player1;
        public int RoundNumber { get; private set; } = 1;

        private bool _player1Ready = false;
        private bool _player2Ready = false;

        private static int GetTeamRow(Team team) => team == Team.Blue ? 0 : 1;

        private readonly Stack<Action> _undoStack = new();
        private bool _suppressUndoRecording;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                InitializeGame();
            }
            else
            {
                throw new InvalidOperationException("GameManager instance already exists");
            }
        }

        public GameManager()
        {
            Awake();
        }

        private void InitializeGame()
        {
            GameBoard = new Board();
            CombatManager = new TurnManager();

            Player1Inventory = new PlayersInventory(EconomyManager.StartingGold);
            Player2Inventory = new PlayersInventory(EconomyManager.StartingGold);

            InitializePlayerUnits(Team.Blue, Player1Inventory);
            InitializePlayerUnits(Team.Red, Player2Inventory);

            Console.WriteLine("Auto Chess game initialized!");
        }

        private void InitializePlayerUnits(Team team, PlayersInventory inventory)
        {
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

        public void PlayerReady(int playerId)
        {
            if (playerId != 1 && playerId != 2)
                throw new ArgumentException("Player ID must be 1 or 2.", nameof(playerId));

            if (CurrentPhase != GamePhase.Preparation)
                throw new InvalidOperationException("Can only ready players during preparation phase.");

            // Check if both players have at least one unit on the board
            bool blueHasUnits = GameBoard.GetFieldUnits(Team.Blue).Any();
            bool redHasUnits = GameBoard.GetFieldUnits(Team.Red).Any();

            if (!blueHasUnits || !redHasUnits)
            {
                // Don't start combat yet, wait for both players to place units
                Console.WriteLine("Waiting for both players to place at least one unit on the board.");
                return;
            }

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
            Console.WriteLine($"Round {RoundNumber}: Combat phase started!");

            // Undo is only available in preparation.
            _undoStack.Clear();

            ExecuteCombat();
            EndRound();
        }

        private void ExecuteCombat()
        {
            var blueUnits = GameBoard.GetFieldUnits(Team.Blue).ToList();
            var redUnits = GameBoard.GetFieldUnits(Team.Red).ToList();

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

            CombatManager.ResolveBattle(blueUnits, redUnits);
        }

        private void EndRound()
        {
            int blueAlive = GameBoard.GetFieldUnits(Team.Blue).Count(u => u.IsAlive());
            int redAlive = GameBoard.GetFieldUnits(Team.Red).Count(u => u.IsAlive());

            Team roundWinner = blueAlive > redAlive ? Team.Blue :
                              redAlive > blueAlive ? Team.Red : Team.Blue;

            int reward = EconomyManager.CalculateRoundReward(RoundNumber);
            if (roundWinner == Team.Blue)
                Player1Inventory.AddAmount(reward);
            else
                Player2Inventory.AddAmount(reward);

            bool blueHasLivingUnits = Player1Inventory.GetAllUnits().Any(u => u.IsAlive()) || blueAlive > 0;
            bool redHasLivingUnits = Player2Inventory.GetAllUnits().Any(u => u.IsAlive()) || redAlive > 0;

            if (!blueHasLivingUnits || !redHasLivingUnits)
            {
                EndGame(roundWinner);
                return;
            }

            ReturnUnitsToInventory();
            StartNewRound();
        }

        private void ReturnUnitsToInventory()
        {
            var blueUnits = GameBoard.GetFieldUnits(Team.Blue);
            foreach (var unit in blueUnits)
            {
                Player1Inventory.AddUnit(unit);
            }

            var redUnits = GameBoard.GetFieldUnits(Team.Red);
            foreach (var unit in redUnits)
            {
                Player2Inventory.AddUnit(unit);
            }
        }

        private void StartNewRound()
        {
            if (!Player1Inventory.GetAllUnits().Any())
            {
                EndGame(Team.Red);
                return;
            }
            if (!Player2Inventory.GetAllUnits().Any())
            {
                EndGame(Team.Blue);
                return;
            }

            RoundNumber++;
            SetGamePhase(GamePhase.Preparation);
            _player1Ready = false;
            _player2Ready = false;
            _undoStack.Clear();

            GameBoard.ClearBoard();
            ResetAllTargets(Player1Inventory.GetAllUnits().Concat(Player2Inventory.GetAllUnits()));

            Console.WriteLine($"Round {RoundNumber}: Preparation phase started!");
        }

        private void ResetAllTargets(IEnumerable<ICharacter> units)
        {
            foreach (var unit in units)
            {
                if (unit is Entities.Tank tank) tank.ResetTarget();
                else if (unit is Entities.Mage mage) mage.ResetTarget();
                else if (unit is Entities.Trickster trickster) trickster.ResetTarget();
                else if (unit is Entities.Healer healer) healer.ResetTarget();
            }
        }

        private void EndGame(Team winner)
        {
            SetGamePhase(GamePhase.GameOver);
            Console.WriteLine($"Game Over! Winner: {winner}");
        }

        public bool PlaceUnitOnBoard(ICharacter unit, int x, int y)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit), "Unit cannot be null.");

            if (x < 0 || x >= 5)
                throw new ArgumentOutOfRangeException(nameof(x), "X coordinate must be between 0 and 4.");

            if (y < 0 || y >= 2)
                throw new ArgumentOutOfRangeException(nameof(y), "Y coordinate must be 0 (Blue row) or 1 (Red row).");

            if (CurrentPhase != GamePhase.Preparation)
                throw new InvalidOperationException("Can only place units during preparation phase.");

            if (!unit.IsAlive())
                throw new ArgumentException("Cannot place dead units on board.", nameof(unit));

            int expectedRow = GetTeamRow(unit.Team);
            if (y != expectedRow)
                throw new InvalidOperationException($"Unit from team {unit.Team} can only be placed on row {expectedRow}.");

            bool success = GameBoard.PlaceUnit(unit, x, y);
            if (success)
            {
                var inventory = unit.Team == Team.Blue ? Player1Inventory : Player2Inventory;
                inventory.RemoveUnit(unit);

                RecordUndo(() =>
                {
                    // Undo placement: remove from board and return to inventory.
                    GameBoard.RemoveUnit(unit);
                    inventory.AddUnit(unit);
                });
            }
            return success;
        }

        public void RemoveUnitFromBoard(ICharacter unit)
        {
            if (CurrentPhase != GamePhase.Preparation)
                return;

            GameBoard.RemoveUnit(unit);

            // Return back to the owner's inventory so players can reposition during preparation.
            var inventory = unit.Team == Team.Blue ? Player1Inventory : Player2Inventory;
            inventory.AddUnit(unit);
        }

        public bool UpgradeUnit(ICharacter unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit), "Unit cannot be null.");

            if (CurrentPhase != GamePhase.Preparation)
                throw new InvalidOperationException("Can only upgrade units during preparation phase.");

            if (!unit.IsAlive())
                throw new ArgumentException("Cannot upgrade dead units.", nameof(unit));

            var inventory = unit.Team == Team.Blue ? Player1Inventory : Player2Inventory;
            return inventory.TryUpgradeUnit(unit);
        }

        public bool UseUltimate(IUltimate ultimateUser)
        {
            if (ultimateUser == null)
                throw new ArgumentNullException(nameof(ultimateUser), "Ultimate user cannot be null.");

            if (CurrentPhase != GamePhase.Combat && CurrentPhase != GamePhase.Preparation)
                throw new InvalidOperationException("Can only use ultimates during preparation or combat phase.");

            var character = (ICharacter)ultimateUser;
            if (!character.IsAlive())
                throw new ArgumentException("Cannot use ultimate with dead units.", nameof(ultimateUser));

            var inventory = character.Team == Team.Blue ? Player1Inventory : Player2Inventory;
            int cost = EconomyManager.GetUltimateCost(ultimateUser);
            bool success = inventory.TryUseUltimate(ultimateUser);

            if (success && CurrentPhase == GamePhase.Preparation)
            {
                RecordUndo(() =>
                {
                    inventory.AddAmount(cost);
                    if (ultimateUser is IUltimateResettable resettable)
                        resettable.ResetUltimateState();
                });
            }

            return success;
        }

        public bool UndoLastAction()
        {
            if (CurrentPhase != GamePhase.Preparation)
                return false;

            if (_undoStack.Count == 0)
                return false;

            _suppressUndoRecording = true;
            try
            {
                _undoStack.Pop().Invoke();
                return true;
            }
            finally
            {
                _suppressUndoRecording = false;
            }
        }

        private void RecordUndo(Action undo)
        {
            if (_suppressUndoRecording) return;
            if (CurrentPhase != GamePhase.Preparation) return;
            _undoStack.Push(undo);
        }

        public PlayersInventory GetPlayerInventory(Team team)
        {
            if (team != Team.Blue && team != Team.Red)
                throw new ArgumentException("Team must be Blue or Red.", nameof(team));

            return team == Team.Blue ? Player1Inventory : Player2Inventory;
        }

        public void SetGamePhase(GamePhase newPhase)
        {
            CurrentPhase = newPhase;
            Console.WriteLine($"Game phase changed to: {newPhase}");
        }

        public void SetRoundNumber(int roundNumber)
        {
            RoundNumber = roundNumber;
        }
    }
}
