using Raylib_cs;
using static Raylib_cs.Raylib;
using System.IO;
using RayLibAutoChess.Entities;

namespace RayLibAutoChess
{
    public class Render
    {
        private const int ScreenWidth = 1200;
        private const int ScreenHeight = 800;
        private const int CellSize = 80;
        private const int BoardRows = 2;
        private const int BoardCols = 5;
        private const int TopBarHeight = 70;
        private const int PanelWidth = 320;
        private const int PanelMarginX = 30;
        private const int PanelTopY = 90;
        private const int PanelPadding = 14;
        private const int PanelRowHeight = 34;

        private const int UiTextLg = 22;
        private const int UiTextMd = 18;
        private const int UiTextSm = 16;
        private const int UiTextXs = 14;

        private const int GameOverButtonWidth = 200;
        private const int GameOverButtonHeight = 50;
        private const int GameOverButtonSpacing = 30;

        private GameManager _gameManager;
        private Font _font;

        private ICharacter? _selectedBlueUnit;
        private ICharacter? _selectedRedUnit;

        // Режим выбора цели для таргетированных ультимейтов (например, Маг)
        private bool _isSelectingUltimateTarget;
        private Team _ultimateCasterTeam;
        private ITargetedUltimate? _ultimateCaster;

        // Вычисленная компоновка
        private int _boardOffsetX;
        private int _boardOffsetY;

        public Render(GameManager gameManager)
        {
            _gameManager = gameManager;
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Auto Chess");
            Raylib.SetTargetFPS(60);

            // Шрифт: пытаемся загрузить из assets, если есть, иначе используем по умолчанию, чтобы избежать предупреждений во время выполнения.
            const string fontPath = "assets/fonts/default.ttf";
            _font = File.Exists(fontPath) ? LoadFont(fontPath) : GetFontDefault();

            RecomputeLayout();
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                Update();
                Draw();
            }

            Cleanup();
        }

        private void Update()
        {
            // Обрабатываем ввод
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                HandleMouseClick();
            }

            if (_isSelectingUltimateTarget && Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                _isSelectingUltimateTarget = false;
                _ultimateCaster = null;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                // Готовим игроков
                if (_gameManager.CurrentPhase == GamePhase.Preparation)
                {
                    _gameManager.PlayerReady(1);
                    _gameManager.PlayerReady(2);
                }
            }

