using UnityEngine;
using System.Collections.Generic;

public enum UpgradeBranch
{
    Tiempo,
    Cursor,
    VelocidadAnalisis,
    CantidadSenales,
    TamanoSenales,
    Sweep
}

[System.Serializable]
public class UpgradeData
{
    public string id;
    public string nombre;
    public string descripcion;
    public double coste;
    public UpgradeBranch rama;
    public int nivel;
    public bool comprada = false;

    public UpgradeData(string id, string nombre, string descripcion,
                       double coste, UpgradeBranch rama, int nivel)
    {
        this.id = id;
        this.nombre = nombre;
        this.descripcion = descripcion;
        this.coste = coste;
        this.rama = rama;
        this.nivel = nivel;
    }

    public bool IsAvailable(UpgradeManager manager)
    {
        if (comprada) return false;
        if (!manager.IsBranchUnlocked(rama)) return false;
        if (nivel == 1) return true;

        UpgradeData previous = manager.GetUpgrade(rama, nivel - 1);
        return previous != null && previous.comprada;
    }

    public bool CanAfford()
        => GameManager.Instance.scanData >= coste;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    private Dictionary<UpgradeBranch, bool> unlockedBranches =
        new Dictionary<UpgradeBranch, bool>();

    // Estado de mejoras activas
    private float bonusTimeOnAnalysisChance = 0f;
    private float bonusTimeOnAnalysisAmount = 0.1f;

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
        // RAMA TIEMPO 
        Add("tiempo_1", "Módulo I",
            "+1s al contador de ronda",
            10, UpgradeBranch.Tiempo, 1);
        Add("tiempo_2", "Módulo II",
            "+1s al contador de ronda",
            10, UpgradeBranch.Tiempo, 2);
        Add("tiempo_3", "Módulo de tier",
            "+3s al subir de tier en la ronda",
            3000, UpgradeBranch.Tiempo, 3);
        Add("tiempo_4", "Receptor temporal I",
            "+5% prob. de +0.1s al analizar señal",
            3500, UpgradeBranch.Tiempo, 4);
        Add("tiempo_5", "Receptor temporal II",
            "+5% prob. de +0.1s (total 10%)",
            4500, UpgradeBranch.Tiempo, 5);

        // RAMA CURSOR 
        Add("cursor_1", "Expansión I", "Cursor 100% -> 130%", 10, UpgradeBranch.Cursor, 1);
        Add("cursor_2", "Expansión II", "Cursor 130% -> 175%", 15, UpgradeBranch.Cursor, 2);
        Add("cursor_3", "Expansión III", "Cursor 175% -> 200%", 20, UpgradeBranch.Cursor, 3);
        Add("cursor_4", "Expansión IV", "Cursor 200% -> 250%", 25, UpgradeBranch.Cursor, 4);
        Add("cursor_5", "Expansión V", "Cursor 250% -> 275%", 30, UpgradeBranch.Cursor, 5);
        Add("cursor_6", "Expansión VI", "Cursor 275% -> 325%", 40, UpgradeBranch.Cursor, 6);
        Add("cursor_7", "Expansión VII", "Cursor 325% -> 350%", 4000, UpgradeBranch.Cursor, 7);
        Add("cursor_8", "Expansión VIII", "Cursor 350% -> 375%", 7000, UpgradeBranch.Cursor, 8);
        Add("cursor_9", "Expansión IX", "Cursor 375% -> 405%", 7500, UpgradeBranch.Cursor, 9);
        Add("cursor_10", "Expansión X", "Cursor 405% -> 435%", 9000, UpgradeBranch.Cursor, 10);
        Add("cursor_11", "Expansión XI", "Cursor 435% -> 445%", 10000, UpgradeBranch.Cursor, 11);

        // RAMA VELOCIDAD ANÁLISIS 
        Add("analisis_1", "Procesador I", "Ruido: 3s -> 2.5s", 30, UpgradeBranch.VelocidadAnalisis, 1);
        Add("analisis_2", "Procesador II", "Ruido: 2.5s -> 2s", 40, UpgradeBranch.VelocidadAnalisis, 2);
        Add("analisis_3", "Procesador III", "Ruido: 2s -> 1s", 180, UpgradeBranch.VelocidadAnalisis, 3);
        Add("analisis_4", "Crítico I", "+5% prob. análisis crítico (-50%)", 150, UpgradeBranch.VelocidadAnalisis, 4);
        Add("analisis_5", "Crítico II", "+10% prob. crítico (total 15%)", 200, UpgradeBranch.VelocidadAnalisis, 5);
        Add("analisis_6", "Procesador IV", "Ruido: 2s -> 1s", 180, UpgradeBranch.VelocidadAnalisis, 6);
        Add("analisis_7", "Procesador V", "Ruido: 1s -> 0.85s", 600, UpgradeBranch.VelocidadAnalisis, 7);
        Add("analisis_8", "Procesador VI", "Ruido: 0.85s -> 0.7s", 800, UpgradeBranch.VelocidadAnalisis, 8);
        Add("analisis_9", "Crítico III", "+10% prob. crítico (total 25%)", 600, UpgradeBranch.VelocidadAnalisis, 9);
        Add("analisis_10", "Crítico IV", "Crítico: -50% -> -55%", 650, UpgradeBranch.VelocidadAnalisis, 10);
        Add("analisis_11", "Procesador VII", "Ruido: 0.7s -> 0.55s", 4000, UpgradeBranch.VelocidadAnalisis, 11);
        Add("analisis_12", "Procesador VIII", "Ruido: 0.55s -> 0.4s", 4750, UpgradeBranch.VelocidadAnalisis, 12);
        Add("analisis_13", "Procesador IX", "Ruido: 0.4s -> 0.25s", 19000, UpgradeBranch.VelocidadAnalisis, 13);

