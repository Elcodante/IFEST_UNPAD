using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CinematicTransition : MonoBehaviour
{
    [Header("UI Layers")]
    public RectTransform interiorBackground;
    public CanvasGroup minigamePanel;
    public CanvasGroup blackFadeOverlay;

    [Header("Timing Settings (Smoothness)")]
    [Tooltip("Waktu transisi menjadi hitam (Layar Menutup)")]
    public float fadeToBlackDuration = 0.4f;
    [Tooltip("Waktu transisi dari hitam menjadi bening (Layar Membuka)")]
    public float fadeFromBlackDuration = 0.5f;
    [Tooltip("Lama waktu pemain melihat background ruangan sebelum minigame muncul")]
    public float interiorHoldTime = 1.0f;

    private void OnEnable()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // --- PERSIAPAN AWAL ---
        blackFadeOverlay.gameObject.SetActive(true);
        blackFadeOverlay.blocksRaycasts = true;

        // Mulai dengan layar yang masih bening (Alpha 0) agar kita bisa melakukan Fade To Black pertama
        blackFadeOverlay.alpha = 0f;

        // Sembunyikan layer lainnya terlebih dahulu
        interiorBackground.gameObject.SetActive(false);
        minigamePanel.alpha = 0f;
        minigamePanel.interactable = false;
        minigamePanel.blocksRaycasts = false;
        minigamePanel.transform.localScale = Vector3.one;

        // --- 1. KEDIPAN PERTAMA: TUTUP MATA (Top-Down ke Hitam) ---
        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        // Tampilkan gambar interior DI BALIK layar hitam
        interiorBackground.gameObject.SetActive(true);
        interiorBackground.localScale = Vector3.one;

        // --- 2. BUKA MATA (Melihat Interior) ---
        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        // --- 3. TAHAN & ZOOM HALUS INTERIOR ---
        float holdTime = 0f;
        while (holdTime < interiorHoldTime)
        {
            holdTime += Time.deltaTime;
            // Zoom in lambat dari skala 1.0 ke 1.05 untuk ilusi kamera maju
            float scale = Mathf.Lerp(1.0f, 1.05f, holdTime / interiorHoldTime);
            interiorBackground.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // --- 4. KEDIPAN KEDUA: TUTUP MATA (Interior ke Hitam) ---
        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        // Munculkan Panel Minigame DI BALIK layar hitam
        // (Kita tidak butuh animasi Pop-Up lagi karena akan ter-reveal oleh Fade)
        minigamePanel.alpha = 1f;

        // --- 5. BUKA MATA (Melihat Minigame) ---
        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        // Berikan akses klik kepada pemain
        minigamePanel.interactable = true;
        minigamePanel.blocksRaycasts = true;
        blackFadeOverlay.blocksRaycasts = false;
    }

    public void CloseTransition()
    {
        StartCoroutine(OutroSequence());
    }

    private IEnumerator OutroSequence()
    {
        // Kunci layar agar pemain tidak mengklik saat transisi keluar
        minigamePanel.interactable = false;
        minigamePanel.blocksRaycasts = false;
        blackFadeOverlay.blocksRaycasts = true;

        // --- 1. TUTUP MATA TERAKHIR (Minigame ke Hitam) ---
        yield return StartCoroutine(FadeBlack(0f, 1f, fadeToBlackDuration));

        // Matikan semua elemen UI DI BALIK layar hitam
        minigamePanel.alpha = 0f;
        interiorBackground.gameObject.SetActive(false);

        // --- 2. BUKA MATA KE CCTV (Hitam ke Top-Down) ---
        yield return StartCoroutine(FadeBlack(1f, 0f, fadeFromBlackDuration));

        blackFadeOverlay.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fungsi Bantuan (Helper) untuk melakukan Fade dengan kurva pergerakan SmoothStep.
    /// Penggunaan yield return di fungsinya memastikan sistem menunggu animasi ini selesai
    /// sebelum melanjutkan ke baris kode berikutnya.
    /// </summary>
    private IEnumerator FadeBlack(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            // SmoothStep memperhalus ujung-ujung animasi (ease-in & ease-out)
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            blackFadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }
        blackFadeOverlay.alpha = endAlpha;
    }
}