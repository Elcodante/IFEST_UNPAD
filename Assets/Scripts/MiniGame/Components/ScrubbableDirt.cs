using UnityEngine;
using UnityEngine.EventSystems;

public class ScrubbableDirt : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    [Header("Scrub Settings")]
    public float maxDirtHealth = 1000f;
    public int visualSteps = 3;

    [Header("Hardcore Mechanic")]
    public bool canRegrow = true;
    public float regrowRate = 100f;

    [Header("Juice Effects")]
    public float wobbleIntensity = 0.15f;
    [Tooltip("Masukkan objek Particle System debu/busa di sini")]
    public ParticleSystem scrubParticles; // --- TAMBAHAN PARTIKEL ---

    [HideInInspector]
    public MinigameScrubManager manager;

    private float currentHealth;
    private CanvasGroup canvasGroup;
    private bool isClean = false;
    private bool isInitialized = false;
    private Vector3 originalScale;

    private void Awake()
    {
        InitData();
    }

    private void InitData()
    {
        if (isInitialized) return;

        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isClean && canRegrow && currentHealth < maxDirtHealth)
        {
            currentHealth += regrowRate * Time.deltaTime;
            if (currentHealth > maxDirtHealth) currentHealth = maxDirtHealth;
            UpdateVisualState();
        }
    }

    public void ResetDirt()
    {
        InitData();
        currentHealth = maxDirtHealth;
        isClean = false;

        transform.localScale = originalScale;
        canvasGroup.blocksRaycasts = true;
        UpdateVisualState();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (isClean) return;

        // 1. Kurangi Health
        currentHealth -= eventData.delta.magnitude;

        // 2. Efek Kinetik (Micro-Wobble)
        float randomX = Random.Range(1f - wobbleIntensity, 1f + wobbleIntensity);
        float randomY = Random.Range(1f - wobbleIntensity, 1f + wobbleIntensity);
        transform.localScale = new Vector3(originalScale.x * randomX, originalScale.y * randomY, 1f);

        // --- 3. EFEK PARTIKEL BUSA/DEBU MENGKUTI KURSOR ---
        if (scrubParticles != null)
        {
            // Pindahkan emiter partikel tepat ke posisi kursor saat ini
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
            {
                scrubParticles.transform.position = worldPos;
            }

            // Tembakkan 1 hingga 3 partikel setiap kursor bergeser (tergantung kecepatan)
            scrubParticles.Emit(Random.Range(1, 4));
        }

        // 4. Perbarui Visual
        UpdateVisualState();

        // 5. Cek Kemenangan
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isClean = true;

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            transform.localScale = originalScale;

            if (manager != null)
            {
                manager.ChekWinCondition();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isClean) transform.localScale = originalScale;
    }

    private void UpdateVisualState()
    {
        if (isClean) return;
        float healthPercentage = currentHealth / maxDirtHealth;
        float steppedAlpha = Mathf.Ceil(healthPercentage * visualSteps) / visualSteps;
        canvasGroup.alpha = steppedAlpha;
    }

    public bool CheckIfClean()
    {
        return isClean;
    }
}