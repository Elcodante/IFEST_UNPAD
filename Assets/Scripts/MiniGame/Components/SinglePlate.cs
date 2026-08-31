using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SinglePlate : MonoBehaviour, IDropHandler
{
    public CafeteriaOrderManager manager;

    [System.Serializable]
    public struct FoodVisualMapping
    {
        public string foodID; // Harus persis sama (misal: "Sup", "Apel")
        [Tooltip("Tarik objek UI gambar makanan yang sudah ada di dalam piring ke sini")]
        public GameObject foodVisualObject;
    }

    public FoodVisualMapping[] visualMappings;
    private Dictionary<string, GameObject> visualDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var mapping in visualMappings)
        {
            visualDict[mapping.foodID] = mapping.foodVisualObject;

            // Pastikan semua gambar makanan mati (tersembunyi) saat game baru mulai
            if (mapping.foodVisualObject != null)
            {
                mapping.foodVisualObject.SetActive(false);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        FoodDispenser dispenser = eventData.pointerDrag?.GetComponent<FoodDispenser>();

        if (dispenser != null)
        {
            // Cek ke manajer, apakah makanan ini valid (dibutuhkan oleh pesanan)?
            bool accepted = manager.TryAddFood(dispenser.foodID);

            if (accepted && visualDict.ContainsKey(dispenser.foodID))
            {
                // LOGIKA NYALA-MATI: Cukup aktifkan objeknya!
                // Sekalipun dipanggil 2x atau 3x, ia hanya akan memastikan objek ini tetap nyala (true).
                GameObject visual = visualDict[dispenser.foodID];
                if (visual != null)
                {
                    visual.SetActive(true);
                }
            }
        }
    }

    public void ClearPlate()
    {
        // Matikan semua visual makanan saat piring di-reset atau berganti ronde
        foreach (var visual in visualDict.Values)
        {
            if (visual != null)
            {
                visual.SetActive(false);
            }
        }
    }
}