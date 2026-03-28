using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialInitPanel : MonoBehaviour
{
    public static TutorialInitPanel Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public Button initButton;
    public TextMeshProUGUI costText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        initButton.onClick.AddListener(OnClickInit);
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        bool canAfford = GameManager.Instance.scanData >= 50;
        if (costText != null) costText.text = "50";
        initButton.interactable = canAfford;
    }

    void OnClickInit()
    {
        if (GameManager.Instance.scanData < 50) return;
        GameManager.Instance.SpendData(50);
        panel.SetActive(false);
        TutorialManager.Instance.OnTutorialUpgradeBought();
    }
}