using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameAssets.Game;
using GameAssets;

namespace GameAssets.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _roundText;
        [SerializeField] private TextMeshProUGUI _phaseText;

        [Header("Player 1 Info")]
        [SerializeField] private TextMeshProUGUI _player1GoldText;
        [SerializeField] private Button _player1ReadyButton;
        [SerializeField] private TextMeshProUGUI _player1ReadyText;

        [Header("Player 2 Info")]
        [SerializeField] private TextMeshProUGUI _player2GoldText;
        [SerializeField] private Button _player2ReadyButton;
        [SerializeField] private TextMeshProUGUI _player2ReadyText;

        [Header("Unit Info Panel")]
        [SerializeField] private GameObject _unitInfoPanel;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _unitStatsText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _ultimateButton;
        [SerializeField] private TextMeshProUGUI _upgradeCostText;
        [SerializeField] private TextMeshProUGUI _ultimateCostText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _player1ReadyButton.onClick.AddListener(() => OnReadyButtonClick(1));
            _player2ReadyButton.onClick.AddListener(() => OnReadyButtonClick(2));
            _upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
            _ultimateButton.onClick.AddListener(OnUltimateButtonClick);

            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            // Обновляем информацию о раунде и фазе
            _roundText.text = $"Round {GameManager.Instance.RoundNumber}";
            _phaseText.text = GameManager.Instance.CurrentPhase.ToString();

            // Обновляем золото игроков
            _player1GoldText.text = $"Gold: {GameManager.Instance.Player1Inventory.Gold}";
            _player2GoldText.text = $"Gold: {GameManager.Instance.Player2Inventory.Gold}";

            // Обновляем статус готовности
            _player1ReadyText.text = GameManager.Instance.CurrentPhase == GamePhase.Preparation ? "Ready" : "Fighting";
            _player2ReadyText.text = GameManager.Instance.CurrentPhase == GamePhase.Preparation ? "Ready" : "Fighting";
        }

        private void OnReadyButtonClick(int playerId)
        {
            if (GameManager.Instance.CurrentPhase == GamePhase.Preparation)
            {
                GameManager.Instance.PlayerReady(playerId);
            }
        }

        private void OnUpgradeButtonClick()
        {
            // TODO: Реализовать выбор юнита для апгрейда
            Debug.Log("Upgrade button clicked - need to implement unit selection");
        }

        private void OnUltimateButtonClick()
        {
            // TODO: Реализовать выбор юнита для ультимейта
            Debug.Log("Ultimate button clicked - need to implement unit selection");
        }

        public void ShowUnitInfo(Interfaces.ICharacter unit)
        {
            if (unit == null)
            {
                _unitInfoPanel.SetActive(false);
                return;
            }

            _unitInfoPanel.SetActive(true);
            _unitNameText.text = unit.ToString();

            string stats = $"Health: {unit.Health}\nArmor: {unit.Armor}\nLevel: {unit.Level}";
            if (unit is Interfaces.IDamager damager)
                stats += $"\nDamage: {damager.Damage}";
            if (unit is Interfaces.IHealer healer)
                stats += $"\nHeal Power: {healer.HealPower}";
            if (unit is Interfaces.IEvading evader)
                stats += $"\nDodge: {evader.DodgeChance:P0}";

            _unitStatsText.text = stats;

            // Обновляем кнопки апгрейда и ультимейта
            var inventory = GameManager.Instance.GetPlayerInventory(unit.Team);
            bool canUpgrade = inventory.CanUpgradeUnit(unit);
            bool canUseUltimate = unit is Interfaces.IUltimate ultimate && Economy.EconomyManager.CanUseUltimate(ultimate, inventory.Gold);

            _upgradeButton.interactable = canUpgrade;
            _ultimateButton.interactable = canUseUltimate;

            _upgradeCostText.text = canUpgrade ? $"Cost: {Economy.EconomyManager.UpgradeCosts[unit.Level]}" : "Max Level";
            _ultimateCostText.text = canUseUltimate ? $"Cost: {Economy.EconomyManager.UltimateCost}" : "Unavailable";
        }
    }
}
