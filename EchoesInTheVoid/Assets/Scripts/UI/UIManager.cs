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
    public TextMeshProUGUI valueTier;

    private double roundData = 0;
    private int roundSignals = 0;
    private SignalTier bestTier = SignalTier.Normal;
    private bool hasRarity = false;

    private readonly Color colorHigh = new Color(0f, 1f, 0.27f, 1f);
    private readonly Color colorMid = new Color(1f, 0.67f, 0f, 1f);
    private readonly Color colorLow = new Color(1f, 0.2f, 0.2f, 1f);

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
        if (rarezaBox != null) rarezaBox.SetActive(false);
        ResetRoundData();
    }

    void Update()
    {
        UpdateLeftPanel();
        UpdateTierDisplay();
    }

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
        float remaining = GameManager.Instance.roundDuration
                        - GameManager.Instance.roundTimer;
        float ratio = Mathf.Clamp01(
            remaining / GameManager.Instance.roundDuration);

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
        if (valueTier == null || GameManager.Instance == null) return;

        int tier = GameManager.Instance.GetCurrentTier();
        valueTier.text = tier > 0 ? "TIER " + tier : "";
    }

    public void RegisterSignalAnalyzed(SignalData signal)
    {
        roundData += signal.dataReward;
        roundSignals += 1;
        UpdateBestRarity(signal);
    }

    void UpdateBestRarity(SignalData signal)
    {
        int currentRank = GetTierRank(signal);
        int bestRank = GetTierRankFromTier(bestTier, false);

        if (!hasRarity || currentRank > bestRank)
        {
            bestTier = signal.tier;
            hasRarity = true;

            if (rarezaBox != null) rarezaBox.SetActive(true);
            if (rarezaText != null)
            {
                rarezaText.text = GetSignalLabel(signal);
                rarezaText.color = SignalData.GetColorForSignal(signal);
            }
        }
    }

    int GetTierRank(SignalData signal)
    {
        int rank = GetTierRankFromTier(signal.tier, signal.isEnhanced);
        return rank;
    }

    int GetTierRankFromTier(SignalTier tier, bool enhanced)
    {
        int base_rank = 0;
        switch (tier)
        {
            case SignalTier.Normal: base_rank = 0; break;
            case SignalTier.Double: base_rank = 1; break;
            case SignalTier.Triple: base_rank = 2; break;
        }
        if (enhanced) base_rank += 10;
        return base_rank;
    }

    string GetSignalLabel(SignalData signal)
    {
        string label = "";
        switch (signal.tier)
        {
            case SignalTier.Normal: label = "RUIDO SIMPLE"; break;
            case SignalTier.Double: label = "RUIDO DOBLE"; break;
            case SignalTier.Triple: label = "RUIDO TRIPLE"; break;
        }
        if (signal.isEnhanced) label += " ENHANCED";
        return label;
    }

    public void ResetRoundData()
    {
        roundData = 0;
        roundSignals = 0;
        hasRarity = false;
        bestTier = SignalTier.Normal;

        if (rarezaBox != null) rarezaBox.SetActive(false);
    }

    public void FlushRoundDataToTotal() { }

    public void SetHUDVisible(bool visible)
    {
        if (leftPanel != null) leftPanel.SetActive(visible);
        if (rightPanel != null) rightPanel.SetActive(visible);
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