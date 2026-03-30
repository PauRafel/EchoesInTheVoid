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
    public int faseRequerida = 1;

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
        if (faseRequerida > (int)GameManager.Instance.currentPhase) return false;
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

    public List<UpgradeData> GetAllUpgradesOrdered()
    {
        return allUpgrades;
    }

    void InitializeUpgrades()
    {

        //FASE 1 ---------------------------------------------------------------------------------

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

        //FASE 2 ---------------------------------------------------------------------------------

        // CURSOR FASE 2
        Add("cursor_f2_1", "Expansion XII", "Cursor +30%", 111500, UpgradeBranch.Cursor, 12, 2);
        Add("cursor_f2_2", "Expansion XIII", "Cursor +45%", 111800, UpgradeBranch.Cursor, 13, 2);
        Add("cursor_f2_3", "Expansion XIV", "Cursor +25%", 111200, UpgradeBranch.Cursor, 14, 2);
        Add("cursor_f2_4", "Expansion XV", "Cursor +50%", 112000, UpgradeBranch.Cursor, 15, 2);
        Add("cursor_f2_5", "Expansion XVI", "Cursor +25%", 1113000, UpgradeBranch.Cursor, 16, 2);
        Add("cursor_f2_6", "Expansion XVII", "Cursor +50%", 1115000, UpgradeBranch.Cursor, 17, 2);
        Add("cursor_f2_7", "Expansion XVIII", "Cursor +25%", 11130000, UpgradeBranch.Cursor, 18, 2);
        Add("cursor_f2_8", "Expansion XIX", "Cursor +25%", 11150000, UpgradeBranch.Cursor, 19, 2);
        Add("cursor_f2_9", "Expansion XX", "Cursor +30%", 11160000, UpgradeBranch.Cursor, 20, 2);
        Add("cursor_f2_10", "Expansion XXI", "Cursor +30%", 11170000, UpgradeBranch.Cursor, 21, 2);
        Add("cursor_f2_11", "Expansion XXII", "Cursor +15%", 11180000, UpgradeBranch.Cursor, 22, 2);

        // TAMANO SEÑALES FASE 2
        Add("tamano_f2_1", "Eco I", "10% señales son eco", 5111000, UpgradeBranch.TamanoSenales, 7, 2);
        Add("tamano_f2_2", "Eco II", "Eco analiza 30% en rango", 8011100, UpgradeBranch.TamanoSenales, 8, 2);
        Add("tamano_f2_3", "Eco III", "Eco analiza 50% en rango", 12111000, UpgradeBranch.TamanoSenales, 9, 2);
        Add("tamano_f2_4", "Rango eco I", "Rango eco +10%", 15111000, UpgradeBranch.TamanoSenales, 10, 2);
        Add("tamano_f2_5", "Cadena I", "+20% prob cadena eco", 20111000, UpgradeBranch.TamanoSenales, 11, 2);
        Add("tamano_f2_6", "Rango eco II", "Rango eco +10%", 25111000, UpgradeBranch.TamanoSenales, 12, 2);
        Add("tamano_f2_7", "Cuadruple I", "5% señales cuadruples", 30011100, UpgradeBranch.TamanoSenales, 13, 2);
        Add("tamano_f2_8", "Cadena II", "+20% prob cadena eco", 35111000, UpgradeBranch.TamanoSenales, 14, 2);
        Add("tamano_f2_9", "Cuadruple II", "10% señales cuadruples", 40111000, UpgradeBranch.TamanoSenales, 15, 2);
        Add("tamano_f2_10", "Cuadruple III", "15% señales cuadruples", 51110000, UpgradeBranch.TamanoSenales, 16, 2);
        Add("tamano_f2_11", "Superenhanced I", "10% enhanced son superenhanced", 55011100, UpgradeBranch.TamanoSenales, 17, 2);
        Add("tamano_f2_12", "Cuadruple IV", "20% señales cuadruples", 60111000, UpgradeBranch.TamanoSenales, 18, 2);
        Add("tamano_f2_13", "Superenhanced II", "Super enhanced +20% datos", 70111000, UpgradeBranch.TamanoSenales, 19, 2);
        Add("tamano_f2_14", "Superenhanced III", "Super enhanced +20% datos", 80111000, UpgradeBranch.TamanoSenales, 20, 2);

        // VELOCIDAD ANALISIS FASE 2
        Add("analisis_f2_1", "Procesador X", "Velocidad analisis +15%", 1115000, UpgradeBranch.VelocidadAnalisis, 13, 2);
        Add("analisis_f2_2", "Procesador XI", "Velocidad analisis +20%", 1118000, UpgradeBranch.VelocidadAnalisis, 14, 2);
        Add("analisis_f2_3", "Critico V", "Dano critico +20%", 11112000, UpgradeBranch.VelocidadAnalisis, 15, 2);
        Add("analisis_f2_4", "Critico VI", "Prob critico +10%", 11115000, UpgradeBranch.VelocidadAnalisis, 16, 2);
        Add("analisis_f2_5", "Critico VII", "Dano critico +20%", 21110000, UpgradeBranch.VelocidadAnalisis, 17, 2);
        Add("analisis_f2_6", "Procesador XII", "Velocidad analisis +10%", 21115000, UpgradeBranch.VelocidadAnalisis, 18, 2);
        Add("analisis_f2_7", "Critico VIII", "Prob critico +5%", 31110000, UpgradeBranch.VelocidadAnalisis, 19, 2);

        // CANTIDAD SEÑALES FASE 2
        Add("cantidad_f2_1", "Array III", "+100 señales", 5111000, UpgradeBranch.CantidadSenales, 7, 2);
        Add("cantidad_f2_2", "Generador III", "+10% prob señal extra analisis", 10011100, UpgradeBranch.CantidadSenales, 8, 2);
        Add("cantidad_f2_3", "Array IV", "+100 señales", 21110000, UpgradeBranch.CantidadSenales, 9, 2);
        Add("cantidad_f2_4", "Detector tier III", "+50% señales extra en tier", 35011100, UpgradeBranch.CantidadSenales, 10, 2);

        // SWEEP FASE 2
        Add("sweep_f2_1", "Amplificador VII", "Sweep +15%", 5111000, UpgradeBranch.Sweep, 6, 2);
        Add("sweep_f2_2", "Amplificador VIII", "Sweep +15%", 10111000, UpgradeBranch.Sweep, 7, 2);
        Add("sweep_f2_2", "Amplificador VIII", "Sweep +15%", 10111000, UpgradeBranch.Sweep, 7, 2);
    }

    void Add(string id, string nombre, string descripcion,
         double coste, UpgradeBranch rama, int nivel, int fase = 1)
    {
        UpgradeData u = new UpgradeData(
            id, nombre, descripcion, coste, rama, nivel);
        u.faseRequerida = fase;
        allUpgrades.Add(u);
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
            // CURSOR FASE 2
            case "cursor_f2_1": ApplyCursorPercent(1.30f); break;
            case "cursor_f2_2": ApplyCursorPercent(1.75f); break;
            case "cursor_f2_3": ApplyCursorPercent(2.00f); break;
            case "cursor_f2_4": ApplyCursorPercent(2.50f); break;
            case "cursor_f2_5": ApplyCursorPercent(2.75f); break;
            case "cursor_f2_6": ApplyCursorPercent(3.25f); break;
            case "cursor_f2_7": ApplyCursorPercent(3.50f); break;
            case "cursor_f2_8": ApplyCursorPercent(3.75f); break;
            case "cursor_f2_9": ApplyCursorPercent(4.05f); break;
            case "cursor_f2_10": ApplyCursorPercent(4.35f); break;
            case "cursor_f2_11": ApplyCursorPercent(4.45f); break;


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
            // VELOCIDAD ANALISIS FASE 2
            case "analisis_f2_1": GameManager.Instance.analysisSpeedPercent += 15f; break;
            case "analisis_f2_2": GameManager.Instance.analysisSpeedPercent += 20f; break;
            case "analisis_f2_3": GameManager.Instance.criticalBonus += 20f; break;
            case "analisis_f2_4": GameManager.Instance.criticalChance += 0.10f; break;
            case "analisis_f2_5": GameManager.Instance.criticalBonus += 20f; break;
            case "analisis_f2_6": GameManager.Instance.analysisSpeedPercent += 10f; break;
            case "analisis_f2_7": GameManager.Instance.criticalChance += 0.05f; break;


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
            // CANTIDAD FASE 2
            case "cantidad_f2_1":
                SignalManager.Instance.SetBaseSignalCount(
                    SignalManager.Instance.baseSignalCount + 100);
                break;
            case "cantidad_f2_2":
                SignalManager.Instance.SetChanceExtraOnAnalysis(
                    SignalManager.Instance.GetChanceExtraOnAnalysis() + 0.10f);
                break;
            case "cantidad_f2_3":
                SignalManager.Instance.SetBaseSignalCount(
                    SignalManager.Instance.baseSignalCount + 100);
                break;
            case "cantidad_f2_4":
                SignalManager.Instance.SetExtraSignalsOnTierPercent(
                    SignalManager.Instance.GetExtraSignalsOnTierPercent() + 50);
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
            // TAMANO FASE 2
            case "tamano_f2_1": SignalManager.Instance.SetEchoAnalyzePercent(0.10f); break;
            case "tamano_f2_2": SignalManager.Instance.SetEchoAnalyzePercent(0.30f); break;
            case "tamano_f2_3": SignalManager.Instance.SetEchoAnalyzePercent(0.50f); break;
            case "tamano_f2_4": SignalManager.Instance.SetEchoRangeMultiplier(1.10f); break;
            case "tamano_f2_5": SignalManager.Instance.SetEchoChainChance(0.20f); break;
            case "tamano_f2_6": SignalManager.Instance.SetEchoRangeMultiplier(1.20f); break;
            case "tamano_f2_7": SignalManager.Instance.SetChanceQuadruple(0.05f); break;
            case "tamano_f2_8": SignalManager.Instance.SetEchoChainChance(0.40f); break;
            case "tamano_f2_9": SignalManager.Instance.SetChanceQuadruple(0.10f); break;
            case "tamano_f2_10": SignalManager.Instance.SetChanceQuadruple(0.15f); break;
            case "tamano_f2_11": SignalManager.Instance.SetSuperEnhancedChance(0.10f); break;
            case "tamano_f2_12": SignalManager.Instance.SetChanceQuadruple(0.20f); break;
            case "tamano_f2_13": SignalManager.Instance.SetSuperEnhancedBonus(0.70f); break;
            case "tamano_f2_14": SignalManager.Instance.SetSuperEnhancedBonus(0.90f); break;


            // SWEEP
            case "sweep_1": ApplySweepPercent(1.25f); break;
            case "sweep_2": ApplySweepPercent(1.50f); break;
            case "sweep_3": ApplySweepPercent(1.75f); break;
            case "sweep_4": ApplySweepPercent(1.85f); break;
            case "sweep_5": ApplySweepPercent(2.00f); break;
            // SWEEP FASE 2
            case "sweep_f2_1": ApplySweepPercent(2.15f); break;
            case "sweep_f2_2": ApplySweepPercent(2.30f); break;
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

    public void UnlockPhase2Branches()
    {
        UnlockAllBranches();
        Debug.Log("Ramas fase 2 desbloqueadas");
    }
}