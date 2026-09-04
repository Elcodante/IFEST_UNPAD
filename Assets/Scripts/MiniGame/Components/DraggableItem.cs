using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    public string itemID;

    [Header("Audio Settings")]
    [Tooltip("ID suara di AudioManager saat barang ini disentuh (misal: SFX_Ambil_Obat, SFX_Ambil_Kertas)")]
    public string pickupSoundID = "SFX_Ambil";

    [Header("O2 Wind Mechanic")]
    public bool affectedByWind = false;
    public float dragTolerance = 2.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector2 originalPosition;
    private Vector3 originalScale; // Menyimpan skala asli
    [HideInInspector] public Transform originalParent;

    private bool isInitialized = false;
    private bool isFailedDrag = false;

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
        originalScale = rectTransform.localScale;
        originalParent = transform.parent;

        isInitialized = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isFailedDrag = false;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSoundID))
        {
            // Play dengan Random Pitch agar suaranya bervariasi setiap kali diambil
            AudioManager.Instance.PlaySFXRandomPitch(pickupSoundID, 0.9f, 1.1f);
        }
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

                // JUICE: Getarkan layar saat sampah terlepas dari tangan
                if (UIShaker.Instance != null) UIShaker.Instance.Shake(0.2f, 15f);

                // JUICE: Mainkan animasi terbang tersapu angin
                StartCoroutine(BlowAwayAnimation());
                return;
            }
        }

        if (mainCanvas != null)
            rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isFailedDrag) return; // Jangan jalankan kode ini jika sedang terbang tersapu angin

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == originalParent)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        InitData();

        gameObject.SetActive(true);
        transform.SetParent(originalParent, false);

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        rectTransform.rotation = Quaternion.identity;

        canvasGroup.blocksRaycasts = true;
    }

    public void SetNewStartData(Transform newParent, Vector2 newPosition)
    {
        InitData();

        originalParent = newParent;
        rectTransform.anchoredPosition = newPosition;
        originalPosition = newPosition;
    }

    // --- COROUTINE ANIMASI TERSAPU ANGIN ---
    private IEnumerator BlowAwayAnimation()
    {
        float duration = 0.4f;
        float time = 0;
        Vector3 startScale = rectTransform.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;

            // 1. Putar dengan sangat cepat (berantakan)
            rectTransform.Rotate(0, 0, 1500f * Time.deltaTime);

            // 2. Mengecil seolah-olah terbang menjauh tertiup angin
            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, time / duration);

            // 3. Tersapu lurus ke arah kanan layar
            rectTransform.anchoredPosition += new Vector2(30f, 0f) * (time / duration);

            yield return null;
        }

        // Setelah animasi terbang selesai, reset barang kembali ke posisi awal
        ReturnToStart();
        canvasGroup.alpha = 1f;
    }
}