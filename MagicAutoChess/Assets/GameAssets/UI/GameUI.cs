using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameAssets.Game;

namespace GameAssets.UI
{
    /// <summary>
    /// Основной UI для игры Auto Chess
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _roundText;
        [SerializeField] private TextMeshProUGUI _phaseText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Player Controls")]
        [SerializeField] private Button _player1ReadyButton;
        [SerializeField] private Button _player2ReadyButton;

        [Header("Save/Load System")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private TextMeshProUGUI _saveStatusText;

        private void Start()
        {
            SetupButtons();
            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (_player1ReadyButton != null)
                _player1ReadyButton.onClick.AddListener(() => OnPlayerReady(Team.Blue));

            if (_player2ReadyButton != null)
                _player2ReadyButton.onClick.AddListener(() => OnPlayerReady(Team.Red));

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveGame);

            if (_loadButton != null)
                _loadButton.onClick.AddListener(OnLoadGame);
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            // Раунд и фаза
            if (_roundText != null)
                _roundText.text = $"Раунд {GameManager.Instance.RoundNumber}";

            if (_phaseText != null)
                _phaseText.text = GameManager.Instance.CurrentPhase == GamePhase.Preparation ?
                    "Подготовка" : "Бой";

            // Статус
            if (_statusText != null)
            {
                if (GameManager.Instance.CurrentPhase == GamePhase.GameOver)
                    _statusText.text = "Игра окончена!";
                else if (GameManager.Instance.CurrentPhase == GamePhase.Preparation)
                    _statusText.text = "Разместите юнитов и нажмите Готов";
                else
                    _statusText.text = "Бой в процессе...";
            }

            // Кнопки Ready
            bool isPreparation = GameManager.Instance.CurrentPhase == GamePhase.Preparation;
            if (_player1ReadyButton != null) _player1ReadyButton.interactable = isPreparation;
            if (_player2ReadyButton != null) _player2ReadyButton.interactable = isPreparation;

            // Кнопки Save/Load
            if (_saveButton != null) _saveButton.interactable = true; // Сохранять можно всегда
            if (_loadButton != null) _loadButton.interactable = GameManager.Instance.HasSaveGame();
        }

        private void OnPlayerReady(Team team)
        {
            if (GameManager.Instance == null) return;

            int playerId = team == Team.Blue ? 1 : 2;
            GameManager.Instance.PlayerReady(playerId);
        }

        private void OnSaveGame()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.SaveGame();
            ShowSaveStatus("Игра сохранена!");
        }

        private void OnLoadGame()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.HasSaveGame())
            {
                if (GameManager.Instance.LoadGame())
                {
                    ShowSaveStatus("Игра загружена!");
                }
                else
                {
                    ShowSaveStatus("Ошибка загрузки!");
                }
            }
            else
            {
                ShowSaveStatus("Сохранение не найдено!");
            }
        }

        private void ShowSaveStatus(string message)
        {
            if (_saveStatusText != null)
            {
                _saveStatusText.text = message;
                CancelInvoke(nameof(ClearSaveStatus));
                Invoke(nameof(ClearSaveStatus), 3f);
            }
        }

        private void ClearSaveStatus()
        {
            if (_saveStatusText != null)
            {
                _saveStatusText.text = "";
            }
        }
    }
}