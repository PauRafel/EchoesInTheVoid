using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TransmisionUI : MonoBehaviour
{
    public static TransmisionUI Instance { get; private set; }

    [Header("Referencias")]
    public GameObject panel;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI continueText;

    [Header("Configuración")]
    public float typewriterSpeed = 0.03f;

    private bool isTyping = false;
    private bool isWaitingClick = false;
    private string fullText = "";
    private Action onComplete;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!isWaitingClick) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isTyping)
                SkipTypewriter();
            else
                OnClickAdvance();
        }
    }

    // API pública

    public void ShowMessage(string header, string message,
                            Action callback = null)
    {
        onComplete = callback;
        panel.SetActive(true);

        if (headerText != null) headerText.text = header;
        if (continueText != null)
            continueText.text = "[ click para continuar ]";

        StopAllCoroutines();
        StartCoroutine(TypewriterRoutine(message));
    }

    public void Hide()
    {
        StopAllCoroutines();
        isTyping = false;
        isWaitingClick = false;
        if (panel != null) panel.SetActive(false);
    }

    // Typewriter

    IEnumerator TypewriterRoutine(string text)
    {
        fullText = text;
        isTyping = true;
        isWaitingClick = true;

        if (bodyText != null) bodyText.text = "";

        foreach (char c in text)
        {
            if (!isTyping) break;
            if (bodyText != null) bodyText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        if (bodyText != null) bodyText.text = fullText;
        isTyping = false;

        if (continueText != null)
            continueText.text = "[ click para continuar ]";
    }

    void SkipTypewriter()
    {
        StopAllCoroutines();
        isTyping = false;
        if (bodyText != null) bodyText.text = fullText;
        if (continueText != null)
            continueText.text = "[ click para continuar ]";
    }

    void OnClickAdvance()
    {
        isWaitingClick = false;
        Hide();
        onComplete?.Invoke();
    }
}