using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Visual")]
    public int segments = 32;
    public float lineWidth = 0.02f;
    public Color cursorColor = new Color(0f, 0.85f, 0.2f, 0.6f);

    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private float currentRadius = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.visible = false;
        CreateLineRenderer();
    }

    void CreateLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = cursorColor;
        lineRenderer.endColor = cursorColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.sortingOrder = 10;
        DrawCursor(Vector3.zero);
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        // Fuera del juego activo — mostrar cursor del sistema
        bool inGame = GameManager.Instance != null &&
                      GameManager.Instance.IsScanning();

        if (!inGame)
        {
            Cursor.visible = true;
            lineRenderer.enabled = false;
            return;
        }

        Cursor.visible = false;
        lineRenderer.enabled = true;

        if (SignalAnalyzer.Instance != null)
        {
            float target = Mathf.Max(
                SignalAnalyzer.Instance.clickRadius,
                SignalAnalyzer.Instance.cursorRadius);

            currentRadius = Mathf.Lerp(currentRadius, target, Time.deltaTime * 10f);
        }

        DrawCursor(worldPos);
        UpdateColor();
    }

    void DrawCursor(Vector3 center)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            lineRenderer.SetPosition(i, new Vector3(
                center.x + Mathf.Cos(angle) * currentRadius,
                center.y + Mathf.Sin(angle) * currentRadius,
                0f
            ));
        }
    }

    void UpdateColor()
    {
        if (SignalAnalyzer.Instance == null) return;

        // Pulsa cuando está analizando
        Color c = cursorColor;
        if (SignalAnalyzer.Instance.IsHolding())
        {
            float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
            c.a = Mathf.Lerp(0.3f, 0.9f, pulse);
        }
        else
        {
            c.a = 0.6f;
        }

        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    public void SetRadius(float radius)
    {
        currentRadius = radius;
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}