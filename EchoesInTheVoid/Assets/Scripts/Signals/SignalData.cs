using UnityEngine;

public enum SignalType
{
    // Fase 1
    CosmicNoise,        // Ruido cósmico — blanco
    GroupedSignal,      // Señal agrupada — gris claro

    // Fase 2
    WeakSignal,         // Señal débil — cian
    Echo,               // Eco — azul oscuro

    // Fase 3
    MediumSignal,       // Señal media — amarillo
    AttractedSignal,    // Señal atraída — naranja-amarillo

    // Fase 4
    StrongSignal,       // Señal fuerte — naranja
    Biomass,            // Biomasa — rojo

    // Especiales
    Fragmented,         // Fragmentada — azul eléctrico
    Anomaly,            // Anomalía — morado
    DeepSignal          // Señal profunda — rojo-blanco pulsante
}

public enum SignalState
{
    Hidden,             // Generada pero sweep no ha pasado
    Revealed,           // Visible, interactuable
    Analyzing,          // Click mantenido activo
    Completed           // Análisis completado
}

[System.Serializable]
public class SignalData
{
    [Header("Tipo y estado")]
    public SignalType type;
    public SignalState state = SignalState.Hidden;

    [Header("Posición")]
    public Vector2 position;
    public float signalAngle;

    [Header("Valores")]
    public double dataReward;
    public float analysisTime;
    public float analysisProgress = 0f;

    [Header("Señal agrupada")]
    public int groupSize = 1;    // 1, 2 o 3

    [Header("Señal fragmentada")]
    public int fragmentIndex = 0;
    public int fragmentTotal = 3;
    public string fragmentGroupId = "";

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

    public bool RequiresHold()
    {
        // Todas requieren click mantenido — algunas tienen tiempo 0
        return true;
    }

    public bool IsInstant()
    {
        return analysisTime <= 0f;
    }

    public bool IsSpecial()
    {
        return type == SignalType.Fragmented ||
               type == SignalType.Anomaly ||
               type == SignalType.DeepSignal;
    }

    public bool IsBase()
    {
        return !IsSpecial();
    }

    // Fase en la que aparece este tipo de señal
    public static GamePhase GetPhaseForType(SignalType type)
    {
        switch (type)
        {
            case SignalType.CosmicNoise:
            case SignalType.GroupedSignal:
                return GamePhase.Phase1;

            case SignalType.WeakSignal:
            case SignalType.Echo:
            case SignalType.Fragmented:
                return GamePhase.Phase2;

            case SignalType.MediumSignal:
            case SignalType.AttractedSignal:
            case SignalType.Anomaly:
                return GamePhase.Phase3;

            case SignalType.StrongSignal:
            case SignalType.Biomass:
            case SignalType.DeepSignal:
                return GamePhase.Phase4;

            default:
                return GamePhase.Phase1;
        }
    }

    // Color asociado a cada tipo
    public static Color GetColorForType(SignalType type)
    {
        switch (type)
        {
            case SignalType.CosmicNoise:
                return new Color(1f, 1f, 1f, 0.85f);
            case SignalType.GroupedSignal:
                return new Color(0.8f, 0.8f, 0.8f, 0.75f);
            case SignalType.WeakSignal:
                return new Color(0f, 0.85f, 1f, 0.9f);
            case SignalType.Echo:
                return new Color(0f, 0.35f, 0.8f, 0.9f);
            case SignalType.MediumSignal:
                return new Color(1f, 0.9f, 0f, 0.9f);
            case SignalType.AttractedSignal:
                return new Color(1f, 0.65f, 0f, 0.9f);
            case SignalType.StrongSignal:
                return new Color(1f, 0.4f, 0f, 0.9f);
            case SignalType.Biomass:
                return new Color(0.9f, 0.1f, 0.1f, 0.9f);
            case SignalType.Fragmented:
                return new Color(0.1f, 0.3f, 1f, 0.95f);
            case SignalType.Anomaly:
                return new Color(0.65f, 0f, 1f, 0.95f);
            case SignalType.DeepSignal:
                return new Color(1f, 0.95f, 0.95f, 1f);
            default:
                return Color.green;
        }
    }

    // Datos base por tipo (antes de multiplicadores)
    public static double GetBaseReward(SignalType type, GamePhase currentPhase)
    {
        switch (type)
        {
            case SignalType.CosmicNoise: return 10;
            case SignalType.GroupedSignal: return 0;   
            case SignalType.WeakSignal: return 80;
            case SignalType.Echo: return 120;
            case SignalType.MediumSignal: return 500;
            case SignalType.AttractedSignal: return 800;
            case SignalType.StrongSignal: return 3000;
            case SignalType.Biomass: return 5000;
            case SignalType.Fragmented: return 200;
            case SignalType.Anomaly: return 8000;
            case SignalType.DeepSignal:
                switch (currentPhase)
                {
                    case GamePhase.Phase1: return 1000;
                    case GamePhase.Phase2: return 5000;
                    case GamePhase.Phase3: return 25000;
                    case GamePhase.Phase4: return 100000;
                    default: return 1000;
                }
            default: return 10;
        }
    }

    // Tiempo de análisis base por tipo (antes de mejoras)
    public static float GetBaseAnalysisTime(SignalType type)
    {
        switch (type)
        {
            case SignalType.CosmicNoise: return 0.5f;
            case SignalType.GroupedSignal: return 1.0f;
            case SignalType.WeakSignal: return 2.0f;
            case SignalType.Echo: return 2.0f;
            case SignalType.MediumSignal: return 3.0f;
            case SignalType.AttractedSignal: return 3.0f;
            case SignalType.StrongSignal: return 4.0f;
            case SignalType.Biomass: return 4.0f;
            case SignalType.Fragmented: return 3.0f;
            case SignalType.Anomaly: return 5.0f;
            case SignalType.DeepSignal: return 8.0f;
            default: return 1.0f;
        }
    }
}