using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SignalAnalyzer : MonoBehaviour
{
    public static SignalAnalyzer Instance { get; private set; }

    [Header("Configuración")]
    public float clickRadius = 0.3f;
    public float cursorRadius = 0f;

    [Header("Debug")]
    public bool showCursorGizmo = true;

    private Camera mainCamera;

    private Dictionary<SignalData, float> analyzing =
        new Dictionary<SignalData, float>();

    private bool isHolding = false;
    private Vector3 worldMousePos;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!GameManager.Instance.IsScanning()) return;

        UpdateMousePosition();
        HandleInput();
        ProgressAnalysis();
        UpdateMovingSignals();
    }

    // Input

    void UpdateMousePosition()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        worldMousePos = mainCamera.ScreenToWorldPoint(screenPos);
        worldMousePos.z = 0f;
    }

    void HandleInput()
    {
        bool pressing = Mouse.current.leftButton.isPressed;

        if (pressing)
        {
            isHolding = true;
            TryStartAnalysis();
        }
        else
        {
            if (isHolding)
                CancelAllAnalysis();

            isHolding = false;
        }
    }

    void TryStartAnalysis()
    {
        if (SignalManager.Instance == null) return;

        float effectiveRadius = Mathf.Max(clickRadius, cursorRadius);

        foreach (SignalData signal in SignalManager.Instance.GetRevealedSignals())
        {
            if (analyzing.ContainsKey(signal)) continue;

            float dist = Vector2.Distance(worldMousePos, signal.position);
            if (dist <= effectiveRadius)
                StartAnalysis(signal);
        }
    }

    void StartAnalysis(SignalData signal)
    {
        signal.state = SignalState.Analyzing;
        analyzing[signal] = signal.analysisProgress;

        SignalBlip blip = signal.visualObject?.GetComponent<SignalBlip>();
        if (blip != null) blip.SetPulsing(false);
    }

    void CancelAllAnalysis()
    {
        List<SignalData> toCancel = new List<SignalData>(analyzing.Keys);
        foreach (SignalData signal in toCancel)
            CancelAnalysis(signal);
    }

    void CancelAnalysis(SignalData signal)
    {
        if (signal == null) return;

        if (signal.IsSpecial())
            signal.analysisProgress = 0f;

        signal.state = SignalState.Revealed;

        if (signal.visualObject != null)
        {
            SignalBlip blip = signal.visualObject.GetComponent<SignalBlip>();
            if (blip != null) blip.SetPulsing(signal.IsSpecial());
        }

        analyzing.Remove(signal);
    }

    // Progreso

    void ProgressAnalysis()
    {
        List<SignalData> keys = new List<SignalData>(analyzing.Keys);
        List<SignalData> completed = new List<SignalData>();
        List<SignalData> cancelled = new List<SignalData>();

        foreach (SignalData signal in keys)
        {
            if (signal == null || signal.IsCompleted())
            {
                completed.Add(signal);
                continue;
            }

            float dist = Vector2.Distance(worldMousePos, signal.position);
            float effectiveRadius = Mathf.Max(clickRadius, cursorRadius);

            if (dist > effectiveRadius)
            {
                cancelled.Add(signal);
                continue;
            }

            if (signal.analysisTime <= 0f)
            {
                completed.Add(signal);
                continue;
            }

            signal.analysisProgress += Time.deltaTime;
            analyzing[signal] = signal.analysisProgress;

            if (signal.analysisProgress >= signal.analysisTime)
                completed.Add(signal);
        }

        foreach (SignalData signal in cancelled)
            CancelAnalysis(signal);

        foreach (SignalData signal in completed)
        {
            if (signal != null && !signal.IsCompleted())
                SignalManager.Instance.CompleteSignal(signal);
            analyzing.Remove(signal);
        }
    }

    void UpdateMovingSignals()
    {
        if (!isHolding) return;

        List<SignalData> keys = new List<SignalData>(analyzing.Keys);
        foreach (SignalData signal in keys)
        {
            float dist = Vector2.Distance(worldMousePos, signal.position);
            float effectiveRadius = Mathf.Max(clickRadius, cursorRadius);

            if (dist > effectiveRadius)
                CancelAnalysis(signal);
        }
    }

    // API pública

    public void SetCursorRadius(float radius) => cursorRadius = radius;
    public Vector3 GetWorldMousePos() => worldMousePos;
    public bool IsHolding() => isHolding;
    public int GetAnalyzingCount() => analyzing.Count;

    // Gizmos

    void OnDrawGizmos()
    {
        if (!showCursorGizmo || !Application.isPlaying) return;

        float r = Mathf.Max(clickRadius, cursorRadius);
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(worldMousePos, r);
    }

    public void ClearAnalyzing()
    {
        analyzing.Clear();
        isHolding = false;
    }
}