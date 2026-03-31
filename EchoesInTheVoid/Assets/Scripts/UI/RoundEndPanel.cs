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
    public TextMeshProUGUI dataTotalText;
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

        dataRondaText.text = "DATOS OBTENIDOS: " + FormatData(datos);
        senalesRondaText.text = "SENALES ANALIZADAS: " + senales;
        dataTotalText.text = "DATOS TOTALES: " +
            FormatData(GameManager.Instance.scanData);

        btnContinuar.gameObject.SetActive(true);
        btnMejoras.gameObject.SetActive(true);

        UIManager.Instance.SetHUDVisible(false);
    }

    void OnClickMejoras()
    {
        panel.SetActive(false);

        if (PhaseTransitionManager.Instance != null &&
            PhaseTransitionManager.Instance.IsPhaseTransitionComplete())
        {
            switch (GameManager.Instance.currentPhase)
            {
                case GamePhase.Phase1:
                    Phase2InitPanel.Instance.Show();
                    break;
                case GamePhase.Phase2:
                    Phase3InitPanel.Instance.Show();
                    break;
                default:
                    UpgradePanel.Instance.Show();
                    break;
            }
        }
        else
        {
            UpgradePanel.Instance.Show();
        }
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

    public void ShowPhaseTransition(double datos, int senales)
    {
        panel.SetActive(true);

        dataRondaText.text = "DATOS OBTENIDOS: " + FormatData(datos);
        senalesRondaText.text = "SENALES ANALIZADAS: " + senales;
        dataTotalText.text = "DATOS TOTALES: " +
            FormatData(GameManager.Instance.scanData);

        btnContinuar.gameObject.SetActive(false);
        btnMejoras.gameObject.SetActive(true);

        UIManager.Instance.SetHUDVisible(false);
    }
}