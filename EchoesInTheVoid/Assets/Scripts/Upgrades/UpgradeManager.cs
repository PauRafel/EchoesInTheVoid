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
    {
        return GameManager.Instance.scanData >= coste;
    }
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private List<UpgradeData> allUpgrades = new List<UpgradeData>();

    private Dictionary<UpgradeBranch, bool> unlockedBranches =
        new Dictionary<UpgradeBranch, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        Add("tiempo_1", "Modulo I",
            "Ronda +1 segundo",
            60, UpgradeBranch.Tiempo, 1); //1
        Add("tiempo_2", "Modulo II",
            "Ronda +1 segundo",
            100, UpgradeBranch.Tiempo, 2); //2
        Add("tiempo_3", "Modulo de tier",
            "+3 segundos al subir de tier",
            2100, UpgradeBranch.Tiempo, 3); //16
        Add("tiempo_4", "Receptor I",
            "+5% prob de +0.1s al analizar",
            3600, UpgradeBranch.Tiempo, 4); //21
        Add("tiempo_5", "Receptor II",
            "+5% prob de +0.1s al analizar (10% total)",
            4300, UpgradeBranch.Tiempo, 5); //26

        // RAMA CURSOR
        Add("cursor_1", "Expansion I", "Cursor +30%", 80, UpgradeBranch.Cursor, 1); //3
        Add("cursor_2", "Expansion II", "Cursor +45%", 100, UpgradeBranch.Cursor, 2); //4
        Add("cursor_3", "Expansion III", "Cursor +25%", 120, UpgradeBranch.Cursor, 3); //5
        Add("cursor_4", "Expansion IV", "Cursor +50%", 300, UpgradeBranch.Cursor, 4); //9
        Add("cursor_5", "Expansion V", "Cursor +25%", 550, UpgradeBranch.Cursor, 5); //13
        Add("cursor_6", "Expansion VI", "Cursor +50%", 2600, UpgradeBranch.Cursor, 6); //18
        Add("cursor_7", "Expansion VII", "Cursor +25%", 5200, UpgradeBranch.Cursor, 7); //34
        Add("cursor_8", "Expansion VIII", "Cursor +25%", 5300, UpgradeBranch.Cursor, 8); //35
        Add("cursor_9", "Expansion IX", "Cursor +30%", 6000, UpgradeBranch.Cursor, 9); //39
        Add("cursor_10", "Expansion X", "Cursor +30%", 6200, UpgradeBranch.Cursor, 10); //42
        Add("cursor_11", "Expansion XI", "Cursor +15%", 6400, UpgradeBranch.Cursor, 11); //44

        // RAMA VELOCIDAD ANALISIS
        Add("analisis_1", "Procesador I", "Velocidad analisis +15%", 130, UpgradeBranch.VelocidadAnalisis, 1); //6
        Add("analisis_2", "Procesador II", "Velocidad analisis +20%", 300, UpgradeBranch.VelocidadAnalisis, 2); //10
        Add("analisis_3", "Procesador III", "Velocidad analisis +50%", 750, UpgradeBranch.VelocidadAnalisis, 3); //14
        Add("analisis_4", "Critico I", "+5% prob critico", 2100, UpgradeBranch.VelocidadAnalisis, 4); //17
        Add("analisis_5", "Critico II", "+10% prob critico (15% total)", 3000, UpgradeBranch.VelocidadAnalisis, 5); //20
        Add("analisis_6", "Procesador IV", "Velocidad analisis +15%", 3900, UpgradeBranch.VelocidadAnalisis, 6); //23
        Add("analisis_7", "Procesador V", "Velocidad analisis +15%", 4200, UpgradeBranch.VelocidadAnalisis, 7); //25
        Add("analisis_8", "Critico III", "+10% prob critico (25% total)", 4500, UpgradeBranch.VelocidadAnalisis, 8); //27
        Add("analisis_9", "Critico IV", "Critico +20% dano", 5100, UpgradeBranch.VelocidadAnalisis, 9); //33
        Add("analisis_10", "Procesador VI", "Velocidad analisis +15%", 5900, UpgradeBranch.VelocidadAnalisis, 10); //38
        Add("analisis_11", "Procesador VII", "Velocidad analisis +15%", 6200, UpgradeBranch.VelocidadAnalisis, 11); //41
        Add("analisis_12", "Procesador VIII", "Velocidad analisis +25%", 6500, UpgradeBranch.VelocidadAnalisis, 12); //45

        // RAMA CANTIDAD SENALES
        Add("cantidad_1", "Array I",
            "+30 senales basicas (20 a 50)",
            170, UpgradeBranch.CantidadSenales, 1); //7
        Add("cantidad_2", "Array II",
            "+50 senales basicas (50 a 100)",
            500, UpgradeBranch.CantidadSenales, 2); //12
        Add("cantidad_3", "Detector tier I",
            "+25% senales extra al subir tier",
            2650, UpgradeBranch.CantidadSenales, 3); //19
        Add("cantidad_4", "Detector tier II",
            "+25% senales extra al subir tier (50% total)",
            4600, UpgradeBranch.CantidadSenales, 4); //28
        Add("cantidad_5", "Generador I",
            "+30% prob de señal extra al analizar",
            4700, UpgradeBranch.CantidadSenales, 5); //29
        Add("cantidad_6", "Generador II",
            "+40% prob de señal extra al analizar (70% total)",
            4900, UpgradeBranch.CantidadSenales, 6); //31

        // RAMA TAMANO SENALES
        Add("tamano_1", "Masa I",
            "50% de senales son dobles",
            800, UpgradeBranch.TamanoSenales, 1); //15
        Add("tamano_2", "Masa II",
            "1/3 simple 1/3 doble 1/3 triple",
            3750, UpgradeBranch.TamanoSenales, 2); //22
        Add("tamano_3", "Enhanced I",
            "20% de senales son version mejorada",
            4000, UpgradeBranch.TamanoSenales, 3); //24
        Add("tamano_4", "Enhanced II",
            "Version mejorada +10% datos",
            5600, UpgradeBranch.TamanoSenales, 4); //36
        Add("tamano_5", "Enhanced III",
            "Version mejorada +10% datos (20% total)",
            6100, UpgradeBranch.TamanoSenales, 5); //40
        Add("tamano_6", "Enhanced IV",
            "Version mejorada +10% datos (30% total)",
            6300, UpgradeBranch.TamanoSenales, 6); //43

        // RAMA SWEEP
        Add("sweep_1", "Amplificador I", "Sweep +25%", 250, UpgradeBranch.Sweep, 1); //8
        Add("sweep_2", "Amplificador II", "Sweep +25%", 400, UpgradeBranch.Sweep, 2); //11
        Add("sweep_3", "Amplificador III", "Sweep +25%", 4800, UpgradeBranch.Sweep, 3); //30
        Add("sweep_4", "Amplificador IV", "Sweep +10%", 5000, UpgradeBranch.Sweep, 4); //32
        Add("sweep_5", "Amplificador V", "Sweep +15%", 5700, UpgradeBranch.Sweep, 5); //37
    }

    void Add(string id, string nombre, string descripcion,
             double coste, UpgradeBranch rama, int nivel)
    {
        allUpgrades.Add(new UpgradeData(
            id, nombre, descripcion, coste, rama, nivel));
    }

    // GATES

    public void UnlockAllBranches()
    {
        foreach (UpgradeBranch branch in
            System.Enum.GetValues(typeof(UpgradeBranch)))
            unlockedBranches[branch] = true;
    }

    public void UnlockBranch(UpgradeBranch branch)
    {
        unlockedBranches[branch] = true;
    }

    public bool IsBranchUnlocked(UpgradeBranch branch)
    {
        return unlockedBranches.ContainsKey(branch) &&
               unlockedBranches[branch];
    }

    // COMPRA

    public bool TryBuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade.comprada) return false;
        if (!upgrade.IsAvailable(this)) return false;
        if (!GameManager.Instance.SpendData(upgrade.coste)) return false;

        upgrade.comprada = true;
        ApplyEffect(upgrade);
        Debug.Log("Comprada: " + upgrade.nombre);
        return true;
    }

    void ApplyEffect(UpgradeData upgrade)
    {
        switch (upgrade.id)
        {
            // TIEMPO
            case "tiempo_1":
            case "tiempo_2":
                GameManager.Instance.roundDuration += 1f;
                break;
            case "tiempo_3":
                GameManager.Instance.bonusTimeOnTier += 3f;
                break;
            case "tiempo_4":
                GameManager.Instance.bonusTimeOnAnalysisChance += 0.05f;
                break;
            case "tiempo_5":
                GameManager.Instance.bonusTimeOnAnalysisChance += 0.05f;
                break;

            // CURSOR
            case "cursor_1": ApplyCursorPercent(1.30f); break;
            case "cursor_2": ApplyCursorPercent(1.75f); break;
            case "cursor_3": ApplyCursorPercent(2.00f); break;
            case "cursor_4": ApplyCursorPercent(2.50f); break;
            case "cursor_5": ApplyCursorPercent(2.75f); break;
            case "cursor_6": ApplyCursorPercent(3.25f); break;
            case "cursor_7": ApplyCursorPercent(3.50f); break;
            case "cursor_8": ApplyCursorPercent(3.75f); break;
            case "cursor_9": ApplyCursorPercent(4.05f); break;
            case "cursor_10": ApplyCursorPercent(4.35f); break;
            case "cursor_11": ApplyCursorPercent(4.45f); break;

            // VELOCIDAD ANALISIS
            case "analisis_1": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_2": GameManager.Instance.analysisSpeedPercent += 20f; break;
            case "analisis_3": GameManager.Instance.analysisSpeedPercent += 50f; break;
            case "analisis_4": GameManager.Instance.criticalChance += 0.05f; break;
            case "analisis_5": GameManager.Instance.criticalChance += 0.10f; break;
            case "analisis_6": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_7": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_8": GameManager.Instance.criticalChance += 0.10f; break;
            case "analisis_9": GameManager.Instance.criticalBonus += 20f; break;
            case "analisis_10": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_11": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_12": GameManager.Instance.analysisSpeedPercent += 25f; break;

            // CANTIDAD SENALES
            case "cantidad_1":
                SignalManager.Instance.SetBaseSignalCount(50);
                break;
            case "cantidad_2":
                SignalManager.Instance.SetBaseSignalCount(100);
                break;
            case "cantidad_3":
                SignalManager.Instance.SetExtraSignalsOnTierPercent(25);
                break;
            case "cantidad_4":
                SignalManager.Instance.SetExtraSignalsOnTierPercent(50);
                break;
            case "cantidad_5":
                SignalManager.Instance.SetChanceExtraOnAnalysis(0.30f);
                break;
            case "cantidad_6":
                SignalManager.Instance.SetChanceExtraOnAnalysis(0.70f);
                break;

            // TAMANO SENALES
            case "tamano_1":
                SignalManager.Instance.SetChanceDouble(0.50f);
                break;
            case "tamano_2":
                SignalManager.Instance.SetChanceDouble(0.333f);
                SignalManager.Instance.SetChanceTriple(0.333f);
                break;
            case "tamano_3":
                SignalManager.Instance.SetChanceEnhanced(0.20f);
                break;
            case "tamano_4":
                SignalManager.Instance.SetEnhancedDataBonus(0.10f);
                break;
            case "tamano_5":
                SignalManager.Instance.SetEnhancedDataBonus(0.20f);
                break;
            case "tamano_6":
                SignalManager.Instance.SetEnhancedDataBonus(0.30f);
                break;

            // SWEEP
            case "sweep_1": ApplySweepPercent(1.25f); break;
            case "sweep_2": ApplySweepPercent(1.50f); break;
            case "sweep_3": ApplySweepPercent(1.75f); break;
            case "sweep_4": ApplySweepPercent(1.85f); break;
            case "sweep_5": ApplySweepPercent(2.00f); break;
        }
    }

    void ApplyCursorPercent(float totalPercent)
    {
        float baseRadius = 0.3f;
        SignalAnalyzer.Instance.SetCursorRadius(baseRadius * totalPercent);
    }

    void ApplySweepPercent(float totalPercent)
    {
        float baseSweep = 45f;
        RadarController.Instance.SetSweepSpeed(baseSweep * totalPercent);
    }

    // CONSULTAS

    public UpgradeData GetUpgrade(UpgradeBranch branch, int nivel)
    {
        return allUpgrades.Find(u => u.rama == branch && u.nivel == nivel);
    }

    public List<UpgradeData> GetBranchUpgrades(UpgradeBranch branch)
    {
        return allUpgrades.FindAll(u => u.rama == branch);
    }

    public List<UpgradeData> GetVisibleUpgrades(UpgradeBranch branch)
    {
        List<UpgradeData> all = GetBranchUpgrades(branch);
        List<UpgradeData> visible = new List<UpgradeData>();
        int uncompradaCount = 0;

        foreach (UpgradeData u in all)
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
}