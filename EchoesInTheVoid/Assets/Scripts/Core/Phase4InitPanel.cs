using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase4InitPanel : MonoBehaviour
{
    public static Phase4InitPanel Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public TextMeshProUGUI costText;
    public Button btnMejorar;

    private double cost = 5000000;

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

        ApplyPhase4Changes();

        // En fase 4 no hay panel de mejoras
        // Continuar directamente
        UIManager.Instance.SetHUDVisible(true);
        UIManager.Instance.ResetRoundData();
        SignalManager.Instance.ClearAllSignals();
        GameManager.Instance.StartRound();
    }

    void ApplyPhase4Changes()
    {
        GameManager.Instance.SetPhase(GamePhase.Phase4);

        RadarController.Instance.SetRingCount(4);

        SignalAnalyzer.Instance.SetCursorRadius(0.3f);

        SignalManager.Instance.SetPhase2SignalScale(0.5f);

        GameManager.Instance.dataMultiplier *= 8.0;

        if (PhaseTransitionManager.Instance != null)
            PhaseTransitionManager.Instance.RestorePanelColors();

        if (Phase4Manager.Instance != null)
            Phase4Manager.Instance.ActivatePhase4();

        Debug.Log("Fase 4 iniciada");
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }
}