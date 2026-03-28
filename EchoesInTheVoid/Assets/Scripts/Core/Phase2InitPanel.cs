using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase2InitPanel : MonoBehaviour
{
    public static Phase2InitPanel Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public TextMeshProUGUI costText;
    public Button btnMejorar;

    private double cost = 20000;

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

        ApplyPhase2Changes();
        UpgradePanel.Instance.Show();
    }

    void ApplyPhase2Changes()
    {
        GameManager.Instance.SetPhase(GamePhase.Phase2);
        UpgradeManager.Instance.UnlockPhase2Branches();

        RadarController.Instance.SetRingCount(2);

        float phase2CursorRadius = 0.3f * 1.0f;
        SignalAnalyzer.Instance.SetCursorRadius(phase2CursorRadius);

        SignalManager.Instance.SetPhase2SignalScale(0.5f);

        GameManager.Instance.dataMultiplier *= 3.0;

        Debug.Log("Fase 2 iniciada");
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }
}