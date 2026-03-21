using UnityEngine;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Instance { get; private set; }

    [Header("Referencias")]
    public GameObject signalPrefab;

    [Header("Configuración base")]
    public int baseSignalCount = 20;
    public float baseAnalysisTime = 2.5f;

    // Señales activas en la ronda actual
    private List<SignalData> activeSignals = new List<SignalData>();

    // Probabilidades de tier (modificadas por mejoras) 
    private float chanceDouble = 0f; // prob de que una señal sea doble
    private float chanceTriple = 0f; // prob de que una señal sea triple
    private float chanceEnhanced = 0f; // prob de que una señal sea enhanced

    // modificadores de análisis
    private float analysisTimeMultiplier = 1f;
    private float criticalChance = 0f; // prob de análisis crítico (-50%)
    private float criticalMultiplier = 0.5f; // cuánto reduce el crítico

    // Modificadores de cantidad 
    private int extraSignalsOnTier = 0;    // señales extra al subir tier (%)
    private float chanceExtraOnAnalysis = 0f;   // prob de generar señal extra al analizar

    // Modificadores de datos enhanced
    private float enhancedDataBonus = 0f;   // bonus % datos enhanced (0.1 = 10%)

    // Señales totales a generar esta ronda (base + mejoras)
    private int totalSignalsThisRound = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Suscribirse al evento de tier
        GameManager.Instance.OnTierChanged += OnTierUp;
        GameManager.Instance.OnRoundDataReset += OnRoundReset;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTierChanged -= OnTierUp;
            GameManager.Instance.OnRoundDataReset -= OnRoundReset;
        }
    }

    // Generación de ronda 

    public void GenerateRoundSignals()
    {
        ClearAllSignals();

        totalSignalsThisRound = baseSignalCount;
        SpawnSignalBatch(totalSignalsThisRound);
    }

    void SpawnSignalBatch(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnSignal();
    }

    void SpawnSignal()
    {
        SignalData signal = new SignalData();
        signal.state = SignalState.Hidden;
        signal.position = GetRandomPosition();
        signal.signalAngle = Mathf.Atan2(signal.position.y, signal.position.x)
                             * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f) signal.signalAngle += 360f;

        // Determinar tier
        signal.tier = RollTier();

        // Determinar tipo base según tier
        signal.type = GetTypeForTier(signal.tier);

        // Calcular valores según tier
        AssignValues(signal);

        // Visual
        CreateVisual(signal);
        signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
    }

    SignalTier RollTier()
    {
        float roll = Random.value;

        if (chanceTriple > 0f && roll < chanceTriple)
            return SignalTier.Triple;

        if (chanceDouble > 0f && roll < chanceDouble + chanceTriple)
            return SignalTier.Double;

        return SignalTier.Normal;
    }

    SignalType GetTypeForTier(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Double: return SignalType.CosmicNoiseDouble;
            case SignalTier.Triple: return SignalType.CosmicNoiseTriple;
            default: return SignalType.CosmicNoise;
        }
    }

    void AssignValues(SignalData signal)
    {
        float baseTime = baseAnalysisTime * analysisTimeMultiplier;
        double baseReward = SignalData.GetBaseReward(SignalType.CosmicNoise);

        switch (signal.tier)
        {
            case SignalTier.Normal:
                signal.analysisTime = baseTime;
                signal.dataReward = baseReward;
                signal.baseScale = 1f;
                break;

            case SignalTier.Double:
                signal.analysisTime = baseTime * 2f;
                signal.dataReward = baseReward * 2;
                signal.baseScale = 1.4f;
                break;

            case SignalTier.Triple:
                signal.analysisTime = baseTime * 3f;
                signal.dataReward = baseReward * 3;
                signal.baseScale = 1.8f;
                break;
        }

        // Aplicar enhanced
        if (chanceEnhanced > 0f && Random.value < chanceEnhanced)
        {
            signal.tier = SignalTier.Enhanced;
            signal.dataReward *= 2 * (1 + enhancedDataBonus);
            signal.analysisTime *= 2f;
            signal.baseScale *= 1.3f;
        }

        // Aplicar crítico
        if (criticalChance > 0f && Random.value < criticalChance)
            signal.analysisTime *= criticalMultiplier;
    }

    // Tier up 

    void OnTierUp(int tier)
    {
        if (extraSignalsOnTier <= 0) return;

        int extra = Mathf.RoundToInt(totalSignalsThisRound * (extraSignalsOnTier / 100f));
        Debug.Log($"Tier {tier} — generando {extra} señales extra");
        SpawnSignalBatch(extra);
        totalSignalsThisRound += extra;
    }

    void OnRoundReset()
    {
        totalSignalsThisRound = 0;
    }

    // Visual

    void CreateVisual(SignalData signal)
    {
        if (signalPrefab == null) return;

        GameObject obj = Instantiate(signalPrefab,
                         signal.position, Quaternion.identity);
        obj.transform.SetParent(transform);

        SignalBlip blip = obj.GetComponent<SignalBlip>();
        if (blip != null) blip.Setup(signal);

        signal.visualObject = obj;
    }

    // Reveal 

    public void RevealSignal(SignalData signal)
    {
        if (!signal.IsHidden()) return;
        signal.state = SignalState.Revealed;
        if (signal.visualObject != null)
            signal.visualObject.SetActive(true);

        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.IsTutorialActive())
            TutorialManager.Instance.OnSignalRevealed();
    }

    public List<SignalData> GetUnrevealedSignals()
        => activeSignals.FindAll(s => s.IsHidden());

    public List<SignalData> GetRevealedSignals()
        => activeSignals.FindAll(s => s.IsRevealed());

    // Completar señal

    public void CompleteSignal(SignalData signal)
    {
        if (signal.IsCompleted()) return;
        signal.state = SignalState.Completed;

        GameManager.Instance.AddScanData(signal.dataReward);
        GameManager.Instance.totalSignalsAnalyzed++;

        if (UIManager.Instance != null)
            UIManager.Instance.RegisterSignalAnalyzed(signal);

        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.IsTutorialActive())
            TutorialManager.Instance.OnSignalAnalyzed();

        // Bonus tiempo si hay mejora
        float bonusTime = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetBonusTimeOnAnalysis(signal)
            : 0f;
        if (bonusTime > 0f)
            GameManager.Instance.AddRoundTime(bonusTime);

        // Probabilidad de señal extra al analizar
        if (chanceExtraOnAnalysis > 0f && Random.value < chanceExtraOnAnalysis)
        {
            SpawnSignal();
            // Revelar inmediatamente si el sweep ya pasó
            SignalData last = activeSignals[activeSignals.Count - 1];
            RevealSignal(last);
        }

        if (signal.visualObject != null)
            Destroy(signal.visualObject);

        activeSignals.Remove(signal);
    }

    // Limpiar

    public void ClearAllSignals()
    {
        foreach (SignalData s in activeSignals)
            if (s.visualObject != null)
                Destroy(s.visualObject);

        activeSignals.Clear();

        if (SignalAnalyzer.Instance != null)
            SignalAnalyzer.Instance.ClearAnalyzing();
    }

    // API pública para mejoras

    public void SetBaseSignalCount(int count)
        => baseSignalCount = count;

    public void SetAnalysisTimeMultiplier(float mult)
        => analysisTimeMultiplier = mult;

    public void SetChanceDouble(float chance)
        => chanceDouble = chance;

    public void SetChanceTriple(float chance)
        => chanceTriple = chance;

    public void SetChanceEnhanced(float chance)
        => chanceEnhanced = chance;

    public void SetEnhancedDataBonus(float bonus)
        => enhancedDataBonus = bonus;

    public void SetCriticalChance(float chance)
        => criticalChance = chance;

    public void SetCriticalMultiplier(float mult)
        => criticalMultiplier = mult;

    public void SetExtraSignalsOnTier(int percent)
        => extraSignalsOnTier = percent;

    public void SetChanceExtraOnAnalysis(float chance)
        => chanceExtraOnAnalysis = chance;

    public List<SignalData> GetActiveSignals() => activeSignals;

    // Posición aleatoria

    Vector2 GetRandomPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0.15f, 0.95f)
                         * RadarController.Instance.GetRadarRadius();
        return new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance);
    }
}