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

            WireDragItem wireItem = eventData.pointerDrag.GetComponent<WireDragItem>();
            if (wireItem != null)
            {
                if (wireItem.itemID == expectedItemID)
                {
                    wireItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    wireItem.enabled = false;

                    if (minigameManager != null)
                    {
                        minigameManager.AddCorrectMatch();
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
            currentItem.ReturnToStart();
            currentItem = null;

            if (minigameManager != null)
            {
                if (!(minigameManager is GeneratorGearManager))
                {
                    minigameManager.RemoveCorrectMatch();
                }
                else
                {
                    ((GeneratorGearManager)minigameManager).CheckGeneratorWinCondition();
                }
            }
        }
    }
}