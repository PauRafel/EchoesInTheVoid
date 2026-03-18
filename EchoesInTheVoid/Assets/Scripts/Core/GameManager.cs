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

    [Header("Estado del juego")]
    public GamePhase currentPhase = GamePhase.Tutorial;
    public GameState currentState = GameState.Scanning;

    [Header("Datos")]
    public double scanData = 0;
    public double totalDataCollected = 0;
    public int totalSignalsAnalyzed = 0;

    [Header("Rondas")]
    public int currentRound = 0;
    public float roundDuration = 10f;
    public float roundTimer = 0f;

    [Header("Multiplicador de datos")]
    public double dataMultiplier = 1.0;

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

    public void AddScanData(double amount)
    {
        double final = amount * dataMultiplier;
        scanData += final;
        totalDataCollected += final;
    }

    public bool SpendData(double amount)
    {
        if (scanData < amount) return false;
        scanData -= amount;
        return true;
    }

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
        Debug.Log($"Ronda {currentRound + 1} iniciada.");
    }

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
    {
        return currentState == GameState.Scanning;
    }
}