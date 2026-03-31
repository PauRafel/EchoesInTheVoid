using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Tutorial = 0,
    Phase1 = 1,
    Phase2 = 2,
    Phase3 = 3,
    Phase4 = 4,
    Ending = 5
}

public enum GameState
{
    Scanning,
    RoundEnd,
    Upgrades,
    Paused,
    PhaseTransition
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado")]
    public GamePhase currentPhase = GamePhase.Tutorial;
    public GameState currentState = GameState.Scanning;

    [Header("Datos")]
    public double scanData = 0;
    public double totalDataCollected = 0;
    public int totalSignalsAnalyzed = 0;

    [Header("Rondas")]
    public int currentRound = 0;
    public float roundDuration = 12f;
    public float roundTimer = 0f;

    [Header("Tiers internos de ronda")]
    public int currentTier = 0;
    public double roundDataCollected = 0;

    [Header("Fase umbrales de datos por ronda")]
    public double phase1Threshold = 20000;
    public double phase2Threshold = 500000;
    public double phase3Threshold = 5000000;

    [Header("Multiplicador global de datos")]
    public double dataMultiplier = 1.0;

    // Velocidad de análisis global (100 = base)
    [HideInInspector] public float analysisSpeedPercent = 100f;

    // Probabilidad y daño crítico
    [HideInInspector] public float criticalChance = 0f;
    [HideInInspector] public float criticalBonus = 150f; // +150% velocidad

    // Bonus tiempo al analizar
    [HideInInspector] public float bonusTimeOnAnalysisChance = 0f;
    [HideInInspector] public float bonusTimeOnAnalysisAmount = 0.1f;

    // Bonus tiempo al subir tier
    [HideInInspector] public float bonusTimeOnTier = 0f;

    // Umbrales de tier para fase 1
    private readonly double[] tierThresholds = { 0, 800, 3100, 6200 };

    // Eventos
    public System.Action<int> OnTierChanged;
    public System.Action OnRoundDataReset;
    public System.Action OnPhaseComplete;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!GameManager.Instance.IsScanning()) return;

        UpdateMovingSignals();
    }

    void UpdateMovingSignals()
    {
        List<SignalData> toRemove = new List<SignalData>();

        foreach (SignalData signal in activeSignals)
        {
            if (!signal.IsRevealed() && !signal.IsAnalyzing()) continue;

            if (signal.type == SignalType.Attracted)
            {
                Vector2 dir = (Vector2.zero - signal.position).normalized;
                signal.position += dir * signal.moveSpeed * Time.deltaTime;

                if (signal.visualObject != null)
                    signal.visualObject.transform.position = signal.position;

                if (signal.position.magnitude < 0.2f)
                    toRemove.Add(signal);
            }
            else if (signal.type == SignalType.Biomass)
            {
                signal.position += signal.moveDir * signal.moveSpeed * Time.deltaTime;

                float maxRadius = RadarController.Instance.GetRadarRadius() * 0.9f;
                if (signal.position.magnitude > maxRadius)
                {
                    signal.moveDir = -signal.moveDir;
                    signal.position = signal.position.normalized * maxRadius;
                }

                if (signal.visualObject != null)
                    signal.visualObject.transform.position = signal.position;
            }
        }

        foreach (SignalData s in toRemove)
        {
            if (s.visualObject != null) Destroy(s.visualObject);
            activeSignals.Remove(s);
        }
    }

    // Datos

    public void AddScanData(double amount)
    {
        double final = amount * dataMultiplier;
        scanData += final;
        totalDataCollected += final;
        roundDataCollected += final;

        CheckTierUp();
        CheckPhaseComplete();
    }

    public bool SpendData(double amount)
    {
        if (scanData < amount) return false;
        scanData -= amount;
        return true;
    }

    // Tiers

    void CheckTierUp()
    {
        int newTier = currentTier;

        for (int i = tierThresholds.Length - 1; i >= 0; i--)
        {
            if (roundDataCollected >= tierThresholds[i])
            {
                newTier = i;
                break;
            }
        }

        if (newTier <= currentTier) return;

        currentTier = newTier;
        OnTierChanged?.Invoke(currentTier);

        if (bonusTimeOnTier > 0f)
            AddRoundTime(bonusTimeOnTier);

        Debug.Log($"Tier {currentTier} — datos ronda: {roundDataCollected:N0}");
    }

    public int GetCurrentTier() => currentTier;
    public double GetTierThreshold(int tier) =>
        tier < tierThresholds.Length ? tierThresholds[tier] : double.MaxValue;

    // Fase completa

    void CheckPhaseComplete()
    {
        if (currentState == GameState.RoundEnd) return;
        if (currentState == GameState.PhaseTransition) return;

        double threshold = GetPhaseThreshold(currentPhase);
        if (threshold <= 0) return;
        if (roundDataCollected < threshold) return;

        currentState = GameState.PhaseTransition;
        OnPhaseComplete?.Invoke();
        Debug.Log("Fase " + currentPhase + " completada");
    }

    double GetPhaseThreshold(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Phase1: return phase1Threshold;
            case GamePhase.Phase2: return phase2Threshold;
            case GamePhase.Phase3: return phase3Threshold;
            default: return 0;
        }
    }

    // Tiempo de análisis

    public float GetAnalysisTime(float baseTime)
    {
        float speed = analysisSpeedPercent / 100f;
        float finalTime = baseTime / speed;

        // Aplicar crítico
        if (criticalChance > 0f && Random.value < criticalChance)
        {
            float critSpeed = (analysisSpeedPercent + criticalBonus) / 100f;
            finalTime = baseTime / critSpeed;
        }

        return Mathf.Max(0.05f, finalTime);
    }

    // Rondas

    public void EndRound()
    {
        if (currentState != GameState.Scanning) return;
        currentState = GameState.RoundEnd;
        roundTimer = 0f;
        currentRound++;

        if (RoundEndPanel.Instance != null)
            RoundEndPanel.Instance.Show(
                UIManager.Instance.GetRoundData(),
                UIManager.Instance.GetRoundSignals());
    }

    public void StartRound()
    {
        currentState = GameState.Scanning;
        currentTier = 0;
        roundDataCollected = 0;
        OnRoundDataReset?.Invoke();

        if (currentPhase != GamePhase.Tutorial &&
            SignalManager.Instance != null)
            SignalManager.Instance.GenerateRoundSignals();

        Debug.Log($"Ronda {currentRound + 1} iniciada.");
    }

    public void AddRoundTime(float seconds)
    {
        roundTimer = Mathf.Max(0f, roundTimer - seconds);
    }

    // Fase 

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"Fase: {phase}");
    }

    public void SetState(GameState state) => currentState = state;
    public bool IsScanning()
    {
        return currentState == GameState.Scanning ||
               currentState == GameState.PhaseTransition;
    }
}