using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class UpgradePanel : MonoBehaviour,
    IPointerClickHandler, IScrollHandler
{
    public static UpgradePanel Instance { get; private set; }

    [Header("Panel principal")]
    public GameObject panel;
    public Button btnContinuar;

    [Header("Nodes container (zoom y pan)")]
    public RectTransform nodesContainer;

    [Header("HUD fijo")]
    public TextMeshProUGUI datosText;
    public Button btnZoomIn;
    public Button btnZoomOut;

    [Header("Panel info inferior")]
    public GameObject bottomInfoPanel;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoEffect;
    public TextMeshProUGUI infoCost;
    public Button btnComprar;

    [Header("Zoom")]
    public float zoomMin = 0.4f;
    public float zoomMax = 1.5f;
    public float zoomStep = 0.1f;
    private float currentZoom = 1f;

    [Header("Pan limites")]
    public float panLimitX = 600f;
    public float panLimitY = 400f;

    private UpgradeData selectedUpgrade = null;
    private UpgradeNodeUI selectedNode = null;
    private List<UpgradeNodeUI> allNodes = new List<UpgradeNodeUI>();

    private bool isDragging = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        btnContinuar.onClick.AddListener(OnClickContinuar);
        btnZoomIn.onClick.AddListener(OnClickZoomIn);
        btnZoomOut.onClick.AddListener(OnClickZoomOut);
        btnComprar.onClick.AddListener(OnClickComprar);

        panel.SetActive(false);
        if (bottomInfoPanel != null) bottomInfoPanel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(false);
        panel.SetActive(true);

        if (bottomInfoPanel != null) bottomInfoPanel.SetActive(false);
        selectedUpgrade = null;
        selectedNode = null;

        RefreshAllNodes();
        UpdateDatosText();
        ApplyZoom();
    }

    public void Hide()
    {
        panel.SetActive(false);
        if (bottomInfoPanel != null) bottomInfoPanel.SetActive(false);
    }

    void RefreshAllNodes()
    {
        allNodes.Clear();
        UpgradeNodeUI[] nodes =
            nodesContainer.GetComponentsInChildren<UpgradeNodeUI>(true);

        List<UpgradeData> allUpgrades = GetAllUpgrades();

        for (int i = 0; i < nodes.Length && i < allUpgrades.Count; i++)
        {
            nodes[i].Setup(allUpgrades[i], this);
            allNodes.Add(nodes[i]);
            UpdateNodeVisibility(nodes[i], allUpgrades[i]);
        }
    }

    void UpdateNodeVisibility(UpgradeNodeUI node, UpgradeData upgrade)
    {
        bool bought = upgrade.comprada;
        bool available = upgrade.IsAvailable(UpgradeManager.Instance);
        bool show = bought || available;

        node.gameObject.SetActive(show);
    }

    List<UpgradeData> GetAllUpgrades()
    {
        return UpgradeManager.Instance.GetAllUpgradesOrdered();
    }

    // SELECCION DE NODO

    public void OnNodeSelected(UpgradeData upgrade, UpgradeNodeUI node)
    {
        if (selectedNode == node)
        {
            CloseInfoPanel();
            return;
        }

        selectedUpgrade = upgrade;
        selectedNode = node;
        OpenInfoPanel(upgrade);
    }

    void OpenInfoPanel(UpgradeData upgrade)
    {
        if (bottomInfoPanel == null) return;
        bottomInfoPanel.SetActive(true);

        if (infoName != null) infoName.text = upgrade.nombre.ToUpper();
        if (infoEffect != null) infoEffect.text = upgrade.descripcion;
        if (infoCost != null) infoCost.text = FormatCost(upgrade.coste);

        bool canAfford = upgrade.CanAfford();
        bool available = upgrade.IsAvailable(UpgradeManager.Instance);
        bool bought = upgrade.comprada;

        if (btnComprar != null)
        {
            btnComprar.gameObject.SetActive(!bought);
            btnComprar.interactable = canAfford && available;

            TextMeshProUGUI btnText =
                btnComprar.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = canAfford && available
                    ? "COMPRAR" : "SIN FONDOS";
                btnText.color = canAfford && available
                    ? new Color(0f, 1f, 0.27f, 1f)
                    : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
    }

    void CloseInfoPanel()
    {
        if (bottomInfoPanel != null) bottomInfoPanel.SetActive(false);
        selectedUpgrade = null;
        selectedNode = null;
    }

    // CLICK EN FONDO PARA CERRAR

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;
        if (eventData.pointerCurrentRaycast.gameObject == nodesContainer.gameObject)
            CloseInfoPanel();
    }

    // COMPRAR

    void OnClickComprar()
    {
        if (selectedUpgrade == null) return;

        bool bought = UpgradeManager.Instance.TryBuyUpgrade(selectedUpgrade);
        if (bought)
        {
            CloseInfoPanel();
            RefreshAllNodes();
            UpdateDatosText();
        }
    }

    // ZOOM

    void OnClickZoomIn()
    {
        currentZoom = Mathf.Clamp(currentZoom + zoomStep, zoomMin, zoomMax);
        ApplyZoom();
    }

    void OnClickZoomOut()
    {
        currentZoom = Mathf.Clamp(currentZoom - zoomStep, zoomMin, zoomMax);
        ApplyZoom();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float delta = eventData.scrollDelta.y * 0.1f;
        currentZoom = Mathf.Clamp(currentZoom + delta, zoomMin, zoomMax);
        ApplyZoom();
    }

    void ApplyZoom()
    {
        if (nodesContainer != null)
            nodesContainer.localScale = Vector3.one * currentZoom;
    }

    // PAN

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasReleasedThisFrame)
            isDragging = false;

        if (!panel.activeSelf) return;

        float scroll = Mouse.current != null
            ? Mouse.current.scroll.ReadValue().y
            : 0f;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float delta = scroll > 0 ? zoomStep : -zoomStep;
            currentZoom = Mathf.Clamp(
                currentZoom + delta, zoomMin, zoomMax);
            ApplyZoom();
        }

        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed && !IsPointerOverNode())
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            if (delta.magnitude > 0.1f)
            {
                isDragging = true;
                Vector2 pos = nodesContainer.anchoredPosition + delta;
                pos.x = Mathf.Clamp(pos.x, -panLimitX, panLimitX);
                pos.y = Mathf.Clamp(pos.y, -panLimitY, panLimitY);
                nodesContainer.anchoredPosition = pos;
            }
        }
    }

    // CONTINUAR

    void OnClickContinuar()
    {
        Hide();
        UIManager.Instance.SetHUDVisible(true);
        UIManager.Instance.ResetRoundData();
        SignalManager.Instance.ClearAllSignals();
        GameManager.Instance.StartRound();
    }

    void UpdateDatosText()
    {
        if (datosText == null || GameManager.Instance == null) return;
        datosText.text = "DATOS: " + FormatCost(GameManager.Instance.scanData);
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }

    public void BuildAllColumnsPublic() => RefreshAllNodes();

    bool IsPointerOverNode()
    {
        foreach (UpgradeNodeUI node in allNodes)
        {
            if (node == null || !node.gameObject.activeSelf) continue;

            RectTransform rt = node.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt,
                Mouse.current.position.ReadValue(),
                null,
                out localPoint);

            if (rt.rect.Contains(localPoint))
                return true;
        }
        return false;
    }
}