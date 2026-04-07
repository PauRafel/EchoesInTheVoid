using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeNodeUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual")]
    public Image nodeImage;
    public Image iconImage;

    [Header("Sprites por estado")]
    public Sprite spriteNormal;

    private UpgradeData upgrade;
    private UpgradePanel panel;

    private static readonly Color ColBought = new Color(0f, 1f, 0.3f, 1f);
    private static readonly Color ColAvailable = new Color(0f, 0.6f, 0.2f, 1f);
    private static readonly Color ColNoFunds = new Color(0f, 0.25f, 0.08f, 1f);
    private static readonly Color ColLocked = new Color(0.2f, 0.2f, 0.2f, 1f);

    public void SetUpgradeData(UpgradeData data)
    {
        upgrade = data;
        panel = null;
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

        Color nodeColor;

        if (bought)
            nodeColor = ColBought;
        else if (available && canAfford)
            nodeColor = ColAvailable;
        else if (available)
            nodeColor = ColNoFunds;
        else
            nodeColor = ColLocked;

        if (nodeImage != null)
            nodeImage.color = nodeColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (upgrade == null || panel == null) return;

        bool available = upgrade.IsAvailable(UpgradeManager.Instance);
        if (!upgrade.comprada && !available) return;

        panel.OnNodeSelected(upgrade, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nodeImage == null) return;
        Color c = nodeImage.color;
        c.a = 0.7f;
        nodeImage.color = c;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisual();
    }

    public UpgradeData GetUpgrade() => upgrade;
}