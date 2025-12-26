using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameAssets.Game
{
    [RequireComponent(typeof(Button))]
    public class TurnButton : MonoBehaviour
    {
        public int PlayerId;
        private Button _button;
        private TextMeshProUGUI _buttonText;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _button.onClick.AddListener(OnClick);
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            bool isPreparation = GameManager.Instance.CurrentPhase == GamePhase.Preparation;
            _button.interactable = isPreparation;
            _buttonText.text = isPreparation ? "Ready" : "Fighting...";
        }

        private void OnClick()
        {
            if (GameManager.Instance.CurrentPhase == GamePhase.Preparation)
            {
                GameManager.Instance.PlayerReady(PlayerId);
            }
        }
    }
}
