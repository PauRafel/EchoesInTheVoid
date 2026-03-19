using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel Instance { get; private set; }

    [Header("Panel principal")]
    public GameObject panel;
    public TextMeshProUGUI datosText;
    public Button btnContinuar;

    [Header("Columnas — Content de cada ScrollView")]
    public RectTransform contentRadar;
    public RectTransform contentTiempo;
    public RectTransform contentSenales;
    public RectTransform contentCursor;

    [Header("Panel detalle")]
    public GameObject detailPanel;
    public TextMeshProUGUI detailIcon;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDesc;
    public TextMeshProUGUI detailCosteValue;
    public TextMeshProUGUI detailRamaValue;
    public TextMeshProUGUI detailFaseValue;
    public Button btnComprar;
    public Button btnCerrar;

    [Header("Prefab")]
    public GameObject upgradeNodePrefab;

    [Header("Layout")]
    public float nodeHeight = 80f;
    public float nodeSpacing = 8f;

    private UpgradeData selectedUpgrade;

    // Todos los nodos creados por rama
    private Dictionary<UpgradeBranch, List<UpgradeNodeUI>> columnNodes =
        new Dictionary<UpgradeBranch, List<UpgradeNodeUI>>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        btnContinuar.onClick.AddListener(OnClickContinuar);
        btnComprar.onClick.AddListener(OnClickComprar);
        btnCerrar.onClick.AddListener(OnClickCerrar);
        panel.SetActive(false);
        detailPanel.SetActive(false);
    }

    // Mostrar

    public void Show()
    {
        panel.SetActive(true);
        detailPanel.SetActive(false);
        selectedUpgrade = null;
        BuildAllColumns();
        UpdateDatosText();
    }

    public void Hide()
    {
        panel.SetActive(false);
        detailPanel.SetActive(false);
    }

    // Construcción

    void BuildAllColumns()
    {
        BuildColumn(UpgradeBranch.Radar, contentRadar);
        BuildColumn(UpgradeBranch.Tiempo, contentTiempo);
        BuildColumn(UpgradeBranch.Senales, contentSenales);
        BuildColumn(UpgradeBranch.Cursor, contentCursor);
    }

    void BuildColumn(UpgradeBranch branch, RectTransform content)
    {
        if (content == null) return;

        // Limpiar
        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        if (!columnNodes.ContainsKey(branch))
            columnNodes[branch] = new List<UpgradeNodeUI>();
        columnNodes[branch].Clear();

        // Quitar Vertical Layout Group si existe
        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);

        List<UpgradeData> all = UpgradeManager.Instance.GetBranchUpgrades(branch);

        float yPos = 0f;

        foreach (UpgradeData upgrade in all)
        {
            GameObject obj = Instantiate(upgradeNodePrefab, content);
            RectTransform rt = obj.GetComponent<RectTransform>();

            // Anchor top-center, crece hacia abajo
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(nodeHeight - 10f, nodeHeight);
            rt.anchoredPosition = new Vector2(0f, -yPos);

            UpgradeNodeUI node = obj.GetComponent<UpgradeNodeUI>();
            node.Setup(upgrade, this);
            node.gameObject.SetActive(true);
            columnNodes[branch].Add(node);

            yPos += nodeHeight + nodeSpacing;
        }

        // Altura total del content
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, yPos);
    }

    void RefreshAllNodes()
    {
        foreach (var column in columnNodes.Values)
            foreach (UpgradeNodeUI node in column)
                if (node != null)
                    node.UpdateVisual();

        UpdateDatosText();
    }

    // Detalle

    public void OnNodeSelected(UpgradeData upgrade)
    {
        selectedUpgrade = upgrade;
        detailPanel.SetActive(true);

        detailIcon.text = GetIconForBranch(upgrade.rama);
        detailTitle.text = upgrade.nombre.ToUpper();
        detailDesc.text = upgrade.descripcion;
        detailRamaValue.text = upgrade.rama.ToString().ToUpper();
        detailFaseValue.text = GetFaseLabel(upgrade.faseRequerida);

        bool bought = upgrade.comprada;
        bool canAfford = upgrade.CanAfford();
        bool available = upgrade.IsAvailable(UpgradeManager.Instance);

        if (bought)
        {
            detailCosteValue.text = "ADQUIRIDA";
            detailCosteValue.color = new Color(0f, 0.8f, 0.2f, 1f);
            btnComprar.gameObject.SetActive(false);
        }
        else
        {
            detailCosteValue.text = FormatCost(upgrade.coste);
            detailCosteValue.color = canAfford
                ? new Color(0f, 1f, 0.27f, 1f)
                : new Color(0.8f, 0.3f, 0.3f, 1f);

            btnComprar.gameObject.SetActive(true);
            btnComprar.interactable = canAfford && available;

            TextMeshProUGUI btnText =
                btnComprar.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = available ? "COMPRAR" : "BLOQUEADA";
                btnText.color = canAfford && available
                    ? new Color(0f, 1f, 0.27f, 1f)
                    : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
    }

    // Botones 

    void OnClickComprar()
    {
        if (selectedUpgrade == null) return;

        bool bought = UpgradeManager.Instance.TryBuyUpgrade(selectedUpgrade);
        if (bought)
        {
            detailPanel.SetActive(false);
            selectedUpgrade = null;
            RefreshAllNodes();
            UpdateDatosText();
        }
    }

    void OnClickCerrar()
    {
        detailPanel.SetActive(false);
        selectedUpgrade = null;
    }

    void OnClickContinuar()
    {
        Hide();
        UIManager.Instance.SetHUDVisible(true);
        UIManager.Instance.ResetRoundData();
        SignalManager.Instance.ClearAllSignals();
        GameManager.Instance.StartRound();
    }

    // Helpers

    void UpdateDatosText()
    {
        if (datosText == null || GameManager.Instance == null) return;
        datosText.text =
            $"DATOS DISPONIBLES: {FormatCost(GameManager.Instance.scanData)}";
    }

    string GetIconForBranch(UpgradeBranch branch)
    {
        switch (branch)
        {
            case UpgradeBranch.Radar: return "R";
            case UpgradeBranch.Tiempo: return "T";
            case UpgradeBranch.Senales: return "S";
            case UpgradeBranch.Cursor: return "C";
            default: return "?";
        }
    }

    string GetFaseLabel(GamePhase fase)
    {
        switch (fase)
        {
            case GamePhase.Phase1: return "FASE 1";
            case GamePhase.Phase2: return "FASE 2";
            case GamePhase.Phase3: return "FASE 3";
            case GamePhase.Phase4: return "FASE 4";
            default: return "—";
        }
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M ";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K ";
        return value.ToString("F0") + " ";
    }

    public void BuildAllColumnsPublic() => BuildAllColumns();
}