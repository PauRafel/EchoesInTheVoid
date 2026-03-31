using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase3InitPanel : MonoBehaviour
{
    public static Phase3InitPanel Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public TextMeshProUGUI costText;
    public Button btnMejorar;

    private double cost = 500000;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        btnMejorar.onClick.AddListener(OnClickMejorar);
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        bool canAfford = GameManager.Instance.scanData >= cost;
        btnMejorar.interactable = canAfford;

        if (costText != null)
            costText.text = "COSTE: " + FormatCost(cost);

        TextMeshProUGUI btnText =
            btnMejorar.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
            btnText.color = canAfford
                ? new Color(0f, 1f, 0.27f, 1f)
                : new Color(0.4f, 0.4f, 0.4f, 1f);
    }

    void OnClickMejorar()
    {
        if (GameManager.Instance.scanData < cost) return;

        GameManager.Instance.SpendData(cost);
        panel.SetActive(false);

        ApplyPhase3Changes();
        UpgradePanel.Instance.Show();
    }

    void ApplyPhase3Changes()
    {
        GameManager.Instance.SetPhase(GamePhase.Phase3);
        UpgradeManager.Instance.UnlockPhase3Branches();

        RadarController.Instance.SetRingCount(3);

        SignalAnalyzer.Instance.SetCursorRadius(0.3f);

        SignalManager.Instance.SetPhase3SignalScale();

        GameManager.Instance.dataMultiplier *= 5.0;

        if (PhaseTransitionManager.Instance != null)
            PhaseTransitionManager.Instance.RestorePanelColors();

        Debug.Log("Fase 3 iniciada");
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }
}