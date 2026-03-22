using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeNodeUI : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;

    private UpgradeData upgrade;
    private UpgradePanel panel;
    private Button button;
    private Image bgImage;

    private static readonly Color ColBought = new Color(0f, 0.10f, 0.02f, 1f);
    private static readonly Color ColAvailable = new Color(0f, 0.08f, 0f, 1f);
    private static readonly Color ColNoFunds = new Color(0f, 0.04f, 0f, 1f);
    private static readonly Color ColLocked = new Color(0.04f, 0.04f, 0.04f, 1f);

    void Awake()
    {
        button = GetComponent<Button>();
        bgImage = GetComponent<Image>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(UpgradeData upgradeData, UpgradePanel upgradePanel)
    {
        upgrade = upgradeData;
        panel = upgradePanel;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (upgrade == null) return;

        bool bought = upgrade.comprada;
        bool available = !bought &&
                         upgrade.IsAvailable(UpgradeManager.Instance);
        bool canAfford = available && upgrade.CanAfford();

        if (nameText != null)
            nameText.text = upgrade.nombre;

        if (costText != null)
            costText.text = bought ? "OK" : FormatCost(upgrade.coste);

        if (bought)
        {
            SetColors(ColBought,
                new Color(0f, 1f, 0.3f, 1f),
                new Color(0f, 0.6f, 0.2f, 1f));
            button.interactable = true;
        }
        else if (available && canAfford)
        {
            SetColors(ColAvailable,
                new Color(0f, 1f, 0.27f, 1f),
                new Color(0f, 0.8f, 0.2f, 1f));
            button.interactable = true;
        }
        else if (available)
        {
            SetColors(ColNoFunds,
                new Color(0.3f, 0.5f, 0.3f, 1f),
                new Color(0.3f, 0.4f, 0.3f, 1f));
            button.interactable = true;
        }
        else
        {
            SetColors(ColLocked,
                new Color(0.15f, 0.2f, 0.15f, 1f),
                new Color(0.12f, 0.15f, 0.12f, 1f));
            button.interactable = false;
        }
    }

    void SetColors(Color bg, Color nameCol, Color costCol)
    {
        if (bgImage != null) bgImage.color = bg;
        if (nameText != null) nameText.color = nameCol;
        if (costText != null) costText.color = costCol;
    }

    void OnClick()
    {
        if (panel != null)
            panel.OnNodeSelected(upgrade);
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }

    public UpgradeData GetUpgrade() => upgrade;
}