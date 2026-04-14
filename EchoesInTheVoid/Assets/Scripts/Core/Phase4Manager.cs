using UnityEngine;
using System.Collections;

public class Phase4Manager : MonoBehaviour
{
    public static Phase4Manager Instance { get; private set; }

    [Header("Configuracion biomasa por umbral")]
    public float[] biomassChancePerThreshold = { 0.05f, 0.10f, 0.18f, 0.28f, 0.40f };
    public float[] cursorRadiusPerThreshold = { 0.6f, 0.9f, 1.2f, 1.5f, 1.8f };

    private bool phase4Active = false;

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
        GameManager.Instance.OnPhase4Threshold += OnThresholdReached;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPhase4Threshold -= OnThresholdReached;
    }

    public void ActivatePhase4()
    {
        phase4Active = true;
        Debug.Log("Fase 4 activada");
    }

    void OnThresholdReached(int index)
    {
        if (index < biomassChancePerThreshold.Length)
        {
            SignalManager.Instance.SetChanceBiomass(
                biomassChancePerThreshold[index]);

            SignalAnalyzer.Instance.SetCursorRadius(
                cursorRadiusPerThreshold[index]);

            Debug.Log("Fase 4 umbral " + index +
                " biomasa: " + biomassChancePerThreshold[index]);
        }

        // Umbral final — indice 5
        if (index == 5)
            StartCoroutine(TriggerFinalSequence());
    }

    IEnumerator TriggerFinalSequence()
    {
        // Generar solo biomasa
        SignalManager.Instance.SetChanceBiomass(1f);
        SignalManager.Instance.GenerateRoundSignals();

        yield return new WaitForSeconds(2f);

        // Aparecer señal especial final
        SignalManager.Instance.SpawnPhaseTransitionSignal();
    }
}