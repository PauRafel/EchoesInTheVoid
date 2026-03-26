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

    [Header("Canvas libre de nodos")]
    public RectTransform nodesContent;

    [Header("Popup")]
    public UpgradePopupUI popup;

    [Header("Prefab")]
    public GameObject upgradeNodePrefab;

    private List<UpgradeNodeUI> allNodes = new List<UpgradeNodeUI>();
    private UpgradeNodeUI selectedNode = null;

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
        panel.SetActive(false);

        if (popup != null) popup.Hide();
    }

    public void Show()
    {
        panel.SetActive(true);
        if (popup != null) popup.Hide();
        selectedNode = null;
        SpawnAllNodes();
        UpdateDatosText();
    }

    public void Hide()
    {
        panel.SetActive(false);
        if (popup != null) popup.Hide();
    }

    void SpawnAllNodes()
    {
        allNodes.Clear();

        UpgradeNodeUI[] nodes = nodesContent.GetComponentsInChildren<UpgradeNodeUI>(true);

        List<UpgradeData> all = GetAllUpgrades();

        for (int i = 0; i < nodes.Length && i < all.Count; i++)
        {
            nodes[i].Setup(all[i], this);
            allNodes.Add(nodes[i]);
        }
    }

    List<UpgradeData> GetAllUpgrades()
    {
        List<UpgradeData> all = new List<UpgradeData>();
        foreach (UpgradeBranch branch in
            System.Enum.GetValues(typeof(UpgradeBranch)))
            all.AddRange(UpgradeManager.Instance.GetBranchUpgrades(branch));
        return all;
    }

    void RefreshAllNodes()
    {
        foreach (UpgradeNodeUI node in allNodes)
            if (node != null) node.UpdateVisual();

        UpdateDatosText();

        if (popup != null && popup.gameObject.activeSelf)
            popup.Hide();
    }

    // Llamado desde UpgradeNodeUI al hacer click
    public void OnNodeSelected(UpgradeData upgrade, UpgradeNodeUI node)
    {
        // Nodos bloqueados no abren popup
        if (!upgrade.comprada && !upgrade.IsAvailable(UpgradeManager.Instance))
            return;

        // Si ya estaba seleccionado cierra el popup
        if (selectedNode == node)
        {
            if (popup != null) popup.Hide();
            selectedNode = null;
            return;
        }

        selectedNode = node;

        if (popup != null)
        {
            Vector2 popupPos = GetPopupPosition(node);
            popup.Show(upgrade, this, popupPos);
        }
    }

    Vector2 GetPopupPosition(UpgradeNodeUI node)
    {
        RectTransform nodeRt = node.GetComponent<RectTransform>();
        RectTransform popupRt = popup.GetComponent<RectTransform>();

        Vector2 nodePos = nodeRt.anchoredPosition;
        float offsetY = nodeRt.sizeDelta.y * 0.5f +
                          popupRt.sizeDelta.y * 0.5f + 10f;

        return new Vector2(nodePos.x, nodePos.y + offsetY);
    }

    // Llamado desde UpgradePopupUI al pulsar comprar
    public void OnBuyFromPopup(UpgradeData upgrade)
    {
        bool bought = UpgradeManager.Instance.TryBuyUpgrade(upgrade);
        if (bought)
        {
            selectedNode = null;
            RefreshAllNodes();
            UpdateDatosText();
        }
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

    public void BuildAllColumnsPublic() => SpawnAllNodes();
}