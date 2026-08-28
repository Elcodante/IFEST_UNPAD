using UnityEngine;
using UnityEngine.EventSystems;
public class ScrubbableDirt : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Scrub Settings")]
    public float maxDirtHealth = 1000f;

    [Header("Hardcore Menchanic")]
    public bool canRegrow = true;
    public float regrowRate = 100f;

    [HideInInspector]
    public MinigameScrubManager manager;

    private float currentHealth;
    private CanvasGroup canvasGroup;
    private bool isClean = false;
    private bool isInitialized = false;

    private void Awake()
    {
        InitData();
    }

    private void InitData()
    {
        if(isInitialized)
        {
            return;
        }
        canvasGroup = GetComponent<CanvasGroup>();
        isInitialized = true;
    }

    private void Update()
    {
        if (!isClean && canRegrow && currentHealth < maxDirtHealth)
        {
            currentHealth += regrowRate * Time.deltaTime;

            if (currentHealth > maxDirtHealth)
            {
                currentHealth = maxDirtHealth;
            }

            canvasGroup.alpha = currentHealth / maxDirtHealth;
        }
    }

    public void ResetDirt()
    {
        InitData();
        currentHealth = maxDirtHealth;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        isClean = false;
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (isClean)
        {
            return;
        }

        currentHealth -= eventData.delta.magnitude;

        canvasGroup.alpha = currentHealth / maxDirtHealth;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            isClean = true;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            if(manager != null)
            {
                manager.ChekWinCondition();
            }
        }
    }

    public bool CheckIfClean()
    {
        return isClean;
    }
}
