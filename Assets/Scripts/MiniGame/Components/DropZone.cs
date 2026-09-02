using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Validation")]
    public string expectedItemID;
    public MinigameDragManager minigameManager;
    public bool hideItemOnDrop = false;
    [Header("Hazard Link")]
    public GearJamHazard myHazard;

    [Header("Juice Effects")]
    public float rotationSpeed = -150f; // Kecepatan putar (Minus = searah jarum jam)

    [HideInInspector]
    public DraggableItem currentItem;

    private void Awake()
    {
        // AUTO-LINK: Otomatis menyambungkan DropZone ini ke Kerikil
        // agar kamu tidak perlu repot men-drag di Inspector satu per satu.
        if (myHazard != null)
        {
            myHazard.parentDropZone = this;
        }
    }

    private void Update()
    {
        // JUICE 1: ROTASI DINAMIS
        // Jika gerigi terpasang DAN (tidak ada kerikil ATAU kerikil sedang mati)
        if (currentItem != null && (myHazard == null || !myHazard.gameObject.activeInHierarchy))
        {
            // Putar gerigi secara visual. Saat kerikil muncul, rotasi otomatis terhenti (macet!)
            currentItem.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                if (draggedItem.itemID == expectedItemID)
                {
                    draggedItem.transform.SetParent(this.transform, false);
                    draggedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    draggedItem.GetComponent<CanvasGroup>().blocksRaycasts = false;

                    currentItem = draggedItem;

                    if(hideItemOnDrop)
                    {
                        draggedItem.gameObject.SetActive(false);
                    }

                    if (minigameManager != null)
                    {
                        if (minigameManager is GeneratorGearManager gearManager)
                        {
                            gearManager.CheckGeneratorWinCondition();
                        }
                        else
                        {
                            minigameManager.AddCorrectMatch();
                        }
                    }
                }
                else
                {
                    Debug.Log("[DropZone] Barang salah! Memulangkan barang.");
                }
                return;
            }

            // ... (Kode DropZone sebelumnya tetap sama, fokus pada Skenario 2) ...

            WireDragItem wireItem = eventData.pointerDrag.GetComponent<WireDragItem>();
            if (wireItem != null)
            {
                if (wireItem.itemID == expectedItemID)
                {
                    wireItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    wireItem.enabled = false;

                    // MATIKAN LISTRIK KARENA SUDAH TERSAMBUNG AMAN
                    if (wireItem.sparkEffect != null)
                    {
                        wireItem.sparkEffect.SetActive(false);
                    }

                    if (minigameManager != null)
                    {
                        minigameManager.AddCorrectMatch();
                    }
                }
                else
                {
                    Debug.Log("[DropZone] KORSLETING! Kabel salah port.");

                    // --- EFEK LAYAR BERGETAR SAAT SALAH ---
                    if (UIShaker.Instance != null)
                    {
                        UIShaker.Instance.Shake(0.3f, 15f); // Getar 0.3 detik
                    }
                }
                return;
            }
        }
    }

    public void EjectItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"[DropZone] Memuntahkan {currentItem.name}!");

            // JUICE 3: HUKUMAN KINETIK (Layar bergetar keras saat gerigi terlempar)
            if (UIShaker.Instance != null)
            {
                UIShaker.Instance.Shake(0.4f, 25f);
            }

            currentItem.ReturnToStart();
            currentItem = null;

            if (minigameManager != null)
            {
                if (!(minigameManager is GeneratorGearManager))
                    minigameManager.RemoveCorrectMatch();
                else
                    ((GeneratorGearManager)minigameManager).CheckGeneratorWinCondition();
            }
        }
    }
}