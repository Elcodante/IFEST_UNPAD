using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class WireDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wire Properties")]
    public string itemID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector2 originalPosition;
    private float originalWidth;

    // Tambahan variabel untuk mencegah Race Condition
    private bool isInitialized = false;

    private void Awake()
    {
        InitData();
    }

    /// <summary>
    /// Mengambil komponen dan menyimpan data awal. 
    /// Dipisahkan dari Awake agar bisa dipanggil paksa oleh Manajer.
    /// </summary>
    private void InitData()
    {
        if (isInitialized) return; // Cegah inisialisasi ganda

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        originalPosition = rectTransform.anchoredPosition;
        originalWidth = rectTransform.sizeDelta.x;

        isInitialized = true;
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
        InitData(); // SANGAT PENTING: Pastikan data sudah terambil sebelum direset

        rectTransform.sizeDelta = new Vector2(originalWidth, rectTransform.sizeDelta.y);
        rectTransform.rotation = Quaternion.identity;

        canvasGroup.blocksRaycasts = true;
    }
}