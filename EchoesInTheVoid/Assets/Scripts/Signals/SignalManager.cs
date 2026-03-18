using UnityEngine;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Instance { get; private set; }

    [Header("Referencias")]
    public GameObject signalPrefab;

    [Header("Configuración")]
    public float spawnInterval = 1.5f;

    // Límites de señales simultáneas por tipo
    private Dictionary<SignalType, int> signalLimits = new Dictionary<SignalType, int>();

    // Modificadores de tiempo de análisis por tipo
    private Dictionary<SignalType, float> analysisModifiers = new Dictionary<SignalType, float>();

    // Señales activas
    private List<SignalData> activeSignals = new List<SignalData>();

    // Timer de spawn
    private float spawnTimer = 0f;

    // Fragmentos activos agrupados por ID
    private Dictionary<string, List<SignalData>> fragmentGroups =
        new Dictionary<string, List<SignalData>>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeLimits();
        InitializeModifiers();
    }

    void InitializeLimits()
    {
        // Límites iniciales — solo ruido cósmico al principio
        signalLimits[SignalType.CosmicNoise] = 3;
        signalLimits[SignalType.GroupedSignal] = 0;
        signalLimits[SignalType.WeakSignal] = 0;
        signalLimits[SignalType.Echo] = 0;
        signalLimits[SignalType.MediumSignal] = 0;
        signalLimits[SignalType.AttractedSignal] = 0;
        signalLimits[SignalType.StrongSignal] = 0;
        signalLimits[SignalType.Biomass] = 0;
        signalLimits[SignalType.Fragmented] = 0;
        signalLimits[SignalType.Anomaly] = 0;
        signalLimits[SignalType.DeepSignal] = 0;
    }

    void InitializeModifiers()
    {
        foreach (SignalType type in System.Enum.GetValues(typeof(SignalType)))
            analysisModifiers[type] = 1f;
    }

    void Update()
    {
        if (!GameManager.Instance.IsScanning()) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnSignals();
        }

        UpdateAttractedSignals();
        UpdateBiomassSignals();
    }

    // Spawn

    void TrySpawnSignals()
    {
        foreach (SignalType type in System.Enum.GetValues(typeof(SignalType)))
        {
            int limit = GetLimit(type);
            int current = CountActive(type);

            if (current < limit)
                SpawnSignal(type);
        }
    }

    void SpawnSignal(SignalType type)
    {
        SignalData signal = new SignalData();
        signal.type = type;
        signal.state = SignalState.Hidden;
        signal.position = GetRandomPosition();
        signal.signalAngle = Mathf.Atan2(signal.position.y, signal.position.x)
                             * Mathf.Rad2Deg;
        if (signal.signalAngle < 0f) signal.signalAngle += 360f;

        // Valores base
        signal.dataReward = SignalData.GetBaseReward(type,
                               GameManager.Instance.currentPhase);
        signal.analysisTime = SignalData.GetBaseAnalysisTime(type)
                               * analysisModifiers[type];

        // Configuración especial por tipo
        ConfigureSpecialSignal(signal);

        // Visual
        CreateVisual(signal);
        signal.visualObject.SetActive(false); // oculta hasta que sweep revele

        activeSignals.Add(signal);
    }

    void ConfigureSpecialSignal(SignalData signal)
    {
        switch (signal.type)
        {
            case SignalType.GroupedSignal:
                signal.groupSize = Random.value < 0.6f ? 2 : 3;
                signal.dataReward = signal.groupSize == 2 ? 25 : 40;
                break;

            case SignalType.Fragmented:
                signal.fragmentTotal = 3;
                signal.fragmentGroupId = System.Guid.NewGuid().ToString();
                signal.fragmentIndex = 0;
                SpawnFragmentGroup(signal);
                break;

            case SignalType.DeepSignal:
                // La señal profunda tiene su propia lógica de spawn
                // controlada por el NarrativeManager (lo haremos más adelante)
                break;
        }
    }

    void SpawnFragmentGroup(SignalData firstFragment)
    {
        string groupId = firstFragment.fragmentGroupId;
        List<SignalData> group = new List<SignalData> { firstFragment };

        for (int i = 1; i < firstFragment.fragmentTotal; i++)
        {
            SignalData fragment = new SignalData();
            fragment.type = SignalType.Fragmented;
            fragment.state = SignalState.Hidden;
            fragment.position = GetRandomPosition();
            fragment.signalAngle = Mathf.Atan2(fragment.position.y,
                                      fragment.position.x) * Mathf.Rad2Deg;
            if (fragment.signalAngle < 0f) fragment.signalAngle += 360f;
            fragment.dataReward = 200;
            fragment.analysisTime = SignalData.GetBaseAnalysisTime(
                                      SignalType.Fragmented)
                                      * analysisModifiers[SignalType.Fragmented];
            fragment.fragmentGroupId = groupId;
            fragment.fragmentIndex = i;
            fragment.fragmentTotal = firstFragment.fragmentTotal;

            CreateVisual(fragment);
            fragment.visualObject.SetActive(false);
            activeSignals.Add(fragment);
            group.Add(fragment);
        }

        fragmentGroups[groupId] = group;
    }

    // Visual

    void CreateVisual(SignalData signal)
    {
        if (signalPrefab == null)
        {
            Debug.LogWarning("SignalManager: signalPrefab no asignado.");
            return;
        }

        GameObject obj = Instantiate(signalPrefab,
                         signal.position, Quaternion.identity);
        obj.transform.SetParent(transform);

        SignalBlip blip = obj.GetComponent<SignalBlip>();
        if (blip != null)
            blip.Setup(signal);

        signal.visualObject = obj;
    }

    // Reveal 

    public void RevealSignal(SignalData signal)
    {
        if (!signal.IsHidden()) return;
        signal.state = SignalState.Revealed;
        if (signal.visualObject != null)
            signal.visualObject.SetActive(true);
    }

    public List<SignalData> GetUnrevealedSignals()
    {
        return activeSignals.FindAll(s => s.IsHidden());
    }

    // Análisis 

    public void CompleteSignal(SignalData signal)
    {
        if (signal.IsCompleted()) return;
        signal.state = SignalState.Completed;

        // Recompensa
        GameManager.Instance.AddScanData(signal.dataReward);
        GameManager.Instance.totalSignalsAnalyzed++;

        if (UIManager.Instance != null)
            UIManager.Instance.RegisterSignalAnalyzed(signal);

        // Fragmento completado
        if (signal.type == SignalType.Fragmented)
            CheckFragmentGroupComplete(signal);

        // Destruir visual
        if (signal.visualObject != null)
            Destroy(signal.visualObject);

        activeSignals.Remove(signal);
    }

    void CheckFragmentGroupComplete(SignalData fragment)
    {
        string groupId = fragment.fragmentGroupId;
        if (!fragmentGroups.ContainsKey(groupId)) return;

        List<SignalData> group = fragmentGroups[groupId];
        bool allComplete = group.TrueForAll(f => f.IsCompleted());

        if (allComplete)
        {
            // Bonus por completar el set
            GameManager.Instance.AddScanData(1000);
            Debug.Log("Fragmento completo — bonus lore desbloqueado");
            fragmentGroups.Remove(groupId);
        }
    }

    // Señales especiales - movimiento 

    void UpdateAttractedSignals()
    {
        foreach (SignalData signal in activeSignals)
        {
            if (signal.type != SignalType.AttractedSignal) continue;
            if (!signal.IsRevealed()) continue;

            // Se mueve hacia el centro
            Vector2 dir = (Vector2.zero - signal.position).normalized;
            signal.position += dir * 0.3f * Time.deltaTime;

            if (signal.visualObject != null)
                signal.visualObject.transform.position = signal.position;

            // Si llega al centro desaparece sin recompensa
            if (signal.position.magnitude < 0.3f)
                RemoveSignalNoReward(signal);
        }
    }

    void UpdateBiomassSignals()
    {
        foreach (SignalData signal in activeSignals)
        {
            if (signal.type != SignalType.Biomass) continue;
            if (!signal.IsRevealed()) continue;

            // Se mueve aleatoriamente
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            signal.position += randomDir * 0.2f * Time.deltaTime;

            // Mantener dentro del radar
            if (signal.position.magnitude > RadarController.Instance.GetRadarRadius() * 0.95f)
                signal.position = signal.position.normalized
                                  * RadarController.Instance.GetRadarRadius() * 0.9f;

            if (signal.visualObject != null)
                signal.visualObject.transform.position = signal.position;
        }
    }

    void RemoveSignalNoReward(SignalData signal)
    {
        if (signal.visualObject != null)
            Destroy(signal.visualObject);
        activeSignals.Remove(signal);
    }

    // API pública 

    public List<SignalData> GetActiveSignals() => activeSignals;
    public List<SignalData> GetRevealedSignals()
        => activeSignals.FindAll(s => s.IsRevealed());

    public void SetLimit(SignalType type, int limit)
        => signalLimits[type] = limit;

    public void AddToLimit(SignalType type, int amount)
    {
        if (!signalLimits.ContainsKey(type)) signalLimits[type] = 0;
        signalLimits[type] += amount;
    }

    public void SetAnalysisModifier(SignalType type, float modifier)
        => analysisModifiers[type] = modifier;

    public float GetAnalysisModifier(SignalType type)
        => analysisModifiers.ContainsKey(type) ? analysisModifiers[type] : 1f;

    int GetLimit(SignalType type)
        => signalLimits.ContainsKey(type) ? signalLimits[type] : 0;

    int CountActive(SignalType type)
        => activeSignals.FindAll(s => s.type == type && !s.IsCompleted()).Count;

    Vector2 GetRandomPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0.3f, 0.95f)
                         * RadarController.Instance.GetRadarRadius();
        return new Vector2(Mathf.Cos(angle) * distance,
                           Mathf.Sin(angle) * distance);
    }

    public void ClearAllSignals()
    {
        foreach (SignalData signal in activeSignals)
            if (signal.visualObject != null)
                Destroy(signal.visualObject);

        activeSignals.Clear();
        fragmentGroups.Clear();
    }
}