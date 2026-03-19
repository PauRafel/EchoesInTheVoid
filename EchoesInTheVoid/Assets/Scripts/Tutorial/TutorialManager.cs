using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Referencias")]
    public Image fadePanel;

    [Header("Configuración")]
    public float fadeDuration = 2.5f;
    public int signalsRequired = 10;

    private int signalsAnalyzed = 0;
    private bool tutorialActive = false;
    private bool waitingForSignal = false;
    private bool waitingForAnalysis = false;

    // Mensajes del capitán
    private const string HEADER = "CAPITÁN MORSE";

    private const string MSG_1 =
        "Soldado... si puedes oírme, significa que el sistema de emergencia " +
        "ha funcionado. Nuestra tripulación ha desaparecido. " +
        "Tu misión es encontrarlos usando el radar de largo alcance.";

    private const string MSG_2 =
        "El radar actual no tiene suficiente potencia para localizarlos. " +
        "Necesitaremos mejorarlo. Pero primero, debemos recopilar datos " +
        "del entorno para financiar las mejoras.";

    private const string MSG_3 =
        "¿Ves esa señal que acaba de aparecer en el radar? " +
        "Mantén pulsado el cursor sobre ella para analizarla.";

    private const string MSG_4 =
        "Bien hecho. Repite esta acción con todas las señales que detectes. " +
        "Cada análisis nos proporciona datos valiosos.";

    private const string MSG_5 =
        "Con estos datos tenemos suficiente para inicializar " +
        "el sistema de mejoras del radar. Accede al panel ahora.";

    private const string MSG_6 =
        "Instalación completada. El radar ya está operativo. " +
        "A partir de aquí ya sabes qué hacer. " +
        "Suerte con tu misión... nos vemos pronto, amigo.";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartTutorial());
    }

    // Flujo principal

    IEnumerator StartTutorial()
    {
        tutorialActive = true;
        GameManager.Instance.SetState(GameState.Paused);

        UIManager.Instance.SetHUDVisible(false);

        // Aseguramos que el radar está en estado inicial
        RadarController.Instance.SetRingCount(1);
        RadarController.Instance.SetSweepSpeed(45f);

        // Fade in — de negro a juego
        yield return StartCoroutine(FadeIn());

        // 3 segundos mirando el radar
        GameManager.Instance.SetState(GameState.Scanning);
        yield return new WaitForSeconds(3f);

        // Mensaje 1 del capitán
        yield return ShowMessage(MSG_1);

        // Mensaje 2
        yield return ShowMessage(MSG_2);

        // AHORA activamos las señales — solo después del mensaje 2
        SignalManager.Instance.SetLimit(SignalType.CosmicNoise, 7);

        // Esperar que el sweep revele la primera señal
        waitingForSignal = true;
        yield return new WaitUntil(() => !waitingForSignal);

        // Primera señal detectada — mensaje 3 automático
        yield return ShowMessage(MSG_3);

        // Esperar primer análisis
        waitingForAnalysis = true;
        yield return new WaitUntil(() => !waitingForAnalysis);

        // Mensaje 4
        yield return ShowMessage(MSG_4);

        // Esperar 5 análisis en total
        yield return new WaitUntil(() => signalsAnalyzed >= signalsRequired);

        // Mensaje 5
        yield return ShowMessage(MSG_5);

        // Abrir panel de mejoras con solo el nodo inicial
        OpenTutorialUpgradePanel();
    }

    // Fade

    IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 0f;
        fadePanel.color = c;
        fadePanel.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = fadePanel.color;
        c.a = 0f;
        fadePanel.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 1f;
        fadePanel.color = c;
    }

    // Mensajes 

    IEnumerator ShowMessage(string message)
    {
        bool done = false;
        TransmisionUI.Instance.ShowMessage(HEADER, message, () => done = true);
        yield return new WaitUntil(() => done);
    }

    // Panel de mejoras del tutorial

    void OpenTutorialUpgradePanel()
    {
        GameManager.Instance.SetState(GameState.Upgrades);
        SignalManager.Instance.ClearAllSignals();
        UIManager.Instance.SetHUDVisible(false);
        TutorialInitPanel.Instance.Show();
    }

    public void OnTutorialUpgradeBought()
    {
        // Se llama desde UpgradeManager al comprar "inicializar_sistema"
        StartCoroutine(TutorialComplete());
    }

    IEnumerator TutorialComplete()
    {
        yield return ShowMessage(MSG_6);

        GameManager.Instance.SetPhase(GamePhase.Phase1);
        UpgradeManager.Instance.UnlockAllBranches();
        tutorialActive = false;

        UIManager.Instance.SetHUDVisible(true);

        SignalManager.Instance.SetLimit(SignalType.CosmicNoise, 15);
        SignalManager.Instance.spawnInterval = 0.4f;

        UpgradePanel.Instance.Show();
    }

    // Callbacks desde otros sistemas

    public void OnSignalRevealed()
    {
        if (!waitingForSignal) return;
        waitingForSignal = false;
    }

    public void OnSignalAnalyzed()
    {
        signalsAnalyzed++;

        if (waitingForAnalysis && signalsAnalyzed >= 1)
            waitingForAnalysis = false;
    }

    public bool IsTutorialActive() => tutorialActive;
}