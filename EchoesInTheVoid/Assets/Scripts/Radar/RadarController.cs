using UnityEngine;
using System.Collections.Generic;

public class RadarController : MonoBehaviour
{
    public static RadarController Instance { get; private set; }

    [Header("Configuración")]
    public float radarRadius = 4.8f;
    public int ringCount = 1;
    public float sweepSpeed = 45f;

    [Header("Colores")]
    public Color ringColor = new Color(0f, 0.25f, 0f, 1f);
    public Color sweepColor = new Color(0f, 0.85f, 0.2f, 0.8f);
    public Color centerColor = new Color(0f, 1f, 0.27f, 1f);

    [Header("Segundo sweep")]
    public bool hasSecondSweep = false;

    // Estado interno
    private float currentAngle = 0f;
    private float secondAngle = 180f;

    private List<LineRenderer> rings = new List<LineRenderer>();
    private LineRenderer sweepLine;
    private LineRenderer sweepLine2;
    private LineRenderer borderLine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildRadar();
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameState.Scanning &&
        GameManager.Instance.currentState != GameState.PhaseTransition) return;

        currentAngle += sweepSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        if (hasSecondSweep)
        {
            secondAngle += sweepSpeed * Time.deltaTime;
            if (secondAngle >= 360f) secondAngle -= 360f;
        }

        UpdateSweepLines();
        CheckSignalReveal();
    }

    // Construcción

    public void BuildRadar()
    {
        ClearRadar();
        BuildBorder();
        BuildRings();
        BuildSweepLines();
        BuildCenter();
    }

    void ClearRadar()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        rings.Clear();
        sweepLine = null;
        sweepLine2 = null;
        borderLine = null;
    }

    void BuildBorder()
    {
        GameObject obj = new GameObject("Border");
        obj.transform.SetParent(transform);
        borderLine = obj.AddComponent<LineRenderer>();
        SetupLineRenderer(borderLine, ringColor, 0.03f);
        DrawCircle(borderLine, radarRadius, 128);
    }

    void BuildRings()
    {
        for (int i = 0; i < ringCount; i++)
        {
            GameObject obj = new GameObject($"Ring_{i}");
            obj.transform.SetParent(transform);

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            SetupLineRenderer(lr, ringColor, 0.015f);

            float radius = radarRadius * ((float)(i + 1) / (ringCount + 1));
            DrawCircle(lr, radius, 96);
            rings.Add(lr);
        }
    }

    void BuildSweepLines()
    {
        // Sweep principal
        GameObject obj1 = new GameObject("Sweep1");
        obj1.transform.SetParent(transform);
        sweepLine = obj1.AddComponent<LineRenderer>();
        SetupLineRenderer(sweepLine, sweepColor, 0.025f);
        sweepLine.positionCount = 2;

        // Segundo sweep (inactivo hasta que se desbloquee)
        GameObject obj2 = new GameObject("Sweep2");
        obj2.transform.SetParent(transform);
        sweepLine2 = obj2.AddComponent<LineRenderer>();
        SetupLineRenderer(sweepLine2, sweepColor, 0.025f);
        sweepLine2.positionCount = 2;
        sweepLine2.gameObject.SetActive(false);
    }

    void BuildCenter()
    {
        GameObject obj = new GameObject("Center");
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        SetupLineRenderer(lr, centerColor, 0.06f);
        DrawCircle(lr, 0.06f, 16);
    }

    // Update visual

    void UpdateSweepLines()
    {
        Vector3 dir1 = AngleToDirection(currentAngle);
        sweepLine.SetPosition(0, transform.position);
        sweepLine.SetPosition(1, transform.position + dir1 * radarRadius);

        if (hasSecondSweep && sweepLine2.gameObject.activeSelf)
        {
            Vector3 dir2 = AngleToDirection(secondAngle);
            sweepLine2.SetPosition(0, transform.position);
            sweepLine2.SetPosition(1, transform.position + dir2 * radarRadius);
        }
    }

    void CheckSignalReveal()
    {
        if (SignalManager.Instance == null) return;

        foreach (SignalData signal in SignalManager.Instance.GetUnrevealedSignals())
        {
            if (IsAngleNear(currentAngle, signal.signalAngle) ||
                (hasSecondSweep && IsAngleNear(secondAngle, signal.signalAngle)))
            {
                SignalManager.Instance.RevealSignal(signal);
            }
        }
    }

    bool IsAngleNear(float sweepAngle, float targetAngle)
    {
        float tolerance = sweepSpeed * Time.deltaTime * 2.5f;
        return Mathf.Abs(Mathf.DeltaAngle(sweepAngle, targetAngle)) < tolerance;
    }

    // API pública

    public void SetRingCount(int count)
    {
        ringCount = count;
        BuildRadar();
    }

    public void SetSweepSpeed(float speed)
    {
        sweepSpeed = speed;
    }

    public void EnableSecondSweep()
    {
        hasSecondSweep = true;
        secondAngle = currentAngle + 180f;
        sweepLine2.gameObject.SetActive(true);
    }

    public float GetCurrentAngle() => currentAngle;
    public float GetSecondAngle() => secondAngle;
    public float GetRadarRadius() => radarRadius;

    // Utilidades

    Vector3 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }

    void DrawCircle(LineRenderer lr, float radius, int segments)
    {
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            lr.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            ));
        }
    }

    void SetupLineRenderer(LineRenderer lr, Color color, float width)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = false;
        lr.loop = false;
    }
}