        // RAMA CANTIDAD SEÑALES 
        Add("cantidad_1", "Array I",
            "Ruido: 20 -> 50 señales",
            70, UpgradeBranch.CantidadSenales, 1);
        Add("cantidad_2", "Array II",
            "Ruido: 50 -> 105 señales",
            250, UpgradeBranch.CantidadSenales, 2);
        Add("cantidad_3", "Detector de tier I",
            "+25% señales extra al subir tier",
            3500, UpgradeBranch.CantidadSenales, 3);
        Add("cantidad_4", "Detector de tier II",
            "+50% señales extra al subir tier",
            4250, UpgradeBranch.CantidadSenales, 4);
        Add("cantidad_5", "Generador I",
            "+30% prob. señal extra al analizar",
            6000, UpgradeBranch.CantidadSenales, 5);
        Add("cantidad_6", "Generador II",
            "+40% prob. señal extra (total 70%)",
            20000, UpgradeBranch.CantidadSenales, 6);

        // RAMA TAMAÑO SEÑALES 
        Add("tamano_1", "Masa I",
            "50% señales son dobles (x2 valor y tiempo)",
            200, UpgradeBranch.TamanoSenales, 1);
        Add("tamano_2", "Masa II",
            "1/3 triples, 1/3 dobles, 1/3 simples",
            600, UpgradeBranch.TamanoSenales, 2);
        Add("tamano_3", "Enhanced I",
            "20% señales son versión mejorada (x2 todo)",
            600, UpgradeBranch.TamanoSenales, 3);
        Add("tamano_4", "Enhanced II",
            "Enhanced da +10% datos extra",
            700, UpgradeBranch.TamanoSenales, 4);
        Add("tamano_5", "Enhanced III",
            "Enhanced da +20% datos (total +20%)",
            4250, UpgradeBranch.TamanoSenales, 5);
        Add("tamano_6", "Enhanced IV",
            "Enhanced da +30% datos (total +30%)",
            4250, UpgradeBranch.TamanoSenales, 6);

