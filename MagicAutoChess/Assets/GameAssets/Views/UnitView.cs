using GameAssets.Interfaces;
using GameAssets.Entities;
using UnityEngine;

namespace GameAssets.Views
{
    [RequireComponent(typeof(Collider))]
    public class UnitView : MonoBehaviour
    {
        public ICharacter Character { get; private set; }
        private bool _isSelected = false;

        public void Initialize(ICharacter character)
        {
            Character = character;
            gameObject.name = character.ToString();
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            // Обновляем визуальное представление юнита
            if (Character != null)
            {
                // Здесь можно добавить логику для отображения уровня, здоровья и т.д.
            }
        }

        private void OnMouseDown()
        {
            // В бою клики по юнитам не обрабатываем
            if (GameAssets.Game.GameManager.Instance.CurrentPhase != GameAssets.Game.GamePhase.Preparation)
                return;

            // Для визуализации просто подсвечиваем/снимаем подсветку
            _isSelected = !_isSelected;
            Highlight(_isSelected);

            // Если есть система выбора юнитов, можно добавить логику
            // GameAssets.Game.GameManager.Instance.UnitSelected(this);
        }

        public void Highlight(bool value)
        {
            // Подсвечиваем юнит
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = value ? Color.yellow : GetOriginalColor();
            }
        }

        private Color GetOriginalColor()
        {
            // Определяем цвет по типу юнита (соответствует PrefabCreator)
            if (Character is Tank) return new Color(0.8f, 0.6f, 0.4f); // Бежевый
            if (Character is Mage) return new Color(0.6f, 0.8f, 1f);   // Голубой
            if (Character is Healer) return new Color(0.6f, 1f, 0.6f); // Светло-зеленый
            if (Character is Trickster) return new Color(0.8f, 0.4f, 0.8f); // Фиолетовый
            return Color.white;
        }
    }
}
