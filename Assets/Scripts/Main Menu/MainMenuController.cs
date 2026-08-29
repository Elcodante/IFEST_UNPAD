using System.Collections;
using UnityEngine;

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

    [Header("Transition")]
    [SerializeField] private float duration = 0.55f;
    [SerializeField] private float slideDistance = 450f;

    [Header("Gameplay")]
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private GameplaySystemsController gameplaySystemsController;

    private Vector2[] originalPositions;
    private bool isTransitioning;

    private void Awake()
    {
        originalPositions = new Vector2[elements.Length];

        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null && elements[i].rect != null)
            {
                originalPositions[i] =
                    elements[i].rect.anchoredPosition;
            }
        }
    }

    private void Start()
    {
        if (playerController != null)
            playerController.enabled = false;
    }

    public void PlayGame()
    {
        if (isTransitioning)
            return;

        StartCoroutine(HideMenu());
        gameplaySystemsController.StartGameplay();
    }

    private IEnumerator HideMenu()
    {
        isTransitioning = true;

        mainMenuGroup.interactable = false;
        mainMenuGroup.blocksRaycasts = false;

        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.unscaledDeltaTime;

            bool finished = true;

            for (int i = 0; i < elements.Length; i++)
            {
                MenuElement element = elements[i];

                if (element == null)
                    continue;

                float t =
                    Mathf.Clamp01(
                        (elapsed - element.delay) / duration
                    );

                if (t < 1f)
                    finished = false;

                t = EaseOutCubic(t);

                Vector2 target =
                    originalPositions[i] +
                    Vector2.left * slideDistance;

                if (element.rect != null)
                {
                    element.rect.anchoredPosition =
                        Vector2.Lerp(
                            originalPositions[i],
                            target,
                            t
                        );
                }

                if (element.canvasGroup != null)
                {
                    element.canvasGroup.alpha =
                        Mathf.Lerp(1f, 0f, t);
                }
            }

            // Fade keseluruhan menu sedikit lebih cepat
            mainMenuGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.Clamp01(elapsed / duration)
                );

            if (finished)
                break;

            yield return null;
        }

        mainMenuGroup.alpha = 0f;
        mainMenuGroup.gameObject.SetActive(false);

        StartGameplay();
    }

    private void StartGameplay()
    {
        if (playerController != null)
            playerController.enabled = true;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}