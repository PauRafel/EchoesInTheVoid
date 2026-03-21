using UnityEngine;

public enum SignalType
{
    CosmicNoise,      // Ruido cósmico base
    CosmicNoiseDouble,// Ruido doble (M1 tamaño)
    CosmicNoiseTriple,// Ruido triple (M2 tamaño)
    Enhanced          // Versión mejorada x2 de cualquier señal (M3 tamaño)
}

public enum SignalState
{
    Hidden,
    Revealed,
    Analyzing,
    Completed
}

public enum SignalTier
{
    Normal,   // señal base
    Double,   // x2 valor y tiempo
    Triple,   // x3 valor y tiempo
    Enhanced  // x2 de su base (sea normal, double o triple)
}

[System.Serializable]
public class SignalData
{
    [Header("Tipo y estado")]
    public SignalType type;
    public SignalTier tier = SignalTier.Normal;
    public SignalState state = SignalState.Hidden;

    [Header("Posición")]
    public Vector2 position;
    public float signalAngle;

    [Header("Valores")]
    public double dataReward;
    public float analysisTime;
    public float analysisProgress = 0f;
    public float baseScale = 1f;

    [Header("Visual")]
    public GameObject visualObject;

    // Helpers 

    public bool IsHidden() => state == SignalState.Hidden;
    public bool IsRevealed() => state == SignalState.Revealed;
    public bool IsAnalyzing() => state == SignalState.Analyzing;
    public bool IsCompleted() => state == SignalState.Completed;

    public float GetProgressRatio()
    {
        if (analysisTime <= 0f) return 1f;
        return Mathf.Clamp01(analysisProgress / analysisTime);
    }

    public bool IsSpecial() => false; // En fase 1 ninguna es especial

    public bool IsInstant() => analysisTime <= 0f;

    // Colores 

    public static Color GetColorForType(SignalType type, SignalTier tier)
    {
        // Enhanced siempre amarillo independiente del tipo
        if (tier == SignalTier.Enhanced)
            return new Color(1f, 0.85f, 0f, 0.95f);

        switch (type)
        {
            case SignalType.CosmicNoise:
                return new Color(1f, 1f, 1f, 0.85f);
            case SignalType.CosmicNoiseDouble:
                return new Color(0.85f, 0.85f, 0.85f, 0.85f);
            case SignalType.CosmicNoiseTriple:
                return new Color(0.65f, 0.65f, 0.65f, 0.85f);
            default:
                return Color.white;
        }
    }

    // Valores base 

    public static float GetBaseAnalysisTime(SignalType type)
    {
        // Tiempo base del ruido simple — los demás se calculan en SignalManager
        return 3f;
    }

    public static double GetBaseReward(SignalType type)
    {
        return 10.0;
    }
}