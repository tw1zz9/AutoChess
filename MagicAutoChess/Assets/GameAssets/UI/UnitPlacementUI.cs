using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameAssets;
using GameAssets.Game;
using GameAssets.Factories;

namespace GameAssets.UI
{
    /// <summary>
    /// UI для размещения юнитов на поле
    /// </summary>
    public class UnitPlacementUI : MonoBehaviour
    {
        [Header("Unit Buttons")]
        [SerializeField] private Button _tankButton;
        [SerializeField] private Button _mageButton;
        [SerializeField] private Button _healerButton;
        [SerializeField] private Button _tricksterButton;

        [Header("Unit Info")]
        [SerializeField] private TextMeshProUGUI _unitInfoText;
        [SerializeField] private TextMeshProUGUI _inventoryCountText;

        [Header("Placement")]
        [SerializeField] private Button _placeButton;
        [SerializeField] private TextMeshProUGUI _placeButtonText;

        private Team _currentPlayer = Team.Blue;
        private System.Type _selectedUnitType;
        private bool _unitSelected = false;

        private void Start()
        {
            SetupButtons();
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (_tankButton != null)
                _tankButton.onClick.AddListener(() => SelectUnit(typeof(Entities.Tank)));

            if (_mageButton != null)
                _mageButton.onClick.AddListener(() => SelectUnit(typeof(Entities.Mage)));

            if (_healerButton != null)
                _healerButton.onClick.AddListener(() => SelectUnit(typeof(Entities.Healer)));

            if (_tricksterButton != null)
                _tricksterButton.onClick.AddListener(() => SelectUnit(typeof(Entities.Trickster)));

            if (_placeButton != null)
                _placeButton.onClick.AddListener(PlaceSelectedUnit);
        }

        private void Update()
        {
            UpdateUI();
        }

        private void SelectUnit(System.Type unitType)
        {
            _selectedUnitType = unitType;
            _unitSelected = true;

            // Показываем информацию о юните
            if (_unitInfoText != null)
            {
                string unitName = unitType.Name;
                string info = GetUnitInfo(unitType);
                _unitInfoText.text = $"{unitName}\n{info}";
            }

            UpdateUI();
        }

        private string GetUnitInfo(System.Type unitType)
        {
            if (unitType == typeof(Entities.Tank))
                return "Здоровье: 1000\nУрон: 100\nБроня: 15\nУльтимейт: Taunt";
            if (unitType == typeof(Entities.Mage))
                return "Здоровье: 500\nУрон: 150\nБроня: 3\nУльтимейт: Arcane Surge";
            if (unitType == typeof(Entities.Healer))
                return "Здоровье: 800\nЛечение: 100\nБроня: 8\nУльтимейт: Divine Light";
            if (unitType == typeof(Entities.Trickster))
                return "Здоровье: 650\nУрон: 200\nБроня: 5\nУклонение: 25%\nУльтимейт: Shadow Step";

            return "Неизвестный юнит";
        }

        private void PlaceSelectedUnit()
        {
            if (!_unitSelected || GameManager.Instance == null) return;

            // Создаем юнит в инвентаре текущего игрока
            Interfaces.ICharacter newUnit = CreateUnit(_selectedUnitType);

            if (newUnit != null)
            {
                var inventory = _currentPlayer == Team.Blue ?
                    GameManager.Instance.Player1Inventory :
                    GameManager.Instance.Player2Inventory;

                inventory.AddUnits(new[] { newUnit });
                Debug.Log($"Юнит {newUnit.GetType().Name} добавлен в инвентарь игрока {_currentPlayer}");

                _unitSelected = false;
                UpdateUI();
            }
        }

        private Interfaces.ICharacter CreateUnit(System.Type unitType)
        {
            if (unitType == typeof(Entities.Tank))
                return UnitFactory.CreateTank(_currentPlayer);
            if (unitType == typeof(Entities.Mage))
                return UnitFactory.CreateMage(_currentPlayer);
            if (unitType == typeof(Entities.Healer))
                return UnitFactory.CreateHealer(_currentPlayer);
            if (unitType == typeof(Entities.Trickster))
                return UnitFactory.CreateTrickster(_currentPlayer);

            return null;
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            // Обновляем счетчик юнитов в инвентаре
            var inventory = _currentPlayer == Team.Blue ?
                GameManager.Instance.Player1Inventory :
                GameManager.Instance.Player2Inventory;

            var units = inventory.GetAllUnits();
            if (_inventoryCountText != null)
            {
                _inventoryCountText.text = $"Юнитов в инвентаре: {units.Count}";
            }

            // Обновляем кнопку размещения
            if (_placeButton != null)
            {
                _placeButton.interactable = _unitSelected &&
                    GameManager.Instance.CurrentPhase == GamePhase.Preparation;
            }

            if (_placeButtonText != null)
            {
                _placeButtonText.text = _unitSelected ? "Добавить в инвентарь" : "Выберите юнит";
            }

            // Показываем UI только в фазе подготовки
            gameObject.SetActive(GameManager.Instance.CurrentPhase == GamePhase.Preparation);
        }

        /// <summary>
        /// Переключение текущего игрока
        /// </summary>
        public void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == Team.Blue ? Team.Red : Team.Blue;
            UpdateUI();
        }
    }
}
