using UnityEngine;

public class SignalBlip : MonoBehaviour
{
    [Header("Referencias")]
    private SpriteRenderer sr;
    private SignalData signal;

    [Header("Visual")]
    private Color baseColor;
    private float pulseSpeed = 3f;
    private float pulseMin = 0.4f;
    private float pulseMax = 1.0f;
    private bool isPulsing = false;

    // Para señal agrupada
    private SpriteRenderer[] groupDots;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(SignalData signalData)
    {
        signal = signalData;
        baseColor = SignalData.GetColorForType(signal.type);

        if (sr != null)
            sr.color = baseColor;

        ConfigureVisualByType();
    }

    void ConfigureVisualByType()
    {
        switch (signal.type)
        {
            case SignalType.CosmicNoise:
                SetScale(0.12f);
                break;

            case SignalType.GroupedSignal:
                SetScale(0.12f);
                CreateGroupDots();
                break;

            case SignalType.WeakSignal:
                SetScale(0.15f);
                break;

            case SignalType.Echo:
                SetScale(0.15f);
                break;

            case SignalType.MediumSignal:
                SetScale(0.18f);
                break;

            case SignalType.AttractedSignal:
                SetScale(0.18f);
                break;

            case SignalType.StrongSignal:
                SetScale(0.20f);
                break;

            case SignalType.Biomass:
                SetScale(0.20f);
                break;

            case SignalType.Fragmented:
                SetScale(0.16f);
                isPulsing = true;
                break;

            case SignalType.Anomaly:
                SetScale(0.22f);
                isPulsing = true;
                break;

            case SignalType.DeepSignal:
                SetScale(0.25f);
                isPulsing = true;
                break;
        }
    }

    void CreateGroupDots()
    {
        if (signal.groupSize <= 1) return;

        // Ocultar sprite principal
        if (sr != null) sr.enabled = false;

        groupDots = new SpriteRenderer[signal.groupSize];
        float spacing = 0.18f;
        float offset = (signal.groupSize - 1) * spacing * 0.5f;

        for (int i = 0; i < signal.groupSize; i++)
        {
            GameObject dot = new GameObject($"Dot_{i}");
            dot.transform.SetParent(transform);
            dot.transform.localPosition = new Vector3(
                (i * spacing) - offset, 0f, 0f);

            SpriteRenderer dotSr = dot.AddComponent<SpriteRenderer>();
            dotSr.sprite = GetComponent<SpriteRenderer>()?.sprite;
            dotSr.color = baseColor;
            dot.transform.localScale = Vector3.one * 0.12f;

            groupDots[i] = dotSr;
        }
    }

    void Update()
    {
        if (signal == null) return;

        if (isPulsing)
            UpdatePulse();

        if (signal.IsAnalyzing())
            UpdateAnalyzingVisual();
    }

    void UpdatePulse()
    {
        float pulse = Mathf.Lerp(pulseMin, pulseMax,
                      (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        Color c = baseColor;
        c.a = pulse;

        if (sr != null) sr.color = c;

        if (groupDots != null)
            foreach (var dot in groupDots)
                if (dot != null) dot.color = c;
    }

    void UpdateAnalyzingVisual()
    {
        float progress = signal.GetProgressRatio();
        Color c = Color.Lerp(baseColor * 0.5f, baseColor * 1.3f, progress);
        c.a = 1f;

        if (sr != null) sr.color = c;

        if (groupDots != null)
            foreach (var dot in groupDots)
                if (dot != null) dot.color = c;
    }

    public void SetPulsing(bool active)
    {
        isPulsing = active;
        if (!active && sr != null)
            sr.color = baseColor;
    }

    void SetScale(float size)
    {
        transform.localScale = Vector3.one * size;
    }

    public SignalData GetSignalData() => signal;
}