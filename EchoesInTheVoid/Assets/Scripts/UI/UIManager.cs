using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles")]
    public GameObject leftPanel;
    public GameObject rightPanel;

    [Header("Panel izquierdo")]
    public TextMeshProUGUI valueDatos;
    public TextMeshProUGUI valueSenales;
    public TextMeshProUGUI valueTiempo;
    public Image timerBarFill;

    [Header("Panel derecho")]
    public GameObject rarezaBox;
    public TextMeshProUGUI rarezaText;
    public GameObject transmisionBox;
    public TextMeshProUGUI transmisionText;
    public TextMeshProUGUI valueTier;

    // Datos de ronda
    private double roundData = 0;
    private int roundSignals = 0;
    private SignalTier bestTier = SignalTier.Normal;
    private bool hasRarityThisRound = false;

    // Colores timer
    private readonly Color colorHigh = new Color(0f, 1f, 0.27f, 1f);
    private readonly Color colorMid = new Color(1f, 0.67f, 0f, 1f);
    private readonly Color colorLow = new Color(1f, 0.2f, 0.2f, 1f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (rarezaBox != null) rarezaBox.SetActive(false);
        if (transmisionBox != null) transmisionBox.SetActive(false);
        ResetRoundData();
    }

    void Update()
    {
        UpdateLeftPanel();
        UpdateTierDisplay();
    }

    // Panel izquierdo

    void UpdateLeftPanel()
    {
        if (GameManager.Instance == null) return;

        if (valueDatos != null)
            valueDatos.text = FormatData(roundData);

        if (valueSenales != null)
            valueSenales.text = roundSignals.ToString();

        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (GameManager.Instance == null) return;

        float remaining = GameManager.Instance.roundDuration
                        - GameManager.Instance.roundTimer;
        float ratio = Mathf.Clamp01(remaining
                        / GameManager.Instance.roundDuration);

        if (valueTiempo != null)
            valueTiempo.text = remaining.ToString("F1") + "s";

        if (timerBarFill != null)
        {
            timerBarFill.fillAmount = ratio;
            timerBarFill.color = ratio > 0.5f ? colorHigh
                                    : ratio > 0.25f ? colorMid
                                    : colorLow;
        }
    }

    void UpdateTierDisplay()
    {
        if (GameManager.Instance == null || valueTier == null) return;

        int tier = GameManager.Instance.GetCurrentTier();
        valueTier.text = tier > 0 ? $"TIER {tier}" : "";
    }

    // Panel derecho

    public void RegisterSignalAnalyzed(SignalData signal)
    {
        roundData += signal.dataReward;
        roundSignals += 1;

        UpdateBestRarity(signal.tier);
    }

    void UpdateBestRarity(SignalTier tier)
    {
        int current = TierRank(tier);
        int best = TierRank(bestTier);

        if (!hasRarityThisRound || current > best)
        {
            bestTier = tier;
            hasRarityThisRound = true;

            if (rarezaBox != null) rarezaBox.SetActive(true);
            if (rarezaText != null)
            {
                rarezaText.text = "" + GetTierLabel(tier);
                rarezaText.color = GetTierColor(tier);
            }
        }
    }

    public void ShowTransmision(string texto, int fragmento, int total)
    {
        if (transmisionBox != null) transmisionBox.SetActive(true);
        if (transmisionText != null)
            transmisionText.text =
                $"\"{texto}\"\n fragmento {fragmento}/{total}";
    }

    public void AddRoundTime(float seconds)
    {
        GameManager.Instance.AddRoundTime(seconds);
    }

    // Control de ronda

    public void ResetRoundData()
    {
        roundData = 0;
        roundSignals = 0;
        hasRarityThisRound = false;
        bestTier = SignalTier.Normal;

        if (rarezaBox != null) rarezaBox.SetActive(false);
        if (transmisionBox != null) transmisionBox.SetActive(false);
    }

    public void FlushRoundDataToTotal()
    {
        GameManager.Instance.AddScanData(0); // ya se sumó en tiempo real
    }

    public void SetHUDVisible(bool visible)
    {
        if (leftPanel != null) leftPanel.SetActive(visible);
        if (rightPanel != null) rightPanel.SetActive(visible);
    }

    // Helpers

    int TierRank(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Normal: return 0;
            case SignalTier.Double: return 1;
            case SignalTier.Triple: return 2;
            case SignalTier.Enhanced: return 3;
            default: return 0;
        }
    }

    string GetTierLabel(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Normal: return "RUIDO SIMPLE";
            case SignalTier.Double: return "RUIDO DOBLE";
            case SignalTier.Triple: return "RUIDO TRIPLE";
            case SignalTier.Enhanced: return "SEÑAL ENHANCED";
            default: return "DESCONOCIDA";
        }
    }

    Color GetTierColor(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Normal: return new Color(1f, 1f, 1f, 1f);
            case SignalTier.Double: return new Color(0.85f, 0.85f, 0.85f, 1f);
            case SignalTier.Triple: return new Color(0.65f, 0.65f, 0.65f, 1f);
            case SignalTier.Enhanced: return new Color(1f, 0.85f, 0f, 1f);
            default: return Color.white;
        }
    }

    string FormatData(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F1") + "K";
        return Mathf.FloorToInt((float)value).ToString();
    }

    public double GetRoundData() => roundData;
    public int GetRoundSignals() => roundSignals;
}