using UnityEngine;
using GameAssets.Field;
using GameAssets;
using GameAssets.Game;
using GameAssets.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace GameAssets.Views
{
    public class BoardVisualizer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _tankPrefab;
        [SerializeField] private GameObject _magePrefab;
        [SerializeField] private GameObject _healerPrefab;
        [SerializeField] private GameObject _tricksterPrefab;

        [Header("Team Areas")]
        [SerializeField] private Transform _blueTeamArea;
        [SerializeField] private Transform _redTeamArea;

        private const int BOARD_WIDTH = 5;
        private const int BOARD_HEIGHT = 1;
        private Dictionary<(int, int, Team), CellView> _cellViews = new();
        private Dictionary<System.Guid, UnitView> _unitViews = new();

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                CreateBoardVisual();
            }
        }

        private void CreateBoardVisual()
        {
            // Создаем визуальные клетки для обеих команд
            CreateTeamBoard(Team.Blue, _blueTeamArea);
            CreateTeamBoard(Team.Red, _redTeamArea);
        }

        private void CreateTeamBoard(Team team, Transform parent)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
            // Создаем клетку для 2D
            GameObject cellObj = Instantiate(_cellPrefab, parent);
            cellObj.transform.localPosition = new Vector3(x * 2, y * 2, 0);

                    CellView cellView = cellObj.GetComponent<CellView>();
                    Cell cell = GameManager.Instance.GameBoard.GetCell(x, y, team);
                    cellView.Initialize(x, y, team, cell);

                    _cellViews[(x, y, team)] = cellView;
                }
            }
        }

        public void PlaceUnitVisual(ICharacter character, int x, int y)
        {
            Team team = character.Team;
            Transform parentArea = team == Team.Blue ? _blueTeamArea : _redTeamArea;

            // Получаем правильный префаб
            GameObject unitPrefab = GetUnitPrefab(character);

            // Создаем юнит для 2D
            GameObject unitObj = Instantiate(unitPrefab, parentArea);
            unitObj.transform.localPosition = new Vector3(x * 2, y * 2, -1); // Немного позади клеток

            // Настраиваем компонент
            var unitView = unitObj.GetComponent<GameAssets.Views.UnitView>();
            if (unitView == null)
            {
                unitView = unitObj.AddComponent<GameAssets.Views.UnitView>();
            }
            unitView.Initialize(character);

            // Добавляем простой текст здоровья
            AddHealthText(unitObj, character);

            _unitViews[character.ID] = unitView;
        }

        public void RemoveUnitVisual(ICharacter character)
        {
            if (_unitViews.TryGetValue(character.ID, out UnitView unitView))
            {
                if (unitView != null && unitView.gameObject != null)
                {
                    Destroy(unitView.gameObject);
                }
                _unitViews.Remove(character.ID);
            }
        }

        private GameObject GetUnitPrefab(ICharacter character)
        {
            if (character is Entities.Tank) return _tankPrefab != null ? _tankPrefab : CreateBasicUnitPrefab("Tank", Color.red);
            if (character is Entities.Mage) return _magePrefab != null ? _magePrefab : CreateBasicUnitPrefab("Mage", Color.blue);
            if (character is Entities.Healer) return _healerPrefab != null ? _healerPrefab : CreateBasicUnitPrefab("Healer", Color.green);
            if (character is Entities.Trickster) return _tricksterPrefab != null ? _tricksterPrefab : CreateBasicUnitPrefab("Trickster", Color.magenta);

            return CreateBasicUnitPrefab("Unit", Color.white); // fallback
        }

        private GameObject CreateBasicUnitPrefab(string name, Color color)
        {
            // Создаем базовый префаб из куба
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = name + "Prefab";
            prefab.GetComponent<Renderer>().material.color = color;
            prefab.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

            // Добавляем компоненты
            prefab.AddComponent<GameAssets.Views.UnitView>();

            return prefab;
        }

        public void HighlightCell(int x, int y, Team team, bool highlight)
        {
            if (_cellViews.TryGetValue((x, y, team), out CellView cellView))
            {
                cellView.Highlight(highlight);
            }
        }

        public void ClearBoardVisual()
        {
            // Уничтожаем все юниты
            foreach (var unitView in _unitViews.Values)
            {
                if (unitView != null && unitView.gameObject != null)
                {
                    Destroy(unitView.gameObject);
                }
            }
            _unitViews.Clear();

            // Сбрасываем подсветку клеток
            foreach (var cellView in _cellViews.Values)
            {
                cellView.Highlight(false);
            }
        }

        public void UpdateBoardVisual()
        {
            // Обновляем визуальное состояние поля
            ClearBoardVisual();

            // Перерисовываем всех юнитов на поле
            if (GameManager.Instance != null)
            {
                var blueUnits = GameManager.Instance.GameBoard.GetFieldUnits(Team.Blue);
                var redUnits = GameManager.Instance.GameBoard.GetFieldUnits(Team.Red);

                foreach (var unit in blueUnits.Concat(redUnits))
                {
                    var position = GameManager.Instance.GameBoard.GetUnitPosition(unit);
                    if (position.HasValue)
                    {
                        PlaceUnitVisual(unit, position.Value.x, position.Value.y);
                    }
                }
            }
        }

        private void AddHealthText(GameObject unitObj, Interfaces.ICharacter character)
        {
            // Создаем текст здоровья для 2D
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(unitObj.transform);
            textObj.transform.localPosition = new Vector3(0, 1.5f, -2); // Над юнитом

            // Используем TextMeshPro для 2D
            var textMeshPro = textObj.AddComponent<TMPro.TextMeshPro>();
            textMeshPro.text = $"{Mathf.CeilToInt((float)character.Health)} HP";
            textMeshPro.fontSize = 8;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TMPro.TextAlignmentOptions.Center;
            textMeshPro.sortingOrder = 10; // Над всем

            // Добавляем компонент для обновления текста
            textObj.AddComponent<GameAssets.Views.HealthTextBillboard>();
        }
    }
}
