using UnityEngine;
using GameAssets.Interfaces;

namespace GameAssets.Views
{
    /// <summary>
    /// Обновляет текст здоровья для 2D
    /// </summary>
    public class HealthTextBillboard : MonoBehaviour
    {
        private ICharacter _character;
        private TMPro.TextMeshPro _textMeshPro;

        private void Start()
        {
            _textMeshPro = GetComponent<TMPro.TextMeshPro>();

            // Ищем персонажа через родительский объект
            var unitView = transform.parent?.GetComponent<GameAssets.Views.UnitView>();
            if (unitView != null)
            {
                _character = unitView.Character;
            }
        }

        private void Update()
        {
            // Обновляем текст здоровья
            if (_character != null && _textMeshPro != null)
            {
                _textMeshPro.text = $"{Mathf.CeilToInt((float)_character.Health)} HP";
            }
        }
    }
}
