using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class WireDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wire Properties")]
    public string itemID;

    [Header("Juice Effects")]
    [Tooltip("Masukkan objek Partikel Listrik di sini")]
    public GameObject sparkEffect;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector2 originalPosition;
    private float originalWidth;
    private bool isInitialized = false;

    private void Awake()
    {
        InitData();
    }

    private void InitData()
    {
        if (isInitialized) return;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        originalPosition = rectTransform.anchoredPosition;
        originalWidth = rectTransform.sizeDelta.x;

        isInitialized = true;
    }

    private void OnEnable()
    {
        // PASTIKAN LISTRIK MENYALA SEJAK AWAL KABEL MUNCUL
        if (sparkEffect != null) sparkEffect.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        Vector2 startScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rectTransform.position);

        float distance = Vector2.Distance(startScreenPos, mousePos);
        if (mainCanvas != null)
        {
            distance /= mainCanvas.scaleFactor;
        }

        rectTransform.sizeDelta = new Vector2(distance, rectTransform.sizeDelta.y);

        Vector2 direction = mousePos - startScreenPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;

        if (canvasGroup.blocksRaycasts == false)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        InitData();

        rectTransform.sizeDelta = new Vector2(originalWidth, rectTransform.sizeDelta.y);
        rectTransform.rotation = Quaternion.identity;

        canvasGroup.blocksRaycasts = true;

        // KEMBALIKAN LISTRIK MENYALA JIKA KABEL GAGAL TERSAMBUNG / DI-RESET
        if (sparkEffect != null) sparkEffect.SetActive(true);
    }
}