using UnityEngine;

public enum SignalType
{
    CosmicNoise,
    CosmicNoiseDouble,
    CosmicNoiseTriple,
    CosmicNoiseQuadruple,
    CosmicNoiseQuintuple,
    Echo,
    Attracted,
    Biomass,
    PhaseTransition
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
    Normal,
    Double,
    Triple,
    Quadruple,
    Quintuple
}

[System.Serializable]
public class SignalData
{
    public SignalType type;
    public SignalTier tier = SignalTier.Normal;
    public SignalState state = SignalState.Hidden;
    public bool isEnhanced = false;
    public bool isSuperEnhanced = false;

    public Vector2 position;
    public float signalAngle;

    public double dataReward;
    public float analysisTime;
    public float analysisProgress = 0f;
    public float baseScale = 1f;

    public float moveSpeed = 0f;
    public Vector2 moveDir = Vector2.zero;

    public GameObject visualObject;

    public bool IsHidden() => state == SignalState.Hidden;
    public bool IsRevealed() => state == SignalState.Revealed;
    public bool IsAnalyzing() => state == SignalState.Analyzing;
    public bool IsCompleted() => state == SignalState.Completed;

    public float GetProgressRatio()
    {
        if (analysisTime <= 0f) return 1f;
        return Mathf.Clamp01(analysisProgress / analysisTime);
    }

    public bool IsSpecial() => false;
    public bool IsInstant() => analysisTime <= 0f;

    public static Color GetColorForSignal(SignalData signal)
    {
        if (signal.isEnhanced && signal.isSuperEnhanced)
            return new Color(1f, 0.6f, 0f, 0.95f);

        if (signal.isEnhanced)
            return new Color(1f, 0.85f, 0f, 0.95f);

        switch (signal.type)
        {
            case SignalType.PhaseTransition:
                return new Color(0.5f, 0f, 1f, 1f);
            case SignalType.CosmicNoise:
                return new Color(1f, 1f, 1f, 0.85f);
            case SignalType.CosmicNoiseDouble:
                return new Color(0.85f, 0.85f, 0.85f, 0.85f);
            case SignalType.CosmicNoiseTriple:
                return new Color(0.65f, 0.65f, 0.65f, 0.85f);
            case SignalType.CosmicNoiseQuadruple:
                return new Color(0.3f, 0.6f, 1f, 0.9f);
            case SignalType.CosmicNoiseQuintuple:
                return new Color(0.6f, 0.3f, 1f, 0.9f);
            case SignalType.Echo:
                return new Color(0f, 0.8f, 0.8f, 0.9f);
            case SignalType.Attracted:
                return new Color(1f, 0.5f, 0f, 0.9f);
            case SignalType.Biomass:
                return new Color(0.8f, 0.1f, 0.1f, 0.9f);
            default:
                return Color.white;
        }
    }

    public static float GetBaseAnalysisTime()
    {
        return 1.0f;
    }

    public static double GetBaseReward()
    {
        return 10.0;
    }
}