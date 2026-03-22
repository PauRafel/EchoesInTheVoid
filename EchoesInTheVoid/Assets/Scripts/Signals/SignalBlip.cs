using UnityEngine;

public class SignalBlip : MonoBehaviour
{
    private SpriteRenderer sr;
    private SignalData signal;
    private Color baseColor;
    private bool isPulsing = false;
    private float pulseSpeed = 3f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(SignalData signalData)
    {
        signal = signalData;
        baseColor = SignalData.GetColorForSignal(signal);

        if (sr != null)
            sr.color = baseColor;

        ApplyScale();
    }

    void ApplyScale()
    {
        float scale = 0.2f * signal.baseScale;
        transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        if (signal == null) return;

        if (isPulsing)
            UpdatePulse();
        else if (signal.IsAnalyzing())
            UpdateAnalyzing();
    }

    void UpdatePulse()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        Color c = baseColor;
        c.a = Mathf.Lerp(0.3f, 1f, t);
        if (sr != null) sr.color = c;
    }

    void UpdateAnalyzing()
    {
        float progress = signal.GetProgressRatio();
        Color c = Color.Lerp(baseColor * 0.4f, baseColor, progress);
        c.a = 1f;
        if (sr != null) sr.color = c;
    }

    public void SetPulsing(bool active)
    {
        isPulsing = active;
        if (!active && sr != null)
            sr.color = baseColor;
    }

    public SignalData GetSignalData() => signal;
}