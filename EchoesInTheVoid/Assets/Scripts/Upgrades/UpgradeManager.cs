using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    private Dictionary<UpgradeBranch, bool> unlockedBranches =
        new Dictionary<UpgradeBranch, bool>();

    // Bonus de tiempo por señal especial
    private float bonusTimeAnomaly = 0f;
    private float bonusTimeDeepSignal = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeBranches();
        InitializeUpgrades();
    }

    void InitializeBranches()
    {
        foreach (UpgradeBranch branch in
                 System.Enum.GetValues(typeof(UpgradeBranch)))
            unlockedBranches[branch] = false;
    }

    void InitializeUpgrades()
    {
        // RAMA RADAR
        Add("radar_1", "Amplificador I", "Velocidad sweep +20%",
            400, UpgradeBranch.Radar, 1, GamePhase.Phase1,
            UpgradeEffect.IncreaseSweepSpeed, 54f);

        Add("radar_2", "Radar Mk.II", "2 anillos. Desbloquea Fase 2.",
            600, UpgradeBranch.Radar, 2, GamePhase.Phase1,
            UpgradeEffect.UnlockPhase, 2f);

        Add("radar_3", "Amplificador II", "Velocidad sweep +17%",
            3000, UpgradeBranch.Radar, 3, GamePhase.Phase2,
            UpgradeEffect.IncreaseSweepSpeed, 63f);

        Add("radar_4", "Expansión I", "3 anillos de detección",
            5000, UpgradeBranch.Radar, 4, GamePhase.Phase2,
            UpgradeEffect.AddRing, 3f);

        Add("radar_5", "Amplificador III", "Velocidad sweep +14%",
            8000, UpgradeBranch.Radar, 5, GamePhase.Phase2,
            UpgradeEffect.IncreaseSweepSpeed, 72f);

        Add("radar_6", "Expansión II", "4 anillos de detección",
            12000, UpgradeBranch.Radar, 6, GamePhase.Phase2,
            UpgradeEffect.AddRing, 4f);

        Add("radar_7", "Amplificador IV", "Velocidad sweep +13%",
            18000, UpgradeBranch.Radar, 7, GamePhase.Phase2,
            UpgradeEffect.IncreaseSweepSpeed, 81f);

        Add("radar_8", "Radar Mk.III", "Segunda barra sweep. Desbloquea Fase 3.",
            25000, UpgradeBranch.Radar, 8, GamePhase.Phase2,
            UpgradeEffect.UnlockPhase, 3f);

        Add("radar_9", "Amplificador V", "Velocidad sweep +11%",
            90000, UpgradeBranch.Radar, 9, GamePhase.Phase3,
            UpgradeEffect.IncreaseSweepSpeed, 90f);

        Add("radar_10", "Expansión III", "5 anillos de detección",
            140000, UpgradeBranch.Radar, 10, GamePhase.Phase3,
            UpgradeEffect.AddRing, 5f);

        Add("radar_11", "Amplificador VI", "Velocidad sweep +20%",
            200000, UpgradeBranch.Radar, 11, GamePhase.Phase3,
            UpgradeEffect.IncreaseSweepSpeed, 108f);

        Add("radar_12", "Amplificador VII", "Velocidad sweep +17%",
            280000, UpgradeBranch.Radar, 12, GamePhase.Phase3,
            UpgradeEffect.IncreaseSweepSpeed, 126f);

        Add("radar_13", "Expansión IV", "6 anillos de detección",
            450000, UpgradeBranch.Radar, 13, GamePhase.Phase4,
            UpgradeEffect.AddRing, 6f);

        Add("radar_14", "Radar Mk.IV",
            "Máxima potencia. 135 grados/s. Desbloquea Fase 4 y señal final.",
            1000000, UpgradeBranch.Radar, 14, GamePhase.Phase4,
            UpgradeEffect.UnlockPhase, 4f);

        // RAMA TIEMPO
        Add("tiempo_1", "Módulo temporal I", "Duración ronda: 15s",
            200, UpgradeBranch.Tiempo, 1, GamePhase.Phase1,
            UpgradeEffect.IncreaseRoundDuration, 15f);

        Add("tiempo_2", "Módulo temporal II", "Duración ronda: 20s",
            400, UpgradeBranch.Tiempo, 2, GamePhase.Phase1,
            UpgradeEffect.IncreaseRoundDuration, 20f);

        Add("tiempo_3", "Módulo temporal III", "Duración ronda: 27s",
            4000, UpgradeBranch.Tiempo, 3, GamePhase.Phase2,
            UpgradeEffect.IncreaseRoundDuration, 27f);

        Add("tiempo_4", "Receptor anomalías", "Anomalías añaden +3s al timer",
            7000, UpgradeBranch.Tiempo, 4, GamePhase.Phase2,
            UpgradeEffect.BonusTimeOnAnomaly, 3f);

        Add("tiempo_5", "Módulo temporal IV", "Duración ronda: 35s",
            11000, UpgradeBranch.Tiempo, 5, GamePhase.Phase2,
            UpgradeEffect.IncreaseRoundDuration, 35f);

        Add("tiempo_6", "Módulo temporal V", "Duración ronda: 42s",
            80000, UpgradeBranch.Tiempo, 6, GamePhase.Phase3,
            UpgradeEffect.IncreaseRoundDuration, 42f);

        Add("tiempo_7", "Receptor frecuencias", "Señal profunda añade +5s al timer",
            130000, UpgradeBranch.Tiempo, 7, GamePhase.Phase3,
            UpgradeEffect.BonusTimeOnDeepSignal, 5f);

        Add("tiempo_8", "Módulo temporal VI", "Duración ronda: 50s",
            180000, UpgradeBranch.Tiempo, 8, GamePhase.Phase3,
            UpgradeEffect.IncreaseRoundDuration, 50f);

        Add("tiempo_9", "Módulo temporal VII", "Duración ronda: 55s",
            150000, UpgradeBranch.Tiempo, 9, GamePhase.Phase4,
            UpgradeEffect.IncreaseRoundDuration, 55f);

        Add("tiempo_10", "Módulo temporal VIII", "Duración ronda: 60s",
            168000, UpgradeBranch.Tiempo, 10, GamePhase.Phase4,
            UpgradeEffect.IncreaseRoundDuration, 60f);

        // RAMA SEÑALES
        Add("senales_1", "Array I",
            "Ruido: 5 · Agrupada: 1",
            200, UpgradeBranch.Senales, 1, GamePhase.Phase1,
            UpgradeEffect.IncreaseSignalLimit, 1f);

        Add("senales_2", "Procesador I",
            "Ruido instantáneo · Agrupada: 0.5s",
            300, UpgradeBranch.Senales, 2, GamePhase.Phase1,
            UpgradeEffect.ReduceAnalysisTime, 1f);

        Add("senales_3", "Amplificador datos I",
            "Todos los datos ×2",
            500, UpgradeBranch.Senales, 3, GamePhase.Phase1,
            UpgradeEffect.MultiplyData, 2f);

        Add("senales_4", "Array II",
            "Débil: +2 · Eco: +1",
            3000, UpgradeBranch.Senales, 4, GamePhase.Phase2,
            UpgradeEffect.IncreaseSignalLimit, 2f);

        Add("senales_5", "Procesador II",
            "Débil: 1s · Eco: 1s · Agrupada: 0s",
            5000, UpgradeBranch.Senales, 5, GamePhase.Phase2,
            UpgradeEffect.ReduceAnalysisTime, 2f);

        Add("senales_6", "Array III",
            "Ruido: +2 · Débil: +1 · Fragmentada: +1",
            8000, UpgradeBranch.Senales, 6, GamePhase.Phase2,
            UpgradeEffect.IncreaseSignalLimit, 3f);

        Add("senales_7", "Amplificador datos II",
            "Todos los datos ×2 -> ×4 total",
            14000, UpgradeBranch.Senales, 7, GamePhase.Phase2,
            UpgradeEffect.MultiplyData, 2f);

        Add("senales_8", "Procesador III",
            "Media: 1.5s · Atraída: 1.5s · Débil: 0s",
            70000, UpgradeBranch.Senales, 8, GamePhase.Phase3,
            UpgradeEffect.ReduceAnalysisTime, 3f);

        Add("senales_9", "Array IV",
            "Media: +2 · Atraída: +1 · Anomalía: +1",
            110000, UpgradeBranch.Senales, 9, GamePhase.Phase3,
            UpgradeEffect.IncreaseSignalLimit, 4f);

        Add("senales_10", "Procesador IV",
            "Fragmentada: 1s · Anomalía: 3s · Eco: 0s",
            160000, UpgradeBranch.Senales, 10, GamePhase.Phase3,
            UpgradeEffect.ReduceAnalysisTime, 4f);

        Add("senales_11", "Amplificador datos III",
            "Todos los datos ×2 -> ×8 total",
            220000, UpgradeBranch.Senales, 11, GamePhase.Phase3,
            UpgradeEffect.MultiplyData, 2f);

        Add("senales_12", "Array V",
            "Fuerte: +2 · Biomasa: +1 · Profunda: activa",
            200000, UpgradeBranch.Senales, 12, GamePhase.Phase4,
            UpgradeEffect.IncreaseSignalLimit, 5f);

        Add("senales_13", "Procesador V",
            "Fuerte: 2s · Media: 0s · Atraída: 0.5s",
            280000, UpgradeBranch.Senales, 13, GamePhase.Phase4,
            UpgradeEffect.ReduceAnalysisTime, 5f);

        Add("senales_14", "Array VI",
            "Fuerte: +2 · Biomasa: +1 · Desconocida: +1",
            350000, UpgradeBranch.Senales, 14, GamePhase.Phase4,
            UpgradeEffect.IncreaseSignalLimit, 6f);

        Add("senales_15", "Procesador VI",
            "Biomasa: 2s · Profunda: 5s · Fuerte: 0.5s",
            430000, UpgradeBranch.Senales, 15, GamePhase.Phase4,
            UpgradeEffect.ReduceAnalysisTime, 6f);

        Add("senales_16", "Array VII",
            "Máximo de todas las rarezas",
            500000, UpgradeBranch.Senales, 16, GamePhase.Phase4,
            UpgradeEffect.IncreaseSignalLimit, 7f);

        // RAMA CURSOR 
        Add("cursor_1", "Cursor circular I", "Radio: 0.2u",
            200, UpgradeBranch.Cursor, 1, GamePhase.Phase1,
            UpgradeEffect.IncreaseCursorRadius, 0.2f);

        Add("cursor_2", "Cursor circular II", "Radio: 0.4u",
            300, UpgradeBranch.Cursor, 2, GamePhase.Phase1,
            UpgradeEffect.IncreaseCursorRadius, 0.4f);

        Add("cursor_3", "Expansión campo I", "Radio: 0.6u",
            6000, UpgradeBranch.Cursor, 3, GamePhase.Phase2,
            UpgradeEffect.IncreaseCursorRadius, 0.6f);

        Add("cursor_4", "Expansión campo II", "Radio: 0.8u",
            14000, UpgradeBranch.Cursor, 4, GamePhase.Phase2,
            UpgradeEffect.IncreaseCursorRadius, 0.8f);

        Add("cursor_5", "Expansión campo III", "Radio: 1.1u",
            100000, UpgradeBranch.Cursor, 5, GamePhase.Phase3,
            UpgradeEffect.IncreaseCursorRadius, 1.1f);

        Add("cursor_6", "Expansión campo IV", "Radio: 1.4u",
            170000, UpgradeBranch.Cursor, 6, GamePhase.Phase3,
            UpgradeEffect.IncreaseCursorRadius, 1.4f);

        Add("cursor_7", "Expansión campo V", "Radio: 1.7u",
            150000, UpgradeBranch.Cursor, 7, GamePhase.Phase4,
            UpgradeEffect.IncreaseCursorRadius, 1.7f);

        Add("cursor_8", "Expansión campo VI", "Radio: 2.0u — máximo",
            150000, UpgradeBranch.Cursor, 8, GamePhase.Phase4,
            UpgradeEffect.IncreaseCursorRadius, 2.0f);
    }

    void Add(string id, string nombre, string descripcion,
             double coste, UpgradeBranch rama, int nivel,
             GamePhase fase, UpgradeEffect efecto, float valor)
    {
        allUpgrades.Add(new UpgradeData(id, nombre, descripcion,
                        coste, rama, nivel, fase, efecto, valor));
    }

    // Gates y bloqueos

    public void UnlockBranch(UpgradeBranch branch)
    {
        unlockedBranches[branch] = true;
    }

    public void UnlockAllBranches()
    {
        foreach (UpgradeBranch branch in
                 System.Enum.GetValues(typeof(UpgradeBranch)))
            unlockedBranches[branch] = true;
    }

    public bool IsBranchUnlocked(UpgradeBranch branch)
        => unlockedBranches.ContainsKey(branch) && unlockedBranches[branch];

    // Compra

    public bool TryBuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade.comprada) return false;
        if (!upgrade.IsAvailable(this)) return false;
        if (!GameManager.Instance.SpendData(upgrade.coste)) return false;

        upgrade.comprada = true;
        ApplyEffect(upgrade);

        Debug.Log($"Comprada: {upgrade.nombre}");
        return true;
    }

    void ApplyEffect(UpgradeData upgrade)
    {
        switch (upgrade.efecto)
        {
            case UpgradeEffect.IncreaseSweepSpeed:
                RadarController.Instance.SetSweepSpeed(upgrade.valorEfecto);
                break;

            case UpgradeEffect.AddRing:
                RadarController.Instance.SetRingCount(
                    (int)upgrade.valorEfecto);
                break;

            case UpgradeEffect.EnableSecondSweep:
                RadarController.Instance.EnableSecondSweep();
                break;

            case UpgradeEffect.UnlockPhase:
                ApplyPhaseUnlock((int)upgrade.valorEfecto, upgrade);
                break;

            case UpgradeEffect.IncreaseRoundDuration:
                GameManager.Instance.roundDuration = upgrade.valorEfecto;
                break;

            case UpgradeEffect.BonusTimeOnAnomaly:
                bonusTimeAnomaly = upgrade.valorEfecto;
                break;

            case UpgradeEffect.BonusTimeOnDeepSignal:
                bonusTimeDeepSignal = upgrade.valorEfecto;
                break;

            case UpgradeEffect.IncreaseSignalLimit:
                ApplySignalLimitIncrease((int)upgrade.valorEfecto);
                break;

            case UpgradeEffect.ReduceAnalysisTime:
                ApplyAnalysisTimeReduction((int)upgrade.valorEfecto);
                break;

            case UpgradeEffect.MultiplyData:
                GameManager.Instance.dataMultiplier *= upgrade.valorEfecto;
                break;

            case UpgradeEffect.IncreaseCursorRadius:
                SignalAnalyzer.Instance.SetCursorRadius(upgrade.valorEfecto);
                break;
        }
    }

    void ApplyPhaseUnlock(int phase, UpgradeData upgrade)
    {
        switch (phase)
        {
            case 1:
                TutorialManager.Instance?.OnTutorialUpgradeBought();
                break;

            case 2:
                GameManager.Instance.SetPhase(GamePhase.Phase2);
                UnlockAllBranches();
                SignalManager.Instance.SetLimit(SignalType.WeakSignal, 2);
                SignalManager.Instance.SetLimit(SignalType.Echo, 1);
                SignalManager.Instance.SetLimit(SignalType.Fragmented, 1);
                break;

            case 3:
                GameManager.Instance.SetPhase(GamePhase.Phase3);
                RadarController.Instance.EnableSecondSweep();
                SignalManager.Instance.SetLimit(SignalType.MediumSignal, 2);
                SignalManager.Instance.SetLimit(SignalType.AttractedSignal, 1);
                SignalManager.Instance.SetLimit(SignalType.Anomaly, 1);
                break;

            case 4:
                GameManager.Instance.SetPhase(GamePhase.Phase4);
                SignalManager.Instance.SetLimit(SignalType.StrongSignal, 2);
                SignalManager.Instance.SetLimit(SignalType.Biomass, 1);
                SignalManager.Instance.SetLimit(SignalType.DeepSignal, 1);
                break;
        }
    }

    void ApplySignalLimitIncrease(int level)
    {
        switch (level)
        {
            case 1:
                SignalManager.Instance.AddToLimit(SignalType.CosmicNoise, 2);
                SignalManager.Instance.AddToLimit(SignalType.GroupedSignal, 1);
                break;
            case 2:
                SignalManager.Instance.AddToLimit(SignalType.WeakSignal, 2);
                SignalManager.Instance.AddToLimit(SignalType.Echo, 1);
                break;
            case 3:
                SignalManager.Instance.AddToLimit(SignalType.CosmicNoise, 2);
                SignalManager.Instance.AddToLimit(SignalType.WeakSignal, 1);
                SignalManager.Instance.AddToLimit(SignalType.Fragmented, 1);
                break;
            case 4:
                SignalManager.Instance.AddToLimit(SignalType.MediumSignal, 2);
                SignalManager.Instance.AddToLimit(SignalType.AttractedSignal, 1);
                SignalManager.Instance.AddToLimit(SignalType.Anomaly, 1);
                break;
            case 5:
                SignalManager.Instance.AddToLimit(SignalType.StrongSignal, 2);
                SignalManager.Instance.AddToLimit(SignalType.Biomass, 1);
                break;
            case 6:
                SignalManager.Instance.AddToLimit(SignalType.StrongSignal, 2);
                SignalManager.Instance.AddToLimit(SignalType.Biomass, 1);
                break;
            case 7:
                SignalManager.Instance.AddToLimit(SignalType.CosmicNoise, 10);
                SignalManager.Instance.AddToLimit(SignalType.WeakSignal, 5);
                SignalManager.Instance.AddToLimit(SignalType.MediumSignal, 3);
                SignalManager.Instance.AddToLimit(SignalType.StrongSignal, 3);
                break;
        }
    }

    void ApplyAnalysisTimeReduction(int level)
    {
        switch (level)
        {
            case 1:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.CosmicNoise, 0f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.GroupedSignal, 0.5f);
                break;
            case 2:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.WeakSignal, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.Echo, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.GroupedSignal, 0f);
                break;
            case 3:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.MediumSignal, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.AttractedSignal, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.WeakSignal, 0f);
                break;
            case 4:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.Fragmented, 0.33f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.Anomaly, 0.6f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.Echo, 0f);
                break;
            case 5:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.StrongSignal, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.MediumSignal, 0f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.AttractedSignal, 0.17f);
                break;
            case 6:
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.Biomass, 0.5f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.DeepSignal, 0.625f);
                SignalManager.Instance.SetAnalysisModifier(
                    SignalType.StrongSignal, 0.125f);
                break;
        }
    }

    // Bonus de tiempo

    public float GetBonusTimeForSignal(SignalType type)
    {
        if (type == SignalType.Anomaly) return bonusTimeAnomaly;
        if (type == SignalType.DeepSignal) return bonusTimeDeepSignal;
        return 0f;
    }

    // Consultas

    public UpgradeData GetUpgrade(UpgradeBranch branch, int nivel)
        => allUpgrades.Find(u => u.rama == branch && u.nivel == nivel);

    public List<UpgradeData> GetBranchUpgrades(UpgradeBranch branch)
        => allUpgrades.FindAll(u => u.rama == branch);

    public List<UpgradeData> GetVisibleUpgrades(UpgradeBranch branch)
    {
        List<UpgradeData> branchUpgrades = GetBranchUpgrades(branch);
        List<UpgradeData> visible = new List<UpgradeData>();
        int uncompradaCount = 0;

        foreach (UpgradeData u in branchUpgrades)
        {
            if (u.comprada)
            {
                visible.Add(u);
            }
            else if (uncompradaCount < 2)
            {
                visible.Add(u);
                uncompradaCount++;
            }
            else
            {
                break;
            }
        }

        return visible;
    }

    public bool AreAllBranchUpgradesBought(UpgradeBranch branch)
    {
        return GetBranchUpgrades(branch).TrueForAll(u => u.comprada);
    }
}