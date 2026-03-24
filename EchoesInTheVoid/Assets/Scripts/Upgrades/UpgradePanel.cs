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

    [Header("Columnas Content de cada ScrollView")]
    public RectTransform contentRadar;
    public RectTransform contentTiempo;
    public RectTransform contentSenales;
    public RectTransform contentCursor;

    [Header("Panel detalle")]
    public GameObject detailPanel;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDesc;
    public TextMeshProUGUI detailCosteValue;
    public TextMeshProUGUI detailRamaValue;
    public Button btnComprar;
    public Button btnCerrar;

    [Header("Prefab")]
    public GameObject upgradeNodePrefab;

    [Header("Layout")]
    public float nodeHeight = 230f;
    public float nodeSpacing = 15f;

    private UpgradeData selectedUpgrade;

    private Dictionary<UpgradeBranch, List<UpgradeNodeUI>> columnNodes =
        new Dictionary<UpgradeBranch, List<UpgradeNodeUI>>();

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
        btnComprar.onClick.AddListener(OnClickComprar);
        btnCerrar.onClick.AddListener(OnClickCerrar);
        panel.SetActive(false);
        detailPanel.SetActive(false);
    }

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

    void BuildAllColumns()
    {
        BuildColumnMulti(contentRadar,
            new UpgradeBranch[] { UpgradeBranch.Sweep });

        BuildColumnMulti(contentTiempo,
            new UpgradeBranch[] { UpgradeBranch.Tiempo });

        BuildColumnMulti(contentSenales,
            new UpgradeBranch[] {
                UpgradeBranch.CantidadSenales,
                UpgradeBranch.TamanoSenales });

        BuildColumnMulti(contentCursor,
            new UpgradeBranch[] {
                UpgradeBranch.Cursor,
                UpgradeBranch.VelocidadAnalisis });
    }

    void BuildColumnMulti(RectTransform content, UpgradeBranch[] branches)
    {
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        foreach (UpgradeBranch branch in branches)
        {
            if (!columnNodes.ContainsKey(branch))
                columnNodes[branch] = new List<UpgradeNodeUI>();
            columnNodes[branch].Clear();
        }

        List<UpgradeData> allVisible = new List<UpgradeData>();
        foreach (UpgradeBranch branch in branches)
            allVisible.AddRange(
                UpgradeManager.Instance.GetVisibleUpgrades(branch));

        float yPos = 0f;

        foreach (UpgradeData upgrade in allVisible)
        {
            GameObject obj = Instantiate(upgradeNodePrefab, content);
            RectTransform rt = obj.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(nodeHeight - 10f, nodeHeight);
            rt.anchoredPosition = new Vector2(0f, -yPos);

            UpgradeNodeUI node = obj.GetComponent<UpgradeNodeUI>();
            node.Setup(upgrade, this);
            node.gameObject.SetActive(true);

            columnNodes[upgrade.rama].Add(node);
            yPos += nodeHeight + nodeSpacing;
        }

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
                if (node != null) node.UpdateVisual();

        UpdateDatosText();
    }

    public void OnNodeSelected(UpgradeData upgrade)
    {
        selectedUpgrade = upgrade;
        detailPanel.SetActive(true);

        detailTitle.text = upgrade.nombre.ToUpper();
        detailDesc.text = upgrade.descripcion;
        detailRamaValue.text = upgrade.rama.ToString().ToUpper();

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

    void OnClickComprar()
    {
        if (selectedUpgrade == null) return;

        bool bought = UpgradeManager.Instance.TryBuyUpgrade(selectedUpgrade);
        if (bought)
        {
            detailPanel.SetActive(false);
            selectedUpgrade = null;
            BuildAllColumns();
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

    void UpdateDatosText()
    {
        if (datosText == null || GameManager.Instance == null) return;
        datosText.text = "DATOS DISPONIBLES: " +
            FormatCost(GameManager.Instance.scanData);
    }

    string FormatCost(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F0") + "K";
        return value.ToString("F0");
    }

    public void BuildAllColumnsPublic() => BuildAllColumns();
}