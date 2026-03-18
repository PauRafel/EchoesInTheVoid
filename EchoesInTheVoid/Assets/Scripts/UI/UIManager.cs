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
    public TextMeshProUGUI valueAnomalias;

    // Datos de ronda
    private double roundData = 0;
    private int roundSignals = 0;
    private int totalAnomalies = 0;
    private SignalType bestRarity = SignalType.CosmicNoise;
    private bool hasRarityThisRound = false;

    // Colores del timer
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
            valueTiempo.text = Mathf.CeilToInt(remaining) + "s";

        if (timerBarFill != null)
        {
            timerBarFill.fillAmount = ratio;
            timerBarFill.color = ratio > 0.5f ? colorHigh
                                    : ratio > 0.25f ? colorMid
                                    : colorLow;
        }
    }

    // Panel derecho 

    public void RegisterSignalAnalyzed(SignalData signal)
    {
        roundData += signal.dataReward;
        roundSignals += 1;

        if (signal.type == SignalType.Anomaly)
        {
            totalAnomalies++;
            if (valueAnomalias != null)
                valueAnomalias.text = totalAnomalies.ToString();
        }

        UpdateBestRarity(signal.type);
    }

    void UpdateBestRarity(SignalType type)
    {
        int current = RarityRank(type);
        int best = RarityRank(bestRarity);

        if (!hasRarityThisRound || current > best)
        {
            bestRarity = type;
            hasRarityThisRound = true;

            if (rarezaBox != null) rarezaBox.SetActive(true);
            if (rarezaText != null)
            {
                rarezaText.text = "- " + GetSignalLabel(type);
                rarezaText.color = SignalData.GetColorForType(type);
            }
        }
    }

    public void ShowTransmision(string texto, int fragmento, int total)
    {
        if (transmisionBox != null) transmisionBox.SetActive(true);
        if (transmisionText != null)
            transmisionText.text = $"\"{texto}\"\n— fragmento {fragmento}/{total}";
    }

    public void AddRoundTime(float seconds)
    {
        GameManager.Instance.roundTimer =
            Mathf.Max(0f, GameManager.Instance.roundTimer - seconds);
    }

    // Control de ronda 

    public void ResetRoundData()
    {
        roundData = 0;
        roundSignals = 0;
        hasRarityThisRound = false;
        bestRarity = SignalType.CosmicNoise;

        if (rarezaBox != null) rarezaBox.SetActive(false);
        if (transmisionBox != null) transmisionBox.SetActive(false);
    }

    public void FlushRoundDataToTotal()
    {
        GameManager.Instance.AddScanData(roundData);
    }

    // Helpers 

    int RarityRank(SignalType type)
    {
        switch (type)
        {
            case SignalType.CosmicNoise: return 0;
            case SignalType.GroupedSignal: return 1;
            case SignalType.WeakSignal: return 2;
            case SignalType.Echo: return 3;
            case SignalType.MediumSignal: return 4;
            case SignalType.AttractedSignal: return 5;
            case SignalType.StrongSignal: return 6;
            case SignalType.Biomass: return 7;
            case SignalType.Fragmented: return 8;
            case SignalType.Anomaly: return 9;
            case SignalType.DeepSignal: return 10;
            default: return 0;
        }
    }

    string GetSignalLabel(SignalType type)
    {
        switch (type)
        {
            case SignalType.CosmicNoise: return "RUIDO CÓSMICO";
            case SignalType.GroupedSignal: return "SEÑAL AGRUPADA";
            case SignalType.WeakSignal: return "SEÑAL DÉBIL";
            case SignalType.Echo: return "ECO";
            case SignalType.MediumSignal: return "SEÑAL MEDIA";
            case SignalType.AttractedSignal: return "SEÑAL ATRAÍDA";
            case SignalType.StrongSignal: return "SEÑAL FUERTE";
            case SignalType.Biomass: return "BIOMASA";
            case SignalType.Fragmented: return "FRAGMENTADA";
            case SignalType.Anomaly: return "ANOMALÍA";
            case SignalType.DeepSignal: return "SEÑAL PROFUNDA";
            default: return "DESCONOCIDA";
        }
    }

    string FormatData(double value)
    {
        if (value >= 1000000)
            return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000)
            return (value / 1000).ToString("F1") + "K";
        return Mathf.FloorToInt((float)value).ToString();
    }

    public double GetRoundData() => roundData;
    public int GetRoundSignals() => roundSignals;

    public void SetHUDVisible(bool visible)
    {
        if (leftPanel != null) leftPanel.SetActive(visible);
        if (rightPanel != null) rightPanel.SetActive(visible);
    }
}