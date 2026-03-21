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
    public double roundDataCollected = 0; // datos acumulados en la ronda actual

    // Umbrales de tier (datos acumulados en la ronda)
    private readonly double[] tierThresholds = { 0, 800, 5000, 30000 };

    [Header("Multiplicador de datos")]
    public double dataMultiplier = 1.0;

    // Eventos
    public System.Action<int> OnTierChanged;
    public System.Action OnRoundDataReset;

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

        if (newTier > currentTier)
        {
            currentTier = newTier;
            OnTierChanged?.Invoke(currentTier);

            float bonusTime = UpgradeManager.Instance != null
                ? UpgradeManager.Instance.GetBonusTimeOnTier()
                : 0f;
            if (bonusTime > 0f)
                AddRoundTime(bonusTime);

            Debug.Log($"Tier {currentTier} — datos: {roundDataCollected:N0}");
        }
    }

    public int GetCurrentTier() => currentTier;

    public double GetTierThreshold(int tier)
    {
        if (tier < tierThresholds.Length)
            return tierThresholds[tier];
        return double.MaxValue;
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
                UIManager.Instance.GetRoundSignals()
            );
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

    public void SetState(GameState state)
    {
        currentState = state;
    }

    public bool IsScanning()
        => currentState == GameState.Scanning;
}