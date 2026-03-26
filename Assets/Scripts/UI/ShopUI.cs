using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private Button closeButton;

    [Header("Gold")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("HP Upgrade")]
    [SerializeField] private Button hpButton;
    [SerializeField] private TextMeshProUGUI hpButtonText;

    [Header("ATK Upgrade")]
    [SerializeField] private Button atkButton;
    [SerializeField] private TextMeshProUGUI atkButtonText;

    [Header("DEF Upgrade")]
    [SerializeField] private Button defButton;
    [SerializeField] private TextMeshProUGUI defButtonText;

    private readonly Color normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    private readonly Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
        if (upgradeSystem == null)
            upgradeSystem = FindAnyObjectByType<UpgradeSystem>();

        if (shopToggleButton != null)
            shopToggleButton.onClick.AddListener(ToggleShop);
        if (closeButton != null)
            closeButton.onClick.AddListener(() => SetShopVisible(false));
        if (hpButton != null)
            hpButton.onClick.AddListener(() => DoUpgrade("HP"));
        if (atkButton != null)
            atkButton.onClick.AddListener(() => DoUpgrade("ATK"));
        if (defButton != null)
            defButton.onClick.AddListener(() => DoUpgrade("DEF"));

        if (upgradeSystem != null)
            upgradeSystem.OnUpgraded += (_, __) => RefreshUI();

        SetShopVisible(false);
    }

    private void Update()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        RefreshUI();
    }

    public void ToggleShop()
    {
        bool open = shopPanel != null && !shopPanel.activeSelf;
        SetShopVisible(open);
    }

    private void SetShopVisible(bool visible)
    {
        if (shopPanel != null)
            shopPanel.SetActive(visible);
        if (visible)
            RefreshUI();
    }

    private void DoUpgrade(string statType)
    {
        if (upgradeSystem == null || player == null) return;
        upgradeSystem.TryUpgrade(statType, player);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (player == null || upgradeSystem == null) return;

        goldText.text = $"Gold: {player.Gold:F0}";

        UpdateButton(hpButtonText, hpButton, "HP Up", upgradeSystem.hpLevel);
        UpdateButton(atkButtonText, atkButton, "ATK Up", upgradeSystem.atkLevel);
        UpdateButton(defButtonText, defButton, "DEF Up", upgradeSystem.defLevel);
    }

    private void UpdateButton(TextMeshProUGUI text, Button btn, string label, int level)
    {
        int cost = upgradeSystem.GetUpgradeCost(level);
        text.text = $"{label} Lv.{level} | {cost}G";

        bool canAfford = player.Gold >= cost;
        var colors = btn.colors;
        colors.normalColor = canAfford ? normalColor : disabledColor;
        btn.colors = colors;
    }
}
