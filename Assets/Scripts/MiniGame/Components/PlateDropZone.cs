using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
public class PlateDropZone : MonoBehaviour, IDropHandler
{
    [Header("Validation")]
    public string expectedItemID;
    public CafeteriaFoodManager manager;

    [Header("UI Indicator")]
    public TextMeshProUGUI counterText;

    private Dictionary<string, int> requiredOrders = new Dictionary<string, int>();
    private Dictionary<string, int> currentOrders = new Dictionary<string, int>();

    public void SetupComplexPlate(Dictionary<string, int> newOrders)
    {
        requiredOrders = newOrders;
        currentOrders.Clear();

        foreach (string foodType in requiredOrders.Keys)
        {
            currentOrders[foodType] = 0;
        }

        UpdateUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem foodDispenser = eventData.pointerDrag.GetComponent<DraggableItem>();

            if (foodDispenser != null)
            {
                string incomingID = foodDispenser.itemID;

                if (requiredOrders.ContainsKey(incomingID))
                {
                    if (currentOrders[incomingID] < requiredOrders[incomingID])
                    {
                        currentOrders[incomingID]++;

                        GameObject foodClone = Instantiate(foodDispenser.gameObject, this.transform);
                        foodClone.GetComponent<CanvasGroup>().blocksRaycasts = false;
                        Destroy(foodClone.GetComponent<DraggableItem>());

                        UpdateUI();
                        if (manager != null) manager.CheckWinCondition();
                    }
                    else
                    {
                        Debug.Log($"[Piring] Kuota {incomingID} sudah penuh di piring ini!");
                    }
                }
                else
                {
                    Debug.Log($"[Piring] Piring ini sama sekali tidak memesan {incomingID}!");
                }

                foodDispenser.ReturnToStart();
            }
        }
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            string displayText = "";
       
            foreach (var order in requiredOrders)
            {
                string foodName = order.Key;
                int reqAmount = order.Value;
                int curAmount = currentOrders[foodName];

                displayText += $"{foodName}: {curAmount}/{reqAmount}\n";
            }
            counterText.text = displayText;
        }
    }

    public bool IsFull()
    {
        foreach (var order in requiredOrders)
        {
            if (currentOrders[order.Key] < order.Value) return false;
        }
        return true;
    }

    public void ClearPlateVisuals()
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponent<TextMeshProUGUI>() == null)
            {
                Destroy(child.gameObject);
            }
        }

    }
}
