using UnityEngine;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Instance { get; private set; }

    [Header("Referencias")]
    public GameObject signalPrefab;

    [Header("Configuracion base")]
    public int baseSignalCount = 20;

    // Distribucion de tipos de señal
    private float chanceDouble = 0f;
    private float chanceTriple = 0f;
    private float chanceEnhanced = 0f;
    private float enhancedDataBonus = 0f;

    // Señales extra al subir tier (porcentaje)
    private int extraSignalsOnTierPercent = 0;

    // Probabilidad de señal extra al analizar
    private float chanceExtraOnAnalysis = 0f;

    // Señales activas en la ronda
    private List<SignalData> activeSignals = new List<SignalData>();

    // Total generadas esta ronda (para calcular extra por tier)
    private int totalGeneratedThisRound = 0;

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

    public void GenerateRoundSignals()
    {
        ClearAllSignals();
        totalGeneratedThisRound = 0;
        SpawnBatch(baseSignalCount);
    }

    void SpawnBatch(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnSignal();
    }

    void SpawnSignal()
    {
        SignalData signal = new SignalData();
        signal.state = SignalState.Hidden;
        signal.position = GetRandomPosition();

        float angleRad = Mathf.Atan2(signal.position.y, signal.position.x);
        signal.signalAngle = angleRad * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f)
            signal.signalAngle += 360f;

        AssignTier(signal);
        AssignEnhanced(signal);
        AssignValues(signal);
        CreateVisual(signal);

        if (signal.visualObject != null)
            signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
        totalGeneratedThisRound++;
    }

    void AssignTier(SignalData signal)
    {
        float roll = Random.value;

        if (chanceTriple > 0f && chanceDouble > 0f)
        {
            // M2 activa: distribucion 1/3 1/3 1/3
            if (roll < chanceTriple)
                signal.tier = SignalTier.Triple;
            else if (roll < chanceTriple + chanceDouble)
                signal.tier = SignalTier.Double;
            else
                signal.tier = SignalTier.Normal;
        }
        else if (chanceDouble > 0f)
        {
            // Solo M1: 50% doble 50% simple
            signal.tier = roll < chanceDouble ? SignalTier.Double : SignalTier.Normal;
        }
        else
        {
            signal.tier = SignalTier.Normal;
        }

        signal.type = GetTypeForTier(signal.tier);
    }

    void AssignEnhanced(SignalData signal)
    {
        if (chanceEnhanced <= 0f) return;
        signal.isEnhanced = Random.value < chanceEnhanced;
    }

    void AssignValues(SignalData signal)
    {
        float baseTime = SignalData.GetBaseAnalysisTime();
        double baseReward = SignalData.GetBaseReward();

        float tierTimeMultiplier = GetTierTimeMultiplier(signal.tier);
        double tierRewardMultiplier = GetTierRewardMultiplier(signal.tier);

        float timeBeforeEnhanced = baseTime * tierTimeMultiplier;
        double rewardBeforeEnhanced = baseReward * tierRewardMultiplier;

        if (signal.isEnhanced)
        {
            timeBeforeEnhanced *= 1.5f;
            rewardBeforeEnhanced *= 2.0 * (1.0 + enhancedDataBonus);
        }

        signal.analysisTime = GameManager.Instance.GetAnalysisTime(timeBeforeEnhanced);
        signal.dataReward = rewardBeforeEnhanced;
        signal.baseScale = GetTierScale(signal.tier, signal.isEnhanced);
        signal.baseScale *= phase2SignalScaleMultiplier;
    }

    float GetTierTimeMultiplier(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Double: return 1.2f;
            case SignalTier.Triple: return 1.3f;
            default: return 1f;
        }
    }

    double GetTierRewardMultiplier(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Double: return 2.0;
            case SignalTier.Triple: return 3.0;
            default: return 1.0;
        }
    }

    float GetTierScale(SignalTier tier, bool enhanced)
    {
        float scale = 1f;
        switch (tier)
        {
            case SignalTier.Double: scale = 1.5f; break;
            case SignalTier.Triple: scale = 2.0f; break;
        }
        if (enhanced) scale *= 1.2f;
        return scale;
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

    void OnTierUp(int tier)
    {
        if (extraSignalsOnTierPercent <= 0) return;

        int extra = Mathf.RoundToInt(
            totalGeneratedThisRound * (extraSignalsOnTierPercent / 100f));

        if (extra <= 0) return;

        Debug.Log("Tier " + tier + " genera " + extra + " señales extra");
        SpawnBatch(extra);
    }

    void OnRoundReset()
    {
        totalGeneratedThisRound = 0;
    }

    void CreateVisual(SignalData signal)
    {
        if (signalPrefab == null) return;

        GameObject obj = Instantiate(
            signalPrefab, signal.position, Quaternion.identity);
        obj.transform.SetParent(transform);

        SignalBlip blip = obj.GetComponent<SignalBlip>();
        if (blip != null)
            blip.Setup(signal);

        signal.visualObject = obj;
    }

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

    public void CompleteSignal(SignalData signal)
    {
        if (signal.IsCompleted()) return;
        signal.state = SignalState.Completed;

        if (signal.type == SignalType.PhaseTransition)
        {
            if (signal.visualObject != null)
                Destroy(signal.visualObject);
            activeSignals.Remove(signal);
            CompletePhaseTransitionSignal();
            return;
        }

        GameManager.Instance.AddScanData(signal.dataReward);
        GameManager.Instance.totalSignalsAnalyzed++;

        if (UIManager.Instance != null)
            UIManager.Instance.RegisterSignalAnalyzed(signal);

        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.IsTutorialActive())
            TutorialManager.Instance.OnSignalAnalyzed();

        float bonusTime = 0f;

        if (GameManager.Instance.bonusTimeOnAnalysisChance > 0f &&
            Random.value < GameManager.Instance.bonusTimeOnAnalysisChance)
            bonusTime += GameManager.Instance.bonusTimeOnAnalysisAmount;

        if (bonusTime > 0f)
            GameManager.Instance.AddRoundTime(bonusTime);

        if (chanceExtraOnAnalysis > 0f &&
    Random.value < chanceExtraOnAnalysis)
        {
            SpawnSignal();
        }

        if (signal.visualObject != null)
            Destroy(signal.visualObject);

        activeSignals.Remove(signal);
    }

    public void ClearAllSignals()
    {
        foreach (SignalData s in activeSignals)
            if (s.visualObject != null)
                Destroy(s.visualObject);

        activeSignals.Clear();

        if (SignalAnalyzer.Instance != null)
            SignalAnalyzer.Instance.ClearAnalyzing();
    }

    public List<SignalData> GetActiveSignals() => activeSignals;
    public List<SignalData> GetRevealedSignals() =>
        activeSignals.FindAll(s => s.IsRevealed());
    public List<SignalData> GetUnrevealedSignals() =>
        activeSignals.FindAll(s => s.IsHidden());

    public void SetBaseSignalCount(int count) => baseSignalCount = count;
    public void SetChanceDouble(float chance) => chanceDouble = chance;
    public void SetChanceTriple(float chance) => chanceTriple = chance;
    public void SetChanceEnhanced(float chance) => chanceEnhanced = chance;
    public void SetEnhancedDataBonus(float bonus) => enhancedDataBonus = bonus;
    public void SetExtraSignalsOnTierPercent(int percent) => extraSignalsOnTierPercent = percent;
    public void SetChanceExtraOnAnalysis(float chance) => chanceExtraOnAnalysis = chance;

    Vector2 GetRandomPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0.15f, 0.95f) *
                         RadarController.Instance.GetRadarRadius();
        return new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance);
    }

    public void SpawnPhaseTransitionSignal()
    {
        SignalData signal = new SignalData();
        signal.type = SignalType.PhaseTransition;
        signal.state = SignalState.Hidden;
        signal.position = GetRandomPosition();
        signal.dataReward = 0;
        signal.analysisTime = 3f;
        signal.baseScale = 4f;

        float angleRad = Mathf.Atan2(signal.position.y, signal.position.x);
        signal.signalAngle = angleRad * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f)
            signal.signalAngle += 360f;

        CreateVisual(signal);
        if (signal.visualObject != null)
            signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
        isPhaseTransitionActive = true;
    }

    private bool isPhaseTransitionActive = false;
    public bool IsPhaseTransitionActive() => isPhaseTransitionActive;

    public void CompletePhaseTransitionSignal()
    {
        isPhaseTransitionActive = false;
        if (PhaseTransitionManager.Instance != null)
            PhaseTransitionManager.Instance.OnTransitionSignalAnalyzed();
    }

    private float phase2SignalScaleMultiplier = 1f;

    public void SetPhase2SignalScale(float multiplier)
    {
        phase2SignalScaleMultiplier = multiplier;
    }
}