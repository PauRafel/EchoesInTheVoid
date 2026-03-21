using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeNodeUI : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI iconText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;

    private UpgradeData upgrade;
    private UpgradePanel panel;
    private Button button;
    private Image bgImage;

    // Colores de fondo
    private static readonly Color ColBought = new Color(0f, 0.10f, 0.02f, 1f);
    private static readonly Color ColAvailable = new Color(0f, 0.08f, 0f, 1f);
    private static readonly Color ColNoFunds = new Color(0f, 0.05f, 0f, 1f);
    private static readonly Color ColLocked = new Color(0.04f, 0.04f, 0.04f, 1f);

    // Colores de borde
    private static readonly Color BorderBought = new Color(0f, 1f, 0.3f, 1f);
    private static readonly Color BorderAvailable = new Color(0f, 0.8f, 0.2f, 0.8f);
    private static readonly Color BorderNoFunds = new Color(0f, 0.3f, 0.1f, 0.6f);
    private static readonly Color BorderLocked = new Color(0.1f, 0.1f, 0.1f, 1f);

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
        bool available = !bought && upgrade.IsAvailable(UpgradeManager.Instance);
        bool canAfford = available && upgrade.CanAfford();

        // Icono
        if (iconText != null)
            iconText.text = GetIconForBranch(upgrade.rama);

        // Nombre
        if (nameText != null)
            nameText.text = upgrade.nombre;

        // Coste
        if (costText != null)
            costText.text = bought ? "v" : FormatCost(upgrade.coste);

        // Colores y estado del botón
        if (bought)
        {
            ApplyColors(ColBought, BorderBought,
                new Color(0f, 1f, 0.3f, 1f),
                new Color(0f, 0.8f, 0.2f, 1f),
                new Color(0f, 0.6f, 0.2f, 1f));
            button.interactable = true;
        }
        else if (available && canAfford)
        {
            ApplyColors(ColAvailable, BorderAvailable,
                new Color(0f, 1f, 0.27f, 1f),
                new Color(0f, 0.9f, 0.2f, 1f),
                new Color(0f, 0.8f, 0.2f, 1f));
            button.interactable = true;
        }
        else if (available)
        {
            ApplyColors(ColNoFunds, BorderNoFunds,
                new Color(0.2f, 0.5f, 0.2f, 1f),
                new Color(0.2f, 0.45f, 0.2f, 1f),
                new Color(0.2f, 0.4f, 0.2f, 1f));
            button.interactable = true;
        }
        else
        {
            ApplyColors(ColLocked, BorderLocked,
                new Color(0.15f, 0.2f, 0.15f, 1f),
                new Color(0.12f, 0.16f, 0.12f, 1f),
                new Color(0.1f, 0.12f, 0.1f, 1f));
            button.interactable = false;
        }
    }

    void ApplyColors(Color bg, Color border,
                     Color icon, Color name, Color cost)
    {
        if (bgImage != null) bgImage.color = bg;

        Outline outline = GetComponent<Outline>();
        if (outline != null) outline.effectColor = border;

        if (iconText != null) iconText.color = icon;
        if (nameText != null) nameText.color = name;
        if (costText != null) costText.color = cost;
    }

    void OnClick()
    {
        if (panel != null)
            panel.OnNodeSelected(upgrade);
    }

    string GetIconForBranch(UpgradeBranch branch)
    {
        switch (branch)
        {
            case UpgradeBranch.Sweep: return "S";
            case UpgradeBranch.Tiempo: return "T";
            case UpgradeBranch.CantidadSenales: return "CS";
            case UpgradeBranch.TamanoSenales: return "TS";
            case UpgradeBranch.Cursor: return "C";
            case UpgradeBranch.VelocidadAnalisis: return "V";
            default: return "?";
        }
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M *";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K *";
        return value.ToString("F0") + " *";
    }

    public UpgradeData GetUpgrade() => upgrade;
}