            if (_gameManager.CurrentPhase == GamePhase.Preparation)
            {
                // Улучшение / Ультимейт для текущего выбранного юнита (синий имеет приоритет, если выбраны оба)
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    try { _gameManager.UndoLastAction(); } catch { /* ignore for UI */ }
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_U))
                {
                    var unit = _selectedBlueUnit ?? _selectedRedUnit;
                    if (unit != null)
                    {
                        try { _gameManager.UpgradeUnit(unit); } catch { /* ignore for UI */ }
                    }
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_O))
                {
                    // Если уже в режиме прицеливания, отменяем его
                    if (_isSelectingUltimateTarget)
                    {
                        _isSelectingUltimateTarget = false;
                        _ultimateCaster = null;
                    }
                    else
                    {
                        var unit = _selectedBlueUnit ?? _selectedRedUnit;
                        if (unit is IUltimate ultimate)
                        {
                            var team = _selectedBlueUnit != null ? Team.Blue : Team.Red;
                            var inventory = _gameManager.GetPlayerInventory(team);

                            if (ultimate is ITargetedUltimate targeted)
                            {
                                // Входим в режим прицеливания вместо немедленной траты золота.
                                if (ultimate.CanUseUltimate() && inventory.Gold >= EconomyManager.GetUltimateCost(ultimate))
                                {
                                    _isSelectingUltimateTarget = true;
                                    _ultimateCasterTeam = team;
                                    _ultimateCaster = targeted;
                                }
                            }
                            else
                            {
                                try { _gameManager.UseUltimate(ultimate); } catch { /* ignore for UI */ }
                            }
                        }
                    }
                }
            }
        }

        private void HandleMouseClick()
        {
            var mousePos = Raylib.GetMousePosition();

            // Обрабатываем кнопки GameOver
            if (_gameManager.CurrentPhase == GamePhase.GameOver)
            {
                if (IsPointInRect(mousePos, GetRestartGameButtonRect()))
                {
                    _gameManager.RestartGame();
                    return;
                }

                if (IsPointInRect(mousePos, GetQuitGameButtonRect()))
                {
                    Raylib.CloseWindow();
                    return;
                }
            }

            if (_gameManager.CurrentPhase == GamePhase.Preparation)
            {
                // Сначала кнопки действий
                if (TryHandleActionButtonsClick(mousePos, Team.Blue)) return;
                if (TryHandleActionButtonsClick(mousePos, Team.Red)) return;

                if (TryHandleInventoryClick(mousePos, Team.Blue)) return;
                if (TryHandleInventoryClick(mousePos, Team.Red)) return;
            }

            // Проверяем, кликнули ли по доске
            if (mousePos.Y >= _boardOffsetY && mousePos.Y <= _boardOffsetY + BoardRows * CellSize)
            {
                int cellX = (int)((mousePos.X - _boardOffsetX) / CellSize);
                int cellY = (int)((mousePos.Y - _boardOffsetY) / CellSize);

                if (cellX >= 0 && cellX < BoardCols && cellY >= 0 && cellY < BoardRows)
                {
                    // Обрабатываем прицеливание ультимейта, если в режиме прицеливания
                    if (_isSelectingUltimateTarget && _ultimateCaster != null)
                    {
                        if (TryHandleUltimateTargetClick(mousePos))
                            return;
                    }

                    var team = cellY == 0 ? Team.Blue : Team.Red;
                    var selected = team == Team.Blue ? _selectedBlueUnit : _selectedRedUnit;

                    // Размещаем только если игрок выбрал юнита из своего инвентаря
                    if (_gameManager.CurrentPhase == GamePhase.Preparation && selected != null)
                    {
                        bool placed = false;
                        try { placed = _gameManager.PlaceUnitOnBoard(selected, cellX, cellY); } catch { /* ignore for UI */ }
                        if (placed)
                        {
                            if (team == Team.Blue) _selectedBlueUnit = null;
                            else _selectedRedUnit = null;
                        }
                    }
                }
            }
        }

        private bool TryHandleUltimateTargetClick(System.Numerics.Vector2 mousePos)
        {
            // Обрабатываем только клики по доске во время прицеливания
            int boardW = BoardCols * CellSize;
            int boardH = BoardRows * CellSize;

            bool insideBoard = mousePos.X >= _boardOffsetX && mousePos.X <= _boardOffsetX + boardW
                               && mousePos.Y >= _boardOffsetY && mousePos.Y <= _boardOffsetY + boardH;
            if (!insideBoard)
                return false;

            int cellX = (int)((mousePos.X - _boardOffsetX) / CellSize);
            int cellY = (int)((mousePos.Y - _boardOffsetY) / CellSize);
            var cell = _gameManager.GameBoard.GetCell(cellX, cellY);
            var target = cell?.ExistingCharacter;

            // Если нет цели в кликнутой клетке, не потребляем клик
            if (target == null)
                return false;

            // Должен быть союзником (и живым)
            if (target.Team != _ultimateCasterTeam || !target.IsAlive())
                return false;

            // Если кастер является ICharacter, не позволяем прицеливаться на себя для баффовых ультимейтов
            if (_ultimateCaster is ICharacter caster && target.ID == caster.ID)
                return false;

            try
            {
                _ultimateCaster!.SetUltimateTarget(target);
                bool success = _gameManager.UseUltimate(_ultimateCaster);
                if (success)
                {
                    _isSelectingUltimateTarget = false;
                    _ultimateCaster = null;
                }
            }
            catch
            {
                // сохраняем режим прицеливания
            }

            return true;
        }

        private bool TryHandleInventoryClick(System.Numerics.Vector2 mousePos, Team team)
        {
            // Список инвентаря внутри панели
            int panelX = GetPanelX(team);
            int panelY = PanelTopY;
            int listX = panelX + PanelPadding;
            int listY = panelY + 290; // То же самое, что и в DrawInventoryPanel
            int listW = PanelWidth - 2 * PanelPadding;
            int rowH = PanelRowHeight;

            if (mousePos.X < listX || mousePos.X > listX + listW) return false;

            var inventory = _gameManager.GetPlayerInventory(team);
            var units = inventory.GetAllUnits().ToList();

            for (int i = 0; i < units.Count && i < 8; i++)
            {
                int y = listY + i * rowH;
                if (mousePos.Y >= y && mousePos.Y <= y + rowH)
                {
                    if (team == Team.Blue) _selectedBlueUnit = units[i];
                    else _selectedRedUnit = units[i];
                    return true;
                }
            }

            return false;
        }

        private void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(246, 247, 250, 255));

            DrawTopBar();
            DrawBoard();
            DrawUnits();
            DrawUI();
            DrawInfo();

            Raylib.EndDrawing();
        }

        private void DrawTopBar()
        {
            DrawRectangle(0, 0, ScreenWidth, TopBarHeight, new Color(30, 30, 36, 255));
            DrawRectangleLines(0, 0, ScreenWidth, TopBarHeight, new Color(55, 55, 64, 255));

            DrawTextEx(_font, "AUTO CHESS", new System.Numerics.Vector2(20, 18), 30, 1, Color.RAYWHITE);

            string phaseText = $"Phase: {_gameManager.CurrentPhase}";
            string roundText = $"Round: {_gameManager.RoundNumber}";
            DrawTextEx(_font, phaseText, new System.Numerics.Vector2(420, 22), UiTextMd, 1, Color.RAYWHITE);
            DrawTextEx(_font, roundText, new System.Numerics.Vector2(650, 22), UiTextMd, 1, Color.RAYWHITE);

            string hint = _isSelectingUltimateTarget
                ? "TARGETING: click on an ALLY unit to buff it. O or ESC to cancel."
                : "Click unit in inventory -> click your row to place | Upgrade: U or button | Ultimate: O or button | Undo: Z | SPACE: Start combat";
            DrawTextEx(_font, hint, new System.Numerics.Vector2(20, 50), UiTextXs, 1, new Color(200, 200, 210, 255));
        }

        private void DrawBoard()
        {
            // Рисуем фон доски
            int boardW = BoardCols * CellSize;
            int boardH = BoardRows * CellSize;
            Raylib.DrawRectangle(_boardOffsetX - 12, _boardOffsetY - 12, boardW + 24, boardH + 24, new Color(220, 223, 230, 255));
            Raylib.DrawRectangleLines(_boardOffsetX - 12, _boardOffsetY - 12, boardW + 24, boardH + 24, new Color(140, 145, 155, 255));

            // Затемнение рядов, чтобы половинки были очевидны
            DrawRectangle(_boardOffsetX, _boardOffsetY, boardW, CellSize, new Color(70, 120, 255, 26));
            DrawRectangle(_boardOffsetX, _boardOffsetY + CellSize, boardW, CellSize, new Color(255, 70, 70, 26));

            // Рисуем клетки
            for (int y = 0; y < BoardRows; y++)
            {
                for (int x = 0; x < BoardCols; x++)
                {
                    var cell = _gameManager.GameBoard.GetCell(x, y);
                    bool occupied = cell?.ExistingCharacter != null;
                    Color baseColor = occupied ? new Color(230, 233, 240, 255) : new Color(252, 252, 255, 255);
                    int cx = _boardOffsetX + x * CellSize;
                    int cy = _boardOffsetY + y * CellSize;
                    DrawRectangle(cx, cy, CellSize, CellSize, baseColor);
                    DrawRectangleLines(cx, cy, CellSize, CellSize, new Color(70, 70, 78, 255));
                }
            }

            // Метки рядов
            DrawTextEx(_font, "BLUE ROW", new System.Numerics.Vector2(_boardOffsetX, _boardOffsetY - 30), 16, 1, new Color(40, 110, 255, 255));
            DrawTextEx(_font, "RED ROW", new System.Numerics.Vector2(_boardOffsetX, _boardOffsetY + boardH + 16), 16, 1, new Color(230, 60, 60, 255));

            if (_isSelectingUltimateTarget)
            {
                int allyRow = _ultimateCasterTeam == Team.Blue ? 0 : 1;
                var r = new Rectangle(_boardOffsetX, _boardOffsetY + allyRow * CellSize, boardW, CellSize);
                DrawRectangleLinesEx(r, 4, Color.GOLD);
            }
        }

        private void DrawUnits()
        {
            for (int y = 0; y < BoardRows; y++)
            {
                for (int x = 0; x < BoardCols; x++)
                {
                    var cell = _gameManager.GameBoard.GetCell(x, y);
                    if (cell?.ExistingCharacter != null)
                    {
                        var unit = cell.ExistingCharacter;
                        Color unitColor = unit.Team == Team.Blue ? Color.BLUE : Color.RED;

                        // Рисуем юнит как цветной круг
                        int centerX = _boardOffsetX + x * CellSize + CellSize / 2;
                        int centerY = _boardOffsetY + y * CellSize + CellSize / 2;
                        Raylib.DrawCircle(centerX + 2, centerY + 3, 26, new Color(0, 0, 0, 60));
                        Raylib.DrawCircle(centerX, centerY, 25, unitColor);
                        Raylib.DrawCircleLines(centerX, centerY, 25, Color.RAYWHITE);

                        // Рисуем текст здоровья
                        string healthText = $"{unit.Health:F0}";
                        var textSize = MeasureTextEx(_font, healthText, 16, 1);
                        DrawTextEx(_font, healthText,
                            new System.Numerics.Vector2(centerX - textSize.X / 2, centerY - textSize.Y / 2),
                            16, 1, Color.WHITE);
                    }
                }
            }
        }

        private void DrawUI()
        {
            DrawPlayerPanel(Team.Blue);
            DrawPlayerPanel(Team.Red);

            // Подсказка снизу (короткая)
            DrawTextEx(_font, "Tip: click your inventory unit, then click your row to place it.", new System.Numerics.Vector2(20, ScreenHeight - 30), UiTextSm, 1, new Color(80, 80, 92, 255));
        }

        private void DrawPlayerPanel(Team team)
        {
            var inventory = _gameManager.GetPlayerInventory(team);
            int panelX = GetPanelX(team);
            int panelY = PanelTopY;
            int panelH = 610;
            int innerX = panelX + PanelPadding;

            Color teamColor = team == Team.Blue ? new Color(40, 110, 255, 255) : new Color(230, 60, 60, 255);
            string teamName = team == Team.Blue ? "BLUE PLAYER" : "RED PLAYER";
            var selected = team == Team.Blue ? _selectedBlueUnit : _selectedRedUnit;

            // Фон панели
            DrawRectangle(panelX, panelY, PanelWidth, panelH, Color.RAYWHITE);
            DrawRectangleLines(panelX, panelY, PanelWidth, panelH, new Color(160, 165, 175, 255));

            // Полоса заголовка
            DrawRectangle(panelX, panelY, PanelWidth, 42, teamColor);
            DrawTextEx(_font, teamName, new System.Numerics.Vector2(innerX, panelY + 11), UiTextLg, 1, Color.RAYWHITE);

            DrawTextEx(_font, $"Gold: {inventory.Gold}", new System.Numerics.Vector2(innerX, panelY + 60), 22, 1, new Color(30, 30, 36, 255));
            DrawTextEx(_font, $"Inventory: {inventory.GetAllUnits().Count()}/8", new System.Numerics.Vector2(innerX, panelY + 95), UiTextMd, 1, new Color(70, 70, 78, 255));

            string selectedText = selected == null ? "Selected: (none)" : $"Selected: {selected.Name}  L{selected.Level}";
            DrawTextEx(_font, selectedText, new System.Numerics.Vector2(innerX, panelY + 130), UiTextMd, 1, new Color(30, 30, 36, 255));

            DrawActionButtons(team);
            DrawInventoryPanel(team);
        }

        private void DrawInventoryPanel(Team team)
        {
            int panelX = GetPanelX(team);
            int panelY = PanelTopY;
            int listX = panelX + PanelPadding;
            int listY = panelY + 290; // Смещено вниз, чтобы избежать наложения с кнопками
            int listW = PanelWidth - 2 * PanelPadding;
            int listH = 8 * PanelRowHeight;

            DrawTextEx(_font, "Inventory units:", new System.Numerics.Vector2(listX, listY + listH + 8), UiTextMd, 1, new Color(70, 70, 78, 255));
            DrawRectangle(listX, listY, listW, listH, new Color(245, 246, 250, 255));
            DrawRectangleLines(listX, listY, listW, listH, new Color(190, 195, 205, 255));

            var inventory = _gameManager.GetPlayerInventory(team);
            var units = inventory.GetAllUnits().ToList();

            for (int i = 0; i < units.Count && i < 8; i++)
            {
                var unit = units[i];
                int y = listY + i * PanelRowHeight;
                bool selected = team == Team.Blue ? ReferenceEquals(unit, _selectedBlueUnit) : ReferenceEquals(unit, _selectedRedUnit);

                Color rowBg = selected ? new Color(255, 245, 180, 255) : new Color(245, 246, 250, 255);
                DrawRectangle(listX, y, listW, PanelRowHeight - 2, rowBg);
                DrawRectangleLines(listX, y, listW, PanelRowHeight - 2, new Color(190, 195, 205, 255));

                var (dmg, isBuffed) = GetDisplayedDamage(unit);
                string dmgText = isBuffed ? $"DMG:{dmg:F0} (buff)" : $"DMG:{dmg:F0}";
                string text = $"{i + 1}. {unit.Name}  L{unit.Level}  HP:{unit.Health:F0}  {dmgText}";
                DrawTextEx(_font, text, new System.Numerics.Vector2(listX + 8, y + 8), UiTextSm, 1, new Color(30, 30, 36, 255));
            }
        }

        private void DrawInfo()
        {
            if (_gameManager.CurrentPhase == GamePhase.GameOver)
            {
                string gameOverText = "GAME OVER";
                var textSize = MeasureTextEx(_font, gameOverText, 48, 1);
                DrawTextEx(_font, gameOverText,
                    new System.Numerics.Vector2(ScreenWidth / 2 - textSize.X / 2, ScreenHeight / 2 - textSize.Y / 2),
                    48, 1, Color.RED);

                // Рисуем кнопку перезапуска
                DrawButton(GetRestartGameButtonRect(), "НАЧАТЬ СНАЧАЛА", true, Team.Blue, UiTextMd);

                // Рисуем кнопку выхода
                DrawButton(GetQuitGameButtonRect(), "ЗАКРЫТЬ ИГРУ", true, Team.Red, UiTextMd);
            }

            // Панели инвентаря во время подготовки
            if (_gameManager.CurrentPhase == GamePhase.Preparation)
            {
                // Панели рисуются в DrawUI()
            }
        }

        private void DrawActionButtons(Team team)
        {
            var inventory = _gameManager.GetPlayerInventory(team);
            var selected = team == Team.Blue ? _selectedBlueUnit : _selectedRedUnit;

            bool canUpgrade = _gameManager.CurrentPhase == GamePhase.Preparation
                              && selected != null
                              && inventory.Gold >= EconomyManager.GetUpgradeCost(selected);

            bool canUltimate = _gameManager.CurrentPhase == GamePhase.Preparation
                               && selected is IUltimate u
                               && u.CanUseUltimate()
                               && inventory.Gold >= EconomyManager.GetUltimateCost(u);

            int upgradeCost = selected == null ? 0 : EconomyManager.GetUpgradeCost(selected);
            int ultimateCost = selected is IUltimate uu ? EconomyManager.GetUltimateCost(uu) : 0;

            DrawButton(GetUpgradeButtonRect(team), $"UPGRADE  (-{upgradeCost}g)", canUpgrade, team, UiTextMd);
            DrawButton(GetUltimateButtonRect(team), $"ULTIMATE (-{ultimateCost}g)", canUltimate, team, UiTextMd);
        }

        private bool TryHandleActionButtonsClick(System.Numerics.Vector2 mousePos, Team team)
        {
            if (_gameManager.CurrentPhase != GamePhase.Preparation) return false;

            var selected = team == Team.Blue ? _selectedBlueUnit : _selectedRedUnit;
            if (selected == null && (IsPointInRect(mousePos, GetUpgradeButtonRect(team)) || IsPointInRect(mousePos, GetUltimateButtonRect(team))))
                return true; // клик потреблен, но ничего не делаем

            if (IsPointInRect(mousePos, GetUpgradeButtonRect(team)))
            {
                try { if (selected != null) _gameManager.UpgradeUnit(selected); } catch { }
                return true;
            }

            if (IsPointInRect(mousePos, GetUltimateButtonRect(team)))
            {
                if (selected is IUltimate ultimate && ultimate.CanUseUltimate())
                {
                    var inventory = _gameManager.GetPlayerInventory(team);
                    if (ultimate is ITargetedUltimate targeted)
                    {
                        // Входим в режим прицеливания вместо немедленной траты золота.
                        if (inventory.Gold >= EconomyManager.GetUltimateCost(ultimate))
                        {
                            _isSelectingUltimateTarget = true;
                            _ultimateCasterTeam = team;
                            _ultimateCaster = targeted;
                        }
                    }
                    else
                    {
                        try { _gameManager.UseUltimate(ultimate); } catch { }
                    }
                }
                return true;
            }

            return false;
        }

        private Rectangle GetUltimateButtonRect(Team team)
        {
            int panelX = GetPanelX(team);
            int panelY = PanelTopY;
            int innerX = panelX + PanelPadding;
            int btnW = PanelWidth - 2 * PanelPadding;
            int btnH = 42; // Та же высота, что и у кнопки улучшения
            int btnY = panelY + 165; // Позиция после текста выбранного

            return new Rectangle(innerX, btnY, btnW, btnH);
        }

        private Rectangle GetUpgradeButtonRect(Team team)
        {
            int panelX = GetPanelX(team);
            int panelY = PanelTopY;
            int innerX = panelX + PanelPadding;
            int btnW = PanelWidth - 2 * PanelPadding;
            return new Rectangle(innerX, panelY + 210, btnW, 42);
        }

        private Rectangle GetRestartGameButtonRect()
        {
            int centerX = ScreenWidth / 2;
            // Располагаем кнопки значительно ниже текста "GAME OVER" (48px высота шрифта + 50px отступ)
            int centerY = ScreenHeight / 2 + 50 + 50; // высота текста + отступ

            return new Rectangle(
                centerX - GameOverButtonWidth / 2,
                centerY,
                GameOverButtonWidth,
                GameOverButtonHeight
            );
        }

        private Rectangle GetQuitGameButtonRect()
        {
            int centerX = ScreenWidth / 2;
            // Располагаем вторую кнопку ниже первой с правильным отступом
            int centerY = ScreenHeight / 2 + 50 + 50 + GameOverButtonHeight + GameOverButtonSpacing;

            return new Rectangle(
                centerX - GameOverButtonWidth / 2,
                centerY,
                GameOverButtonWidth,
                GameOverButtonHeight
            );
        }

        private void DrawButton(Rectangle rect, string text, bool enabled, Team team, int fontSize = 16)
        {
            var mouse = Raylib.GetMousePosition();
            bool hover = IsPointInRect(mouse, rect);

            Color baseColor = enabled
                ? (team == Team.Blue ? new Color(40, 110, 255, 255) : new Color(230, 60, 60, 255))
                : new Color(170, 175, 185, 255);

            Color hoverColor = enabled
                ? (team == Team.Blue ? new Color(70, 140, 255, 255) : new Color(245, 90, 90, 255))
                : baseColor;

            Color bg = hover ? hoverColor : baseColor;

            DrawRectangleRec(rect, bg);
            DrawRectangleLinesEx(rect, 2, new Color(20, 20, 24, 140));

            var size = MeasureTextEx(_font, text, fontSize, 1);
            DrawTextEx(_font, text,
                new System.Numerics.Vector2(rect.X + rect.Width / 2 - size.X / 2, rect.Y + rect.Height / 2 - size.Y / 2),
                fontSize, 1, Color.RAYWHITE);
        }

        private static bool IsPointInRect(System.Numerics.Vector2 p, Rectangle r)
            => p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;

        private static int GetPanelX(Team team)
            => team == Team.Blue ? PanelMarginX : ScreenWidth - PanelWidth - PanelMarginX;

        private void RecomputeLayout()
        {
            int boardW = BoardCols * CellSize;
            int boardH = BoardRows * CellSize;
            _boardOffsetX = (ScreenWidth - boardW) / 2;
            _boardOffsetY = (ScreenHeight - boardH) / 2; // держим доску точно по центру
        }

        private (double damage, bool isBuffed) GetDisplayedDamage(ICharacter unit)
        {
            if (unit == null) return (0, false);

            double baseDamage = unit is IDamager damager ? damager.Damage : 0;
            double multiplier = GetTeamDamageMultiplierFromActiveMages(unit.Team, unit.ID);
            double effective = baseDamage * multiplier;
            return (effective, multiplier > 1.0001);
        }

        private double GetTeamDamageMultiplierFromActiveMages(Team team, Guid excludeUnitId)
        {
            double multiplier = 1.0;

            // Бафф мага применяется к атакующим на доске; для UI мы отражаем текущие активные баффы.
            foreach (var u in _gameManager.GameBoard.GetFieldUnits(team))
            {
                if (u is Mage mage && mage.IsUltimateActive && mage.ID != excludeUnitId)
                {
                    multiplier = Math.Max(multiplier, mage.BuffMultiplier);
                }
            }

            return multiplier;
        }

        private void Cleanup()
        {
            Raylib.UnloadFont(_font);
            Raylib.CloseWindow();
        }
    }
}
