using UnityEngine;

public enum UpgradeBranch
{
    Radar,
    Tiempo,
    Senales,
    Cursor
}

public enum UpgradeEffect
{
    // Radar
    AddRing,
    IncreaseSweepSpeed,
    EnableSecondSweep,
    UnlockPhase,

    // Tiempo
    IncreaseRoundDuration,
    BonusTimeOnAnomaly,
    BonusTimeOnDeepSignal,

    // Señales y análisis
    IncreaseSignalLimit,
    ReduceAnalysisTime,
    MultiplyData,

    // Cursor
    IncreaseCursorRadius
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
    public GamePhase faseRequerida;
    public UpgradeEffect efecto;
    public float valorEfecto;

    // Estado
    public bool comprada = false;

    public UpgradeData(string id, string nombre, string descripcion,
                       double coste, UpgradeBranch rama, int nivel,
                       GamePhase faseRequerida, UpgradeEffect efecto,
                       float valorEfecto)
    {
        this.id = id;
        this.nombre = nombre;
        this.descripcion = descripcion;
        this.coste = coste;
        this.rama = rama;
        this.nivel = nivel;
        this.faseRequerida = faseRequerida;
        this.efecto = efecto;
        this.valorEfecto = valorEfecto;
    }

    public bool IsAvailable(UpgradeManager manager)
    {
        if (comprada) return false;
        if (!manager.IsBranchUnlocked(rama)) return false;

        // Nivel 1 disponible si la rama está desbloqueada
        if (nivel == 1) return true;

        // Niveles superiores requieren el anterior comprado
        UpgradeData previous = manager.GetUpgrade(rama, nivel - 1);
        return previous != null && previous.comprada;
    }

    public bool CanAfford()
    {
        return GameManager.Instance.scanData >= coste;
    }
}