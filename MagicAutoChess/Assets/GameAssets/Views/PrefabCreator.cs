using UnityEngine;

namespace GameAssets.Views
{
    /// <summary>
    /// Создает базовые префабы для тестирования
    /// </summary>
    public class PrefabCreator : MonoBehaviour
    {
        private Sprite CreateSquareSprite(Color color)
        {
            // Создаем простую квадратную текстуру
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // Создаем спрайт
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        [ContextMenu("Create Basic Prefabs")]
        private void CreateBasicPrefabs()
        {
            CreateCellPrefab();
            CreateUnitPrefabs();
        }

        private void CreateCellPrefab()
        {
            GameObject cellPrefab = new GameObject("CellPrefab");

            SpriteRenderer spriteRenderer = cellPrefab.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite(Color.gray);
            spriteRenderer.sortingOrder = 0;

            // Настраиваем размер
            cellPrefab.transform.localScale = new Vector3(1.8f, 1.8f, 1);

            // Добавляем компоненты
            cellPrefab.AddComponent<BoxCollider2D>();
            cellPrefab.AddComponent<CellView>();

            // Сохраняем как префаб
            #if UNITY_EDITOR
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cellPrefab, "Assets/Prefabs/CellPrefab.prefab");
            #endif

            Destroy(cellPrefab);
        }

        private void CreateUnitPrefabs()
        {
            CreateUnitPrefab("Tank", new Color(0.8f, 0.6f, 0.4f)); // Бежевый
            CreateUnitPrefab("Mage", new Color(0.6f, 0.8f, 1f));   // Голубой
            CreateUnitPrefab("Healer", new Color(0.6f, 1f, 0.6f)); // Светло-зеленый
            CreateUnitPrefab("Trickster", new Color(0.8f, 0.4f, 0.8f)); // Фиолетовый
        }

        private void CreateUnitPrefab(string unitName, Color color)
        {
            GameObject unitPrefab = new GameObject(unitName + "Prefab");

            SpriteRenderer spriteRenderer = unitPrefab.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite(color);
            spriteRenderer.sortingOrder = 1; // Над клетками

            // Настраиваем размер
            unitPrefab.transform.localScale = new Vector3(0.8f, 1.2f, 1);

            // Добавляем компоненты
            unitPrefab.AddComponent<BoxCollider2D>();
            unitPrefab.AddComponent<GameAssets.Views.UnitView>();

            // Сохраняем как префаб
            #if UNITY_EDITOR
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(unitPrefab, $"Assets/Prefabs/{unitName}Prefab.prefab");
            #endif

            Destroy(unitPrefab);
        }
    }
}
