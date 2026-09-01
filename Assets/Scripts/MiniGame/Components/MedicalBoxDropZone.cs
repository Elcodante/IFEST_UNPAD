using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MedicalBoxDropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Masukkan script Manajer Minigame Drag di sini")]
    public MinigameDragManager minigameManager;

    [System.Serializable]
    public struct MedicalVisual
    {
        public string itemID; // Harus sama dengan ID di DraggableItem (misal: "Perban")

        [Tooltip("Gambar barang di dalam rak P3K")]
        public GameObject visualObject;
    }

    public MedicalVisual[] visualMappings;
    private Dictionary<string, GameObject> visualDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var mapping in visualMappings)
        {
            visualDict[mapping.itemID] = mapping.visualObject;
        }
    }

    private void OnEnable()
    {
        // Matikan SEMUA gambar di dalam rak P3K setiap kali minigame dimulai/direset
        // Jika barangnya nanti di-drop, baru dinyalakan satu per satu.
        foreach (var visual in visualDict.Values)
        {
            if (visual != null)
            {
                visual.SetActive(false);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                // Cek apakah barang yang di-drag ada di daftar kotak P3K
                if (visualDict.ContainsKey(draggedItem.itemID))
                {
                    // 1. Sembunyikan barang yang sedang ditarik dari lantai/kursor
                    draggedItem.gameObject.SetActive(false);

                    // 2. Nyalakan gambar barang tersebut di dalam rak P3K
                    GameObject visual = visualDict[draggedItem.itemID];
                    if (visual != null)
                    {
                        visual.SetActive(true);
                    }

                    // 3. Lapor ke Manajer bahwa 1 barang berhasil dibereskan
                    if (minigameManager != null)
                    {
                        minigameManager.AddCorrectMatch();
                    }
                }
                else
                {
                    Debug.Log("[P3K] Barang ini bukan barang medis!");
                }
            }
        }
    }
}