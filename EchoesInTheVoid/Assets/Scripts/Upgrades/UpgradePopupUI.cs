using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePopupUI : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI popupName;
    public TextMeshProUGUI popupEffect;
    public TextMeshProUGUI popupTotal;
    public TextMeshProUGUI popupCost;
    public Button btnComprar;

    private UpgradeData upgrade;
    private UpgradePanel panel;

    void Awake()
    {
        if (btnComprar != null)
            btnComprar.onClick.AddListener(OnClickComprar);

        gameObject.SetActive(false);
    }

    public void Show(UpgradeData upgradeData, UpgradePanel upgradePanel,
                     Vector2 position)
    {
        upgrade = upgradeData;
        panel = upgradePanel;

        gameObject.SetActive(true);

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = position;

        UpdateContent();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        upgrade = null;
    }

    void UpdateContent()
    {
        if (upgrade == null) return;

        if (popupName != null)
            popupName.text = upgrade.nombre.ToUpper();

        if (popupEffect != null)
            popupEffect.text = upgrade.descripcion;

        string totalText = GetTotalText(upgrade);
        if (popupTotal != null)
        {
            popupTotal.text = totalText;
            popupTotal.gameObject.SetActive(totalText != "");
        }

        if (popupCost != null)
            popupCost.text = "Coste: " + FormatCost(upgrade.coste);

        bool canAfford = upgrade.CanAfford();
        bool available = upgrade.IsAvailable(UpgradeManager.Instance);

        if (btnComprar != null)
        {
            btnComprar.gameObject.SetActive(!upgrade.comprada);
            btnComprar.interactable = canAfford && available;

            TextMeshProUGUI btnText =
                btnComprar.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = canAfford && available
                    ? "COMPRAR" : "SIN FONDOS";
                btnText.color = canAfford && available
                    ? new Color(0f, 1f, 0.27f, 1f)
                    : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
    }

    string GetTotalText(UpgradeData u)
    {
        if (GameManager.Instance == null) return "";

        switch (u.id)
        {
            case "tiempo_1":
            case "tiempo_2":
                return "Total ronda: " +
                    GameManager.Instance.roundDuration.ToString("F1") + "s";

            case "tiempo_4":
            case "tiempo_5":
                return "Total prob: " +
                    (GameManager.Instance.bonusTimeOnAnalysisChance * 100f)
                    .ToString("F0") + "%";

            case "analisis_1":
            case "analisis_2":
            case "analisis_3":
            case "analisis_6":
            case "analisis_7":
            case "analisis_10":
            case "analisis_11":
            case "analisis_12":
                return "Total velocidad: " +
                    GameManager.Instance.analysisSpeedPercent
                    .ToString("F0") + "%";

            case "analisis_4":
            case "analisis_5":
            case "analisis_8":
                return "Total critico: " +
                    (GameManager.Instance.criticalChance * 100f)
                    .ToString("F0") + "%";

            case "analisis_9":
                return "Total bonus critico: +" +
                    GameManager.Instance.criticalBonus.ToString("F0") + "%";

            case "cursor_1":
            case "cursor_2":
            case "cursor_3":
            case "cursor_4":
            case "cursor_5":
            case "cursor_6":
            case "cursor_7":
            case "cursor_8":
            case "cursor_9":
            case "cursor_10":
            case "cursor_11":
                float cursorPercent = SignalAnalyzer.Instance != null
                    ? (SignalAnalyzer.Instance.cursorRadius / 0.3f) * 100f
                    : 100f;
                return "Total cursor: " +
                    cursorPercent.ToString("F0") + "%";

            case "sweep_1":
            case "sweep_2":
            case "sweep_3":
            case "sweep_4":
            case "sweep_5":
                float sweepPercent = RadarController.Instance != null
                    ? (RadarController.Instance.sweepSpeed / 45f) * 100f
                    : 100f;
                return "Total sweep: " +
                    sweepPercent.ToString("F0") + "%";

            default:
                return "";
        }
    }

    void OnClickComprar()
    {
        if (upgrade == null || panel == null) return;
        panel.OnBuyFromPopup(upgrade);
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }
}