using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicTransition : MonoBehaviour
{
    [Header("UI Layers")]
    public RectTransform interiorBackground;
    public CanvasGroup minigamePanel;
    public CanvasGroup blackFadeOverlay;

    [Header("Minigame Link (Auto-Detect)")]
    [Tooltip("Sistem akan otomatis mencari script BaseMinigameManager di dalam Panel Minigame.")]
    public BaseMinigameManager minigameManager; // --- PERUBAHAN: Ganti dari MonoBehaviour ke BaseMinigameManager ---

    [Header("Timing Settings (Smoothness)")]
    public float fadeToBlackDuration = 0.4f;
    public float fadeFromBlackDuration = 0.5f;
    public float interiorHoldTime = 1.0f;

    private void Awake()
    {
        // --- PERBAIKAN 1: AUTO-LINK ---
        // Mencari script turunan BaseMinigameManager (seperti MinigameDragManager / AntidoteSequenceManager)
        // secara otomatis di dalam minigamePanel. Ini kebal terhadap error salah tarik di Inspector.
        if (minigameManager == null && minigamePanel != null)
        {
            minigameManager = minigamePanel.GetComponentInChildren<BaseMinigameManager>(true);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(false);
        }

        if (minigameManager != null)
        {
            minigameManager.enabled = false;
        }

        blackFadeOverlay.gameObject.SetActive(true);
        blackFadeOverlay.blocksRaycasts = true;
        blackFadeOverlay.alpha = 0f;

        interiorBackground.gameObject.SetActive(false);

        // 1. Kedipan pertama
        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        interiorBackground.gameObject.SetActive(true);
        interiorBackground.localScale = Vector3.one;

        // 2. Buka mata ke Interior
        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        // 3. Zoom Halus
        float holdTime = 0f;
        while (holdTime < interiorHoldTime)
        {
            holdTime += Time.deltaTime;
            float scale = Mathf.Lerp(1.0f, 1.05f, holdTime / interiorHoldTime);
            interiorBackground.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // 4. Kedipan kedua
        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(true);
            minigamePanel.alpha = 1f;
            minigamePanel.interactable = true;
            minigamePanel.blocksRaycasts = true;
        }

        // --- PERBAIKAN 2: "Booting" Akurat ---
        // Karena variabelnya sekarang adalah BaseMinigameManager, baris ini 
        // DIJAMIN menyalakan script logikanya, bukan komponen gambar.
        if (minigameManager != null)
        {
            minigameManager.enabled = true;
        }

        // 5. Buka mata ke Minigame
        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        blackFadeOverlay.blocksRaycasts = false;
    }

    public void CloseTransition()
    {
        StartCoroutine(OutroSequence());
    }

    private IEnumerator OutroSequence()
    {
        if (minigameManager != null)
        {
            minigameManager.enabled = false;
        }

        if (minigamePanel != null)
        {
            minigamePanel.interactable = false;
            minigamePanel.blocksRaycasts = false;
        }

        blackFadeOverlay.blocksRaycasts = true;

        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        if (minigamePanel != null)
        {
            minigamePanel.gameObject.SetActive(false);
        }
        interiorBackground.gameObject.SetActive(false);

        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        blackFadeOverlay.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private IEnumerator FadeBlack(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            blackFadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        blackFadeOverlay.alpha = endAlpha;
    }
}