        // RAMA SWEEP 
        Add("sweep_1", "Amplificador I", "Sweep 100% -> 125%", 300, UpgradeBranch.Sweep, 1);
        Add("sweep_2", "Amplificador II", "Sweep 125% -> 150%", 400, UpgradeBranch.Sweep, 2);
        Add("sweep_3", "Amplificador III", "Sweep 150% -> 175%", 600, UpgradeBranch.Sweep, 3);
        Add("sweep_4", "Amplificador IV", "Sweep 175% -> 185%", 9000, UpgradeBranch.Sweep, 4);
        Add("sweep_5", "Amplificador V", "Sweep 185% -> 195%", 10000, UpgradeBranch.Sweep, 5);
        Add("sweep_6", "Amplificador VI", "Sweep 195% -> 205%", 10500, UpgradeBranch.Sweep, 6);
    }

    void Add(string id, string nombre, string descripcion,
             double coste, UpgradeBranch rama, int nivel)
    {
        allUpgrades.Add(new UpgradeData(id, nombre, descripcion,
                        coste, rama, nivel));
    }

    // Gates 

    public void UnlockAllBranches()
    {
        foreach (UpgradeBranch branch in
                 System.Enum.GetValues(typeof(UpgradeBranch)))
            unlockedBranches[branch] = true;
    }

    public void UnlockBranch(UpgradeBranch branch)
        => unlockedBranches[branch] = true;

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
        switch (upgrade.id)
        {
            // Tiempo 
            case "tiempo_1":
            case "tiempo_2":
                GameManager.Instance.roundDuration += 1f;
                break;
            case "tiempo_3":
                // Se aplica via evento OnTierChanged — registrado en RoundTierHandler
                break;
            case "tiempo_4":
                bonusTimeOnAnalysisChance += 0.05f;
                break;
            case "tiempo_5":
                bonusTimeOnAnalysisChance += 0.05f;
                break;

            // Cursor
            case "cursor_1": SetCursorByPercent(1.30f); break;
            case "cursor_2": SetCursorByPercent(1.75f); break;
            case "cursor_3": SetCursorByPercent(2.00f); break;
            case "cursor_4": SetCursorByPercent(2.50f); break;
            case "cursor_5": SetCursorByPercent(2.75f); break;
            case "cursor_6": SetCursorByPercent(3.25f); break;
            case "cursor_7": SetCursorByPercent(3.50f); break;
            case "cursor_8": SetCursorByPercent(3.75f); break;
            case "cursor_9": SetCursorByPercent(4.05f); break;
            case "cursor_10": SetCursorByPercent(4.35f); break;
            case "cursor_11": SetCursorByPercent(4.45f); break;

            // Velocidad análisis
            case "analisis_1": SetAnalysisTime(2.1f); break;
            case "analisis_2": SetAnalysisTime(1.7f); break;
            case "analisis_3": SetAnalysisTime(0.85f); break;
            case "analisis_4": SignalManager.Instance.SetCriticalChance(0.05f); break;
            case "analisis_5": SignalManager.Instance.SetCriticalChance(0.15f); break;
            case "analisis_6": SetAnalysisTime(0.85f); break;
            case "analisis_7": SetAnalysisTime(0.85f); break;
            case "analisis_8": SetAnalysisTime(0.70f); break;
            case "analisis_9": SignalManager.Instance.SetCriticalChance(0.25f); break;
            case "analisis_10": SignalManager.Instance.SetCriticalMultiplier(0.45f); break;
            case "analisis_11": SetAnalysisTime(0.45f); break;
            case "analisis_12": SetAnalysisTime(0.35f); break;
            case "analisis_13": SetAnalysisTime(0.2f); break;

            // Cantidad señales
            case "cantidad_1": SignalManager.Instance.SetBaseSignalCount(50); break;
            case "cantidad_2": SignalManager.Instance.SetBaseSignalCount(105); break;
            case "cantidad_3": SignalManager.Instance.SetExtraSignalsOnTier(25); break;
            case "cantidad_4": SignalManager.Instance.SetExtraSignalsOnTier(50); break;
            case "cantidad_5": SignalManager.Instance.SetChanceExtraOnAnalysis(0.30f); break;
            case "cantidad_6": SignalManager.Instance.SetChanceExtraOnAnalysis(0.70f); break;

            // Tamaño señales 
            case "tamano_1": SignalManager.Instance.SetChanceDouble(0.50f); break;
            case "tamano_2":
                SignalManager.Instance.SetChanceDouble(0.333f);
                SignalManager.Instance.SetChanceTriple(0.333f);
                break;
            case "tamano_3": SignalManager.Instance.SetChanceEnhanced(0.20f); break;
            case "tamano_4": SignalManager.Instance.SetEnhancedDataBonus(0.10f); break;
            case "tamano_5": SignalManager.Instance.SetEnhancedDataBonus(0.20f); break;
            case "tamano_6": SignalManager.Instance.SetEnhancedDataBonus(0.30f); break;

            // Sweep
            case "sweep_1": SetSweepByPercent(1.25f); break;
            case "sweep_2": SetSweepByPercent(1.50f); break;
            case "sweep_3": SetSweepByPercent(1.75f); break;
            case "sweep_4": SetSweepByPercent(1.85f); break;
            case "sweep_5": SetSweepByPercent(1.95f); break;
            case "sweep_6": SetSweepByPercent(2.05f); break;
        }
    }

    // Helpers de aplicación 

    void SetCursorByPercent(float percent)
    {
        float baseRadius = 0.3f;
        SignalAnalyzer.Instance.SetCursorRadius(baseRadius * percent);
    }

    void SetAnalysisTime(float seconds)
    {
        SignalManager.Instance.baseAnalysisTime = seconds;
    }

    void SetSweepByPercent(float percent)
    {
        float baseSweep = 45f;
        RadarController.Instance.SetSweepSpeed(baseSweep * percent);
    }

    //  Bonus tiempo al analizar 

    public float GetBonusTimeOnAnalysis(SignalData signal)
    {
        if (bonusTimeOnAnalysisChance <= 0f) return 0f;
        if (Random.value < bonusTimeOnAnalysisChance)
            return bonusTimeOnAnalysisAmount;
        return 0f;
    }

    //  Bonus tiempo al subir tier 

    public float GetBonusTimeOnTier()
    {
        UpgradeData u = GetUpgrade(UpgradeBranch.Tiempo, 3);
        return (u != null && u.comprada) ? 3f : 0f;
    }

    // Consultas

    public UpgradeData GetUpgrade(UpgradeBranch branch, int nivel)
        => allUpgrades.Find(u => u.rama == branch && u.nivel == nivel);

    public List<UpgradeData> GetBranchUpgrades(UpgradeBranch branch)
        => allUpgrades.FindAll(u => u.rama == branch);

    public List<UpgradeData> GetVisibleUpgrades(UpgradeBranch branch)
    {
        List<UpgradeData> all = GetBranchUpgrades(branch);
        List<UpgradeData> visible = new List<UpgradeData>();
        int uncompradaCount = 0;

        foreach (UpgradeData u in all)
        {
            if (u.comprada)
                visible.Add(u);
            else if (uncompradaCount < 2)
            {
                visible.Add(u);
                uncompradaCount++;
            }
            else break;
        }

        return visible;
    }
}