using UnityEngine;
using System.Collections.Generic;
public class CafeteriaFoodManager : BaseMinigameManager
{
    [Header("Cafetaria Settings")]
    public PlateDropZone[] allPlates;
    public DraggableItem[] allFoodDispensers;

    [Header("Menu Types")]
    public string[] availableFoodTypes = { "Burger", "Soda", "Apel", "Sup" };

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ResetMinigame()
    {       
        foreach (PlateDropZone plate in allPlates)
        {
            if (plate != null) plate.ClearPlateVisuals();
        }

        foreach (DraggableItem dispenser in allFoodDispensers)
        {
            if (dispenser != null) dispenser.ReturnToStart();
        }

        foreach (PlateDropZone plate in allPlates)
        {
            if (plate != null)
            {
                int totalItemsInPlate = Random.Range(1, 5);

                Dictionary<string, int> randomizedOrder = new Dictionary<string, int>();

                for (int i = 0; i < totalItemsInPlate; i++)
                {
                    string randomFoodType = availableFoodTypes[Random.Range(0, availableFoodTypes.Length)];

                    if (randomizedOrder.ContainsKey(randomFoodType))
                    {
                        randomizedOrder[randomFoodType]++; 
                    }
                    else
                    {
                        randomizedOrder[randomFoodType] = 1; 
                    }
                }

                plate.SetupComplexPlate(randomizedOrder);
            }
        }
    }

    public void CheckWinCondition()
    {
        bool allFull = true;
        foreach (PlateDropZone plate in allPlates)
        {
            if (plate != null && !plate.IsFull())
            {
                allFull = false;
                break;
            }
        }

        if (allFull) TriggerWinCondition();
    }
}

