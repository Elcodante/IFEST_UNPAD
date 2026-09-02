using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    public string itemID;

    [Header("O2 Wind Mechanic")]
    public bool affectedByWind = false;
    public float dragTolerance = 2.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector2 originalPosition;
    [HideInInspector] public Transform originalParent;

    private bool isInitialized = false; // Pelindung Race Condition
    private bool isFailedDrag = false;

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
        isFailedDrag = false;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFailedDrag) return; 
        
        if (affectedByWind && O2WindManager.Instance != null && O2WindManager.Instance.isWindBlowing)
        {           
            if (eventData.delta.magnitude > dragTolerance)
            {
                Debug.Log("[O2 Minigame] GAGAL! Sampah tersedot angin.");
                isFailedDrag = true; 
                ReturnToStart();     
                return;
            }
        }

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

        gameObject.SetActive(true); // Pastikan item terlihat saat dikembalikan

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