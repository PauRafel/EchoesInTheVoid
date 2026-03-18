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
    public TextMeshProUGUI iconText;

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

        if (costText != null)
            costText.text = "*50*";

        if (costText != null)
            costText.color = canAfford
                ? new Color(0f, 1f, 0.27f, 1f)
                : new Color(0.8f, 0.3f, 0.3f, 1f);

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