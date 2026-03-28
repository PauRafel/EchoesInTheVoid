using UnityEngine;

public enum GamePhase
{
    Tutorial,
    Phase1,
    Phase2,
    Phase3,
    Phase4,
    Ending
}

public enum GameState
{
    Scanning,
    RoundEnd,
    Upgrades,
    Paused
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

    [Header("Fase — umbral de datos por ronda")]
    public double phase1Threshold = 20000;

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
        if (currentState != GameState.Scanning) return;
        if (currentPhase == GamePhase.Tutorial) return;

        roundTimer += Time.deltaTime;

        if (roundTimer >= roundDuration)
            EndRound();
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
        if (currentPhase != GamePhase.Phase1) return;
        if (roundDataCollected < phase1Threshold) return;

        // Fase completada — bloquear análisis
        currentState = GameState.RoundEnd;
        OnPhaseComplete?.Invoke();
        Debug.Log("Fase 1 completada!");
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
    public bool IsScanning() => currentState == GameState.Scanning;
}