using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameAssets;
using GameAssets.Game;
using GameAssets.Interfaces;

namespace GameAssets.UI
{
    /// <summary>
    /// UI для отображения инвентаря игрока с возможностью размещения юнитов
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private Team _playerTeam;
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _playerNameText;

        [Header("Unit Slots")]
        [SerializeField] private InventorySlotUI[] _unitSlots;

        [Header("Unit Info Panel")]
        [SerializeField] private GameObject _unitInfoPanel;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _unitStatsText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _placeButton;
        [SerializeField] private Button _ultimateButton;
        [SerializeField] private TextMeshProUGUI _upgradeCostText;
        [SerializeField] private TextMeshProUGUI _ultimateCostText;

        private Interfaces.ICharacter _selectedUnit;
        private Player.PlayersInventory _playerInventory;

        private void Start()
        {
            InitializeUI();
            UpdateInventoryDisplay();
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentPhase == GamePhase.Preparation)
            {
                UpdateInventoryDisplay();
            }
        }

        private void InitializeUI()
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = _playerTeam == Team.Blue ? "СИНИЙ ИГРОК" : "КРАСНЫЙ ИГРОК";
                _playerNameText.color = _playerTeam == Team.Blue ? Color.blue : Color.red;
            }

            // Настраиваем кнопки
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(UpgradeSelectedUnit);

            if (_placeButton != null)
                _placeButton.onClick.AddListener(PlaceSelectedUnit);

            if (_ultimateButton != null)
                _ultimateButton.onClick.AddListener(ActivateUltimate);

            // Скрываем панель информации
            if (_unitInfoPanel != null)
                _unitInfoPanel.SetActive(false);
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null) return;

            _playerInventory = _playerTeam == Team.Blue ?
                GameManager.Instance.Player1Inventory :
                GameManager.Instance.Player2Inventory;

            // Обновляем золото
            if (_goldText != null)
            {
                _goldText.text = $"Золото: {_playerInventory.Gold}";
            }

            // Получаем юниты
            var units = _playerInventory.GetAllUnits();

            // Обновляем слоты
            for (int i = 0; i < _unitSlots.Length; i++)
            {
                if (i < units.Count)
                {
                    _unitSlots[i].SetUnit(units[i], this);
                }
                else
                {
                    _unitSlots[i].ClearSlot();
                }
            }
        }

        public void SelectUnit(Interfaces.ICharacter unit)
        {
            _selectedUnit = unit;

            if (_unitInfoPanel != null)
                _unitInfoPanel.SetActive(true);

            UpdateUnitInfo();
        }

        private void UpdateUnitInfo()
        {
            if (_selectedUnit == null || _unitNameText == null) return;

            _unitNameText.text = $"{_selectedUnit.ToString()}";

            string stats = $"Уровень: {_selectedUnit.Level}\n" +
                          $"Здоровье: {_selectedUnit.Health}\n";

            // Добавляем специфическую информацию
            if (_selectedUnit is Entities.Tank tank)
                stats += $"Урон: {tank.Damage}\nБроня: {tank.Armor}";
            else if (_selectedUnit is Entities.Mage mage)
                stats += $"Урон: {mage.Damage}\nБроня: {mage.Armor}";
            else if (_selectedUnit is Entities.Healer healer)
                stats += $"Лечение: {healer.HealPower}\nБроня: {healer.Armor}";
            else if (_selectedUnit is Entities.Trickster trickster)
                stats += $"Урон: {trickster.Damage}\nУклонение: {trickster.DodgeChance:P0}";

            if (_unitStatsText != null)
                _unitStatsText.text = stats;

            // Обновляем кнопку апгрейда
            bool canUpgrade = _playerInventory.CanUpgradeUnit(_selectedUnit);
            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = canUpgrade;
                _upgradeButton.gameObject.SetActive(canUpgrade);
            }

            if (_upgradeCostText != null && canUpgrade)
            {
                int cost = Economy.EconomyManager.UpgradeCosts[_selectedUnit.Level];
                _upgradeCostText.text = $"{cost} золота";
            }

            // Кнопка размещения
            if (_placeButton != null)
            {
                bool canPlace = GameManager.Instance.CurrentPhase == GamePhase.Preparation;
                _placeButton.interactable = canPlace;
            }

            // Кнопка ультимейта
            if (_ultimateButton != null && _ultimateCostText != null)
            {
                bool canAfford = _playerInventory.Gold >= Economy.EconomyManager.UltimateCost;
                bool isPreparation = GameManager.Instance.CurrentPhase == GamePhase.Preparation;
                bool notActivated = !(_selectedUnit is Entities.Tank tankUnit && tankUnit.IsUltimateActive) &&
                                   !(_selectedUnit is Entities.Mage mageUnit && mageUnit.IsUltimateActive) &&
                                   !(_selectedUnit is Entities.Healer healerUnit && healerUnit.IsUltimateActive) &&
                                   !(_selectedUnit is Entities.Trickster tricksterUnit && tricksterUnit.IsUltimateActive);

                _ultimateButton.interactable = canAfford && isPreparation && notActivated;
                _ultimateCostText.text = notActivated ?
                    $"{Economy.EconomyManager.UltimateCost} золота" : "Ультимейт активен";
            }
        }

        private void UpgradeSelectedUnit()
        {
            if (_selectedUnit == null || _playerInventory == null) return;

            bool success = _playerInventory.TryUpgradeUnit(_selectedUnit);
            if (success)
            {
                Debug.Log($"Юнит {_selectedUnit.GetType().Name} апгрейджен до уровня {_selectedUnit.Level}");
                UpdateUnitInfo();
                UpdateInventoryDisplay();
            }
            else
            {
                Debug.LogWarning("Не удалось апгрейдить юнит");
            }
        }

        private void ActivateUltimate()
        {
            if (_selectedUnit == null || _playerInventory == null) return;

            // Проверяем стоимость
            int cost = Economy.EconomyManager.UltimateCost;
            if (_playerInventory.Gold < cost)
            {
                Debug.LogWarning($"Недостаточно золота для ультимейта! Нужно {cost}, есть {_playerInventory.Gold}");
                return;
            }

            // Активируем ультимейт на юните
            if (_selectedUnit is Interfaces.IUltimate ultimate)
            {
                // Списываем золото
                _playerInventory.RemoveAmount(cost);

                // Активируем ультимейт
                if (_selectedUnit is Interfaces.IUltimate ultimateUnit)
                {
                    ultimateUnit.UseUltimate();
                }

                Debug.Log($"Ультимейт {_selectedUnit.GetType().Name} активирован за {cost} золота!");
                UpdateUnitInfo();
                UpdateInventoryDisplay();
            }
        }

        private void PlaceSelectedUnit()
        {
            if (_selectedUnit == null || GameManager.Instance == null) return;

            // Находим свободную клетку для размещения
            var freeCells = GameManager.Instance.GameBoard.GetFreeCells(_playerTeam);
            if (freeCells.Any())
            {
                var cell = freeCells.First();
                bool success = GameManager.Instance.PlaceUnitOnBoard(_selectedUnit, cell.X, cell.Y);

                if (success)
                {
                    Debug.Log($"Юнит {_selectedUnit.GetType().Name} размещен на поле ({cell.X}, {cell.Y})");
                    UpdateInventoryDisplay();

                    // Скрываем панель информации
                    if (_unitInfoPanel != null)
                        _unitInfoPanel.SetActive(false);

                    _selectedUnit = null;
                }
            }
            else
            {
                Debug.LogWarning("Нет свободных клеток для размещения юнита");
            }
        }
    }
}
