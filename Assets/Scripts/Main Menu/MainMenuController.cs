using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class MainMenuController : MonoBehaviour
{
    [System.Serializable]
    public class MenuElement
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;

        [Min(0f)]
        public float delay;
    }

    [Header("Menu")]
    [SerializeField] private CanvasGroup mainMenuGroup;
    [SerializeField] private MenuElement[] elements;

    [Header("Settings UI")]
    [Tooltip("Masukkan GameObject Container/Panel Settings di sini.")]
    [SerializeField] private GameObject settingsPanel; // CONTAINER SETTINGS BARU

    [Header("Transition")]
    [SerializeField] private float duration = 0.55f;
    [SerializeField] private float slideDistance = 450f;

    [Header("Gameplay")]
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private GameplaySystemsController gameplaySystemsController;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject slider1;
    [SerializeField] private GameObject slider2;
    [SerializeField] private GameObject pintu;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject day;

    private Vector2[] originalPositions;
    private bool isTransitioning;

    private void Awake()
    {
        originalPositions = new Vector2[elements.Length];

        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null && elements[i].rect != null)
            {
                originalPositions[i] = elements[i].rect.anchoredPosition;
            }
        }
    }

    private void Start()
    {
        if (playerController != null) playerController.enabled = false;
        if (slider1 != null) slider1.SetActive(false);
        if (slider2 != null) slider2.SetActive(false);
        if (pintu != null) pintu.SetActive(false);
        if (timer != null) timer.SetActive(false);

        // --- PERUBAHAN DI SINI: Ubah false menjadi true agar Day muncul di Main Menu ---
        if (day != null) day.SetActive(true);

        // Pastikan settings tertutup di awal game
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- FUNGSI SETTINGS ---

    public void OpenSettings()
    {
        if (isTransitioning) return; // Jangan buka settings kalau sedang transisi play

        // Nyalakan panel settings
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        // Sembunyikan main menu sementara tanpa merusak transisi PlayGame
        if (mainMenuGroup != null)
        {
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
        }
    }

    public void CloseSettings()
    {
        // Matikan panel settings
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Munculkan main menu kembali
        if (mainMenuGroup != null)
        {
            mainMenuGroup.alpha = 1f;
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;
        }
    }

    // --- FUNGSI KELUAR GAME ---

    public void QuitGame()
    {
        Debug.Log("Keluar dari game...");

        // Menutup aplikasi pada saat game di-build
        Application.Quit();

        // Menghentikan mode play jika sedang di dalam Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- FUNGSI GAMEPLAY (BAWAAN) ---

    public void PlayGame()
    {
        if (isTransitioning) return;
        StartCoroutine(HideMenu());
    }

    private IEnumerator HideMenu()
    {
        isTransitioning = true;

        if (mainMenuGroup != null)
        {
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
        }

        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            bool finished = true;

            for (int i = 0; i < elements.Length; i++)
            {
                MenuElement element = elements[i];
                if (element == null) continue;

                float t = Mathf.Clamp01((elapsed - element.delay) / duration);
                if (t < 1f) finished = false;

                t = EaseOutCubic(t);
                Vector2 target = originalPositions[i] + Vector2.left * slideDistance;

                if (element.rect != null)
                {
                    element.rect.anchoredPosition = Vector2.Lerp(originalPositions[i], target, t);
                }

                if (element.canvasGroup != null)
                {
                    element.canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }
            }

            if (mainMenuGroup != null)
            {
                mainMenuGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / duration));
            }

            if (finished) break;
            yield return null;
        }

        if (mainMenuGroup != null)
        {
            mainMenuGroup.alpha = 0f;
            mainMenuGroup.gameObject.SetActive(false);
        }

        StartGameplay();
        isTransitioning = false;
    }

    private void StartGameplay()
    {
        if (slider1 != null) slider1.SetActive(true);
        if (slider2 != null) slider2.SetActive(true);
        if (pintu != null) pintu.SetActive(true);
        if (timer != null) timer.SetActive(true);

        // Day tetap menyala saat masuk gameplay
        if (day != null) day.SetActive(true);

        if (playerController != null) playerController.enabled = true;
        if (gameplaySystemsController != null) gameplaySystemsController.StartGameplay();

        // Nyalakan waktu saat masuk ke game
        if (DayManager.instance != null) DayManager.instance.waktuBerjalan = true;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}