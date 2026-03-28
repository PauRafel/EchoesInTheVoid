using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhaseTransitionManager : MonoBehaviour
{
    public static PhaseTransitionManager Instance { get; private set; }

    [Header("Referencias")]
    public Image fadeImageLeft;
    public Image fadeImageRight;

    [Header("Configuracion")]
    public float fadeDuration = 1.5f;
    public float signalSpawnDelay = 0.5f;

    private bool transitionActive = false;
    private bool phaseTransitionComplete = false;

    public bool IsPhaseTransitionComplete() => phaseTransitionComplete;

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
        GameManager.Instance.OnPhaseComplete += OnPhaseComplete;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPhaseComplete -= OnPhaseComplete;
    }

    void OnPhaseComplete()
    {
        if (transitionActive) return;
        transitionActive = true;
        StartCoroutine(PhaseTransitionRoutine());
    }

    IEnumerator PhaseTransitionRoutine()
    {
        yield return StartCoroutine(FadePanels());

        yield return new WaitForSeconds(signalSpawnDelay);

        SpawnTransitionSignal();
    }

    IEnumerator FadePanels()
    {
        float elapsed = 0f;

        Image leftImg = UIManager.Instance.leftPanel != null
            ? UIManager.Instance.leftPanel.GetComponent<Image>() : null;
        Image rightImg = UIManager.Instance.rightPanel != null
            ? UIManager.Instance.rightPanel.GetComponent<Image>() : null;

        Color leftStart = leftImg != null ? leftImg.color : Color.clear;
        Color rightStart = rightImg != null ? rightImg.color : Color.clear;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (leftImg != null)
            {
                Color c = leftStart;
                c.a = Mathf.Lerp(leftStart.a, 0f, t);
                leftImg.color = c;
            }

            if (rightImg != null)
            {
                Color c = rightStart;
                c.a = Mathf.Lerp(rightStart.a, 0f, t);
                rightImg.color = c;
            }

            yield return null;
        }

        UIManager.Instance.SetHUDVisible(false);
    }

    void SpawnTransitionSignal()
    {
        if (SignalManager.Instance != null)
            SignalManager.Instance.SpawnPhaseTransitionSignal();
    }

    public void OnTransitionSignalAnalyzed()
    {
        StartCoroutine(ShowTransitionMessage());
    }

    IEnumerator ShowTransitionMessage()
    {
        string rawMessage =
            "Aqui... tripulacion... algo... enorme... no es... espacio...";
        string distorted = DistortMessage(rawMessage);

        bool done = false;
        TransmisionUI.Instance.ShowMessage(
            "SENAL INTERCEPTADA",
            distorted,
            () => done = true);

        yield return new WaitUntil(() => done);

        transitionActive = false;
        EndPhaseRound();
    }

    string DistortMessage(string original)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (char c in original)
        {
            if (c == ' ')
            {
                sb.Append(' ');
                continue;
            }

            float roll = Random.value;
            if (roll < 0.6f)
                sb.Append('.');
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    void EndPhaseRound()
    {
        phaseTransitionComplete = true;
        GameManager.Instance.SetState(GameState.RoundEnd);
        GameManager.Instance.currentRound++;
        GameManager.Instance.roundTimer = 0f;

        if (RoundEndPanel.Instance != null)
            RoundEndPanel.Instance.ShowPhaseTransition(
                UIManager.Instance.GetRoundData(),
                UIManager.Instance.GetRoundSignals());
    }
}