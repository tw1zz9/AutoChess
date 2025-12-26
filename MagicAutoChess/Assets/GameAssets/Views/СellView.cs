using GameAssets.Field;
using UnityEngine;

namespace GameAssets.Views
{
    [RequireComponent(typeof(Collider))]
    public class CellView : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Team Team { get; private set; }

        private Cell _cell;

        public void Initialize(int x, int y, Team team, Cell cell)
        {
            X = x;
            Y = y;
            Team = team;
            _cell = cell;
            gameObject.name = $"Cell_{x}_{y}_{team}";
        }

        private void OnMouseDown()
        {
            if (GameAssets.Game.GameManager.Instance.CurrentPhase ==
                GameAssets.Game.GamePhase.Preparation)
            {
                GameAssets.Game.GameManager.Instance.CellSelected(X, Y);
            }
        }

        public void Highlight(bool value)
        {
            if (GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().material.color = value ? Color.green : Color.gray;
            }
        }
    }
}