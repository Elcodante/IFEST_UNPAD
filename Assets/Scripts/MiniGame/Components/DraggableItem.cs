using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    public string itemID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector2 originalPosition;
    [HideInInspector] public Transform originalParent;

    private bool isInitialized = false; // Pelindung Race Condition

    private void Awake()
    {
        InitData();
    }

    // Fungsi untuk mengambil komponen secara aman
    private void InitData()
    {
        if (isInitialized) return;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        isInitialized = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mainCanvas != null)
            rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == originalParent)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        InitData(); // Pastikan sudah inisialisasi sebelum reset

        transform.SetParent(originalParent, false); // Tambah false agar skala tidak rusak
        rectTransform.anchoredPosition = originalPosition;

        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Fungsi untuk Manajer: Mengatur ulang posisi awal setelah diacak
    /// </summary>
    public void SetNewStartData(Transform newParent, Vector2 newPosition)
    {
        InitData(); // SANGAT PENTING: Paksa bangun sebelum menerima koordinat baru

        originalParent = newParent;
        rectTransform.anchoredPosition = newPosition;
        originalPosition = newPosition;
    }
}