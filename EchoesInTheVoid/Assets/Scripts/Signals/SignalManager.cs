using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float chanceQuadruple = 0f;
    private float echoRange = 1.5f;
    private float echoAnalyzePercent = 0f;
    private float echoChainChance = 0f;
    private float echoRangeMultiplier = 1f;
    private float superEnhancedChance = 0f;
    private float superEnhancedBonus = 0.5f;

    private float chanceQuintuple = 0f;
    private float chanceAttracted = 0f;
    private float chanceBiomass = 0f;
    private float attractedSpeedMin = 0.3f;
    private float attractedSpeedMax = 0.8f;
    private float attractedBonusChance = 0f;
    private float biomassDataBonus = 0f;

    private float phase3SignalScaleMultiplier = 1f;

    public void SetPhase3SignalScale()
    {
        phase3SignalScaleMultiplier = 0.5f;
        phase2SignalScaleMultiplier *= 0.5f;
    }

    // Señales extra al subir tier (porcentaje)
    private int extraSignalsOnTierPercent = 0;

    // Probabilidad de señal extra al analizar
    private float chanceExtraOnAnalysis = 0f;

    // Señales activas en la ronda
    private List<SignalData> activeSignals = new List<SignalData>();

    // Total generadas esta ronda (para calcular extra por tier)
    private int totalGeneratedThisRound = 0;

    public float GetChanceExtraOnAnalysis() => chanceExtraOnAnalysis;
    public int GetExtraSignalsOnTierPercent() => extraSignalsOnTierPercent;
    public float GetSuperEnhancedBonus() => superEnhancedBonus;


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
        float roll = Random.value;

        if (chanceBiomass > 0f && roll < chanceBiomass)
        {
            SpawnBiomassSignal();
            return;
        }

        roll = Random.value;
        if (chanceAttracted > 0f && roll < chanceAttracted)
        {
            SpawnAttractedSignal();
            return;
        }

        float ecoRoll = echoAnalyzePercent > 0f ? Random.value : 1f;
        bool isEcho = ecoRoll < 0.1f && echoAnalyzePercent > 0f;

        SignalData signal = new SignalData();
        signal.state = SignalState.Hidden;
        signal.position = GetRandomPosition();

        float angleRad = Mathf.Atan2(signal.position.y, signal.position.x);
        signal.signalAngle = angleRad * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f)
            signal.signalAngle += 360f;

        if (isEcho)
        {
            signal.type = SignalType.Echo;
            signal.tier = SignalTier.Normal;
            AssignEchoValues(signal);
        }
        else
        {
            AssignTier(signal);
            AssignEnhanced(signal);
            AssignValues(signal);
        }

        CreateVisual(signal);
        if (signal.visualObject != null)
            signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
        totalGeneratedThisRound++;
    }

    void SpawnAttractedSignal()
    {
        SignalData signal = new SignalData();
        signal.type = SignalType.Attracted;
        signal.tier = SignalTier.Normal;
        signal.state = SignalState.Hidden;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = RadarController.Instance.GetRadarRadius() * 0.95f;
        signal.position = new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius);

        float angleRad = Mathf.Atan2(signal.position.y, signal.position.x);
        signal.signalAngle = angleRad * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f)
            signal.signalAngle += 360f;

        signal.analysisTime = GameManager.Instance.GetAnalysisTime(2f);
        signal.dataReward = SignalData.GetBaseReward() * 8.0;
        signal.baseScale = 1.5f * phase2SignalScaleMultiplier;

        signal.moveSpeed = Random.Range(attractedSpeedMin, attractedSpeedMax);

        CreateVisual(signal);
        if (signal.visualObject != null)
            signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
        totalGeneratedThisRound++;
    }

    void SpawnBiomassSignal()
    {
        SignalData signal = new SignalData();
        signal.type = SignalType.Biomass;
        signal.tier = SignalTier.Normal;
        signal.state = SignalState.Hidden;

        signal.position = GetRandomPosition();

        float angleRad = Mathf.Atan2(signal.position.y, signal.position.x);
        signal.signalAngle = angleRad * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f)
            signal.signalAngle += 360f;

        signal.analysisTime = GameManager.Instance.GetAnalysisTime(3f);
        signal.dataReward = SignalData.GetBaseReward() * 15.0 *
                              (1.0 + biomassDataBonus);
        signal.baseScale = 2f * phase2SignalScaleMultiplier;

        signal.moveSpeed = Random.Range(0.1f, 0.25f);
        signal.moveDir = Random.insideUnitCircle.normalized;

        CreateVisual(signal);
        if (signal.visualObject != null)
            signal.visualObject.SetActive(false);

        activeSignals.Add(signal);
        totalGeneratedThisRound++;
    }

    void AssignTier(SignalData signal)
    {
        float roll = Random.value;

        if (chanceQuintuple > 0f && roll < chanceQuintuple)
        {
            signal.tier = SignalTier.Quintuple;
            signal.type = SignalType.CosmicNoiseQuintuple;
            return;
        }

        roll = Random.value;

        if (chanceQuadruple > 0f && roll < chanceQuadruple)
        {
            signal.tier = SignalTier.Quadruple;
            signal.type = SignalType.CosmicNoiseQuadruple;
            return;
        }

        roll = Random.value;

        if (chanceTriple > 0f && chanceDouble > 0f)
        {
            if (roll < chanceTriple)
            {
                signal.tier = SignalTier.Triple;
                signal.type = SignalType.CosmicNoiseTriple;
            }
            else if (roll < chanceTriple + chanceDouble)
            {
                signal.tier = SignalTier.Double;
                signal.type = SignalType.CosmicNoiseDouble;
            }
            else
            {
                signal.tier = SignalTier.Normal;
                signal.type = SignalType.CosmicNoise;
            }
        }
        else if (chanceDouble > 0f)
        {
            signal.tier = roll < chanceDouble ? SignalTier.Double : SignalTier.Normal;
            signal.type = signal.tier == SignalTier.Double
                ? SignalType.CosmicNoiseDouble : SignalType.CosmicNoise;
        }
        else
        {
            signal.tier = SignalTier.Normal;
            signal.type = SignalType.CosmicNoise;
        }
    }

    void AssignEnhanced(SignalData signal)
    {
        if (chanceEnhanced <= 0f) return;
        if (Random.value >= chanceEnhanced) return;

        signal.isEnhanced = true;

        if (superEnhancedChance > 0f && Random.value < superEnhancedChance)
            signal.isSuperEnhanced = true;
    }

    void AssignValues(SignalData signal)
    {
        signal.baseScale = GetTierScale(signal.tier, signal.isEnhanced);
        signal.baseScale *= phase2SignalScaleMultiplier;

        float baseTime = SignalData.GetBaseAnalysisTime();
        double baseReward = SignalData.GetBaseReward();

        float tierTimeMult = GetTierTimeMultiplier(signal.tier);
        double tierRewardMult = GetTierRewardMultiplier(signal.tier);

        float timeBeforeEnhanced = baseTime * tierTimeMult;
        double rewardBeforeEnhanced = baseReward * tierRewardMult;

        if (signal.isEnhanced)
        {
            timeBeforeEnhanced *= 1.5f;
            double enhancedMult = 2.0 * (1.0 + enhancedDataBonus);

            if (signal.isSuperEnhanced)
                enhancedMult *= (1.0 + superEnhancedBonus);

            rewardBeforeEnhanced *= enhancedMult;
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
            case SignalTier.Quadruple: return 1.4f;
            case SignalTier.Quintuple: return 1.5f;
            default: return 1f;
        }
    }

    double GetTierRewardMultiplier(SignalTier tier)
    {
        switch (tier)
        {
            case SignalTier.Double: return 2.0;
            case SignalTier.Triple: return 3.0;
            case SignalTier.Quadruple: return 4.0;
            case SignalTier.Quintuple: return 5.0;
            default: return 1.0;
        }
    }

    float GetTierScale(SignalTier tier, bool enhanced)
    {
        float scale = 1f;
        switch (tier)
        {
            case SignalTier.Double: scale = 1.4f; break;
            case SignalTier.Triple: scale = 1.8f; break;
            case SignalTier.Quadruple: scale = 2.2f; break;
            case SignalTier.Quintuple: scale = 2.6f; break;
        }
        if (enhanced) scale *= 1.3f;
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

        if (signal.type == SignalType.Attracted && attractedBonusChance > 0f)
        {
            float distToCenter = signal.position.magnitude;
            float ring2Radius = RadarController.Instance.GetRadarRadius() * 0.5f;
            if (distToCenter > ring2Radius)
                signal.dataReward *= 1.5;
        }

        GameManager.Instance.AddScanData(signal.dataReward);
        GameManager.Instance.totalSignalsAnalyzed++;

        if (UIManager.Instance != null)
            UIManager.Instance.RegisterSignalAnalyzed(signal);

        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.IsTutorialActive())
            TutorialManager.Instance.OnSignalAnalyzed();

        if (signal.type == SignalType.Echo)
            TriggerEchoEffect(signal);

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

    void AssignEchoValues(SignalData signal)
    {
        signal.analysisTime = GameManager.Instance.GetAnalysisTime(1.5f);
        signal.dataReward = SignalData.GetBaseReward() * 1.5;
        signal.baseScale = 1.3f * phase2SignalScaleMultiplier;
    }

    void TriggerEchoEffect(SignalData echoSignal)
    {
        if (echoAnalyzePercent <= 0f) return;

        float effectiveRange = echoRange * echoRangeMultiplier;
        List<SignalData> inRange = new List<SignalData>();

        foreach (SignalData s in activeSignals)
        {
            if (s == echoSignal) continue;
            if (s.IsCompleted()) continue;
            if (s.type == SignalType.PhaseTransition) continue;

            float dist = Vector2.Distance(echoSignal.position, s.position);
            if (dist <= effectiveRange)
                inRange.Add(s);
        }

        int countToAnalyze = Mathf.RoundToInt(inRange.Count * echoAnalyzePercent);
        countToAnalyze = Mathf.Min(countToAnalyze, inRange.Count);

        for (int i = 0; i < countToAnalyze; i++)
        {
            int idx = Random.Range(0, inRange.Count);
            SignalData s = inRange[idx];
            inRange.RemoveAt(idx);

            s.state = SignalState.Analyzing;
            StartCoroutine(AutoAnalyzeSignal(s, echoSignal.position));
        }
    }

    IEnumerator AutoAnalyzeSignal(SignalData signal, Vector2 origin)
    {
        float elapsed = 0f;

        while (elapsed < signal.analysisTime)
        {
            if (signal.IsCompleted()) yield break;
            elapsed += Time.deltaTime;
            signal.analysisProgress = elapsed;
            yield return null;
        }

        if (!signal.IsCompleted())
        {
            bool isChain = echoChainChance > 0f &&
                           Random.value < echoChainChance &&
                           signal.type != SignalType.Echo;

            CompleteSignal(signal);

            if (isChain)
                TriggerChainEffect(signal, origin);
        }
    }

    void TriggerChainEffect(SignalData signal, Vector2 origin)
    {
        float chainRange = echoRange * echoRangeMultiplier * 0.5f;
        List<SignalData> inRange = new List<SignalData>();

        foreach (SignalData s in activeSignals)
        {
            if (s.IsCompleted()) continue;
            if (s.type == SignalType.PhaseTransition) continue;

            float dist = Vector2.Distance(signal.position, s.position);
            if (dist <= chainRange)
                inRange.Add(s);
        }

        int countToAnalyze = Mathf.RoundToInt(inRange.Count * echoAnalyzePercent);
        countToAnalyze = Mathf.Min(countToAnalyze, inRange.Count);

        for (int i = 0; i < countToAnalyze; i++)
        {
            int idx = Random.Range(0, inRange.Count);
            SignalData s = inRange[idx];
            inRange.RemoveAt(idx);
            StartCoroutine(AutoAnalyzeSignal(s, signal.position));
        }
    }
    public void SetChanceQuadruple(float chance) => chanceQuadruple = chance;
    public void SetEchoRange(float range) => echoRange = range;
    public void SetEchoAnalyzePercent(float percent) => echoAnalyzePercent = percent;
    public void SetEchoChainChance(float chance) => echoChainChance = chance;
    public void SetEchoRangeMultiplier(float mult) => echoRangeMultiplier = mult;
    public void SetSuperEnhancedChance(float chance) => superEnhancedChance = chance;
    public void SetSuperEnhancedBonus(float bonus) => superEnhancedBonus = bonus;
    public void SetChanceQuintuple(float chance) => chanceQuintuple = chance;
    public void SetChanceAttracted(float chance) => chanceAttracted = chance;
    public void SetChanceBiomass(float chance) => chanceBiomass = chance;
    public void SetAttractedBonusChance(float chance) => attractedBonusChance = chance;
    public void SetBiomassDataBonus(float bonus) => biomassDataBonus = bonus;
}