using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Validation")]
    [Tooltip("ID dari benda (DraggableItem) atau kabel (WireDragItem) yang boleh masuk ke sini")]
    public string expectedItemID;

    [Tooltip("Masukkan script Manajer Minigame ke sini")]
    public MinigameDragManager minigameManager;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // -------------------------------------------------------------
            // SKENARIO 1: JIKA YANG DI-DROP ADALAH ITEM BIASA (Obat/Barang)
            // -------------------------------------------------------------
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                if (draggedItem.itemID == expectedItemID)
                {               
                    draggedItem.transform.SetParent(this.transform, false);
                    draggedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    draggedItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
                   
                    if (minigameManager != null)
                    {
                        minigameManager.AddCorrectMatch();
                    }
                }
                else
                {
                    Debug.Log("[DropZone] Barang salah! Memulangkan barang.");                    
                }
                return; 
            }

            // -------------------------------------------------------------
            // SKENARIO 2: JIKA YANG DI-DROP ADALAH KABEL (Wire Minigame)
            // -------------------------------------------------------------
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
                else
                {
                    Debug.Log("[DropZone] Kabel salah port! Memulangkan kabel.");                   
                }
                return;
            }
        }
    }
}