using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameAssets.Interfaces;

namespace GameAssets.UI
{
    /// <summary>
    /// Слот для отображения юнита в инвентаре
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Button _slotButton;
        [SerializeField] private Image _unitIcon;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _unitLevelText;
        [SerializeField] private Image _backgroundImage;

        private ICharacter _unit;
        private InventoryUI _inventoryUI;

        private void Start()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        public void SetUnit(ICharacter unit, InventoryUI inventoryUI)
        {
            _unit = unit;
            _inventoryUI = inventoryUI;

            UpdateDisplay();
            gameObject.SetActive(true);
        }

        public void ClearSlot()
        {
            _unit = null;
            _inventoryUI = null;

            if (_unitNameText != null) _unitNameText.text = "";
            if (_unitLevelText != null) _unitLevelText.text = "";
            if (_unitIcon != null) _unitIcon.sprite = null;
            if (_backgroundImage != null) _backgroundImage.color = Color.gray;

            gameObject.SetActive(false);
        }

        private void UpdateDisplay()
        {
            if (_unit == null) return;

            // Имя юнита
            if (_unitNameText != null)
            {
                string unitType = GetUnitTypeName(_unit);
                _unitNameText.text = unitType;
            }

            // Уровень
            if (_unitLevelText != null)
            {
                _unitLevelText.text = $"Lv.{_unit.Level}";
            }

            // Цвет фона в зависимости от типа юнита
            if (_backgroundImage != null)
            {
                _backgroundImage.color = GetUnitTypeColor(_unit);
            }

            // Иконка (пока просто цвет)
            if (_unitIcon != null)
            {
                // Можно добавить спрайты для разных юнитов
                _unitIcon.color = GetUnitTypeColor(_unit);
            }
        }

        private string GetUnitTypeName(ICharacter unit)
        {
            if (unit is Entities.Tank) return "Tank";
            if (unit is Entities.Mage) return "Mage";
            if (unit is Entities.Healer) return "Healer";
            if (unit is Entities.Trickster) return "Trickster";
            return "Unknown";
        }

        private Color GetUnitTypeColor(ICharacter unit)
        {
            if (unit is Entities.Tank) return new Color(0.8f, 0.6f, 0.4f); // Бежевый
            if (unit is Entities.Mage) return new Color(0.6f, 0.8f, 1f);   // Голубой
            if (unit is Entities.Healer) return new Color(0.6f, 1f, 0.6f); // Зеленый
            if (unit is Entities.Trickster) return new Color(0.8f, 0.4f, 0.8f); // Фиолетовый
            return Color.gray;
        }

        private void OnSlotClicked()
        {
            if (_unit != null && _inventoryUI != null)
            {
                _inventoryUI.SelectUnit(_unit);
            }
        }
    }
}
