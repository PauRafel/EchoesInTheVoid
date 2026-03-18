using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundEndPanel : MonoBehaviour
{
    public static RoundEndPanel Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public TextMeshProUGUI dataRondaText;
    public TextMeshProUGUI senalesRondaText;
    public Button btnMejoras;
    public Button btnContinuar;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        btnMejoras.onClick.AddListener(OnClickMejoras);
        btnContinuar.onClick.AddListener(OnClickContinuar);
        panel.SetActive(false);
    }

    public void Show(double datos, int senales)
    {
        panel.SetActive(true);

        dataRondaText.text = $"DATOS OBTENIDOS: {FormatData(datos)}";
        senalesRondaText.text = $"SEÑALES ANALIZADAS: {senales}";

        UIManager.Instance.SetHUDVisible(false);
    }

    void OnClickMejoras()
    {
        panel.SetActive(false);
        // UpgradePanel.Instance.Show(); — lo activamos cuando tengamos el panel
        Debug.Log("Ir a mejoras — pendiente");
    }

    void OnClickContinuar()
    {
        panel.SetActive(false);
        StartNextRound();
    }

    void StartNextRound()
    {
        UIManager.Instance.SetHUDVisible(true);
        UIManager.Instance.FlushRoundDataToTotal();
        UIManager.Instance.ResetRoundData();
        SignalManager.Instance.ClearAllSignals();
        GameManager.Instance.StartRound();
    }

    string FormatData(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("F1") + "M";
        if (value >= 1000) return (value / 1000).ToString("F1") + "K";
        return Mathf.FloorToInt((float)value).ToString();
    }
}