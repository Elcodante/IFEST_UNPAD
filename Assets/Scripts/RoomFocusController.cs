using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomFocusController : MonoBehaviour
{
    [System.Serializable]
    public class RoomFocusEntry
    {
        public RoomController room;
        public MinigameTrigger[] triggers;
    }

    [Header("Room Setup")]
    [Tooltip("Satu entry per room: RoomController-nya dan MinigameTrigger miliknya.")]
    [SerializeField] private RoomFocusEntry[] roomEntries;

    [Header("Focus Position")]
    [SerializeField] private Vector3 targetFocusPosition = Vector3.zero;
    [SerializeField] private float spacingBetweenTriggers = 2f;

    [Header("UI References")]
    [SerializeField] private GameObject cctvPanel;
    [SerializeField] private GameObject backButtonUI;

    [Header("Minigame Panels")]
    [SerializeField] private GameObject[] allMinigamePanels;

    private readonly Dictionary<MinigameTrigger, Vector3> originalPositions = new Dictionary<MinigameTrigger, Vector3>();
    private readonly List<MinigameTrigger> allTriggers = new List<MinigameTrigger>();
    private RoomFocusEntry currentFocusedEntry;

    private void Awake()
    {
        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry?.triggers == null) continue;

            foreach (MinigameTrigger trigger in entry.triggers)
            {
                if (trigger == null) continue;

                if (!originalPositions.ContainsKey(trigger))
                {
                    originalPositions[trigger] = trigger.transform.position;
                }

                if (!allTriggers.Contains(trigger))
                {
                    allTriggers.Add(trigger);
                }
            }
        }

        if (backButtonUI != null)
        {
            backButtonUI.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateStatusTombolRuangan();
    }

    /// <summary>
    /// Mengecek apakah ada zombie di ruangan. Jika ada, nonaktifkan interaksi tombol di CCTV
    /// agar pemain tidak bisa memilih minigame di ruangan tersebut.
    /// </summary>
    private void UpdateStatusTombolRuangan()
    {
        if (RoomManager.instance == null) return;

        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry == null || entry.room == null) continue;

            // Cari status ruangan di RoomManager berdasarkan RoomID
            RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == entry.room.RoomID);

            if (dataRuang != null)
            {
                bool adaZombie = (dataRuang.currentState == RoomManager.RoomState.Diinvasi ||
                                  dataRuang.currentState == RoomManager.RoomState.Hancur);

                // Jika zombie muncul saat pemain sedang fokus di ruangan ini, kembalikan ke CCTV
                if (adaZombie && currentFocusedEntry == entry)
                {
                    Debug.Log($"Ruangan {entry.room.RoomName} diserang zombie! Pemain dikembalikan ke CCTV.");
                    ReturnToCCTV();
                }

                // Matikan / hidupkan Button UI pada RoomController
                Button btn = entry.room.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = !adaZombie;
                }
            }
        }
    }

    public void FocusRoom(RoomController room)
    {
        if (room == null) return;

        RoomFocusEntry entry = System.Array.Find(roomEntries, e => e.room == room);

        if (entry == null)
        {
            Debug.LogWarning($"[RoomFocusController] Tidak ada RoomFocusEntry untuk room '{room.RoomName}'.");
            return;
        }

        FocusEntry(entry);
    }

    public void FocusRoomByName(string roomName)
    {
        RoomFocusEntry entry = System.Array.Find(roomEntries, e => e.room != null && e.room.RoomName == roomName);

        if (entry == null)
        {
            Debug.LogWarning($"[RoomFocusController] Tidak ada RoomFocusEntry dengan roomName '{roomName}'.");
            return;
        }

        FocusEntry(entry);
    }

    private void FocusEntry(RoomFocusEntry entry)
    {
        // Cegah masuk jika ruangan sedang diserang zombie
        if (RoomManager.instance != null && entry.room != null)
        {
            RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == entry.room.RoomID);
            if (dataRuang != null && dataRuang.currentState != RoomManager.RoomState.Aman)
            {
                Debug.LogWarning($"Akses minigame ditolak! Ruangan {entry.room.RoomName} sedang ada zombie.");
                return;
            }
        }

        currentFocusedEntry = entry;

        // Sembunyikan semua trigger
        foreach (MinigameTrigger trigger in allTriggers)
        {
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }
        }

        // Pindahkan trigger room terpilih ke depan kamera
        if (entry.triggers != null)
        {
            for (int i = 0; i < entry.triggers.Length; i++)
            {
                MinigameTrigger trigger = entry.triggers[i];
                if (trigger == null) continue;

                trigger.gameObject.SetActive(true);

                float offsetX = (i - (entry.triggers.Length - 1) / 2f) * spacingBetweenTriggers;
                Vector3 destination = targetFocusPosition + new Vector3(offsetX, 0f, 0f);

                trigger.transform.position = destination;
            }
        }

        if (cctvPanel != null)
        {
            cctvPanel.SetActive(false);
        }

        if (backButtonUI != null)
        {
            backButtonUI.SetActive(true);
        }
    }

    public void ReturnToCCTV()
    {
        if (allMinigamePanels != null)
        {
            foreach (GameObject panel in allMinigamePanels)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
        }

        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry?.triggers == null) continue;

            bool roomStillUnderAttack = entry.room != null && entry.room.CurrentStatus == RoomStatus.Diserang;
            MinigameTrigger activeTrigger = entry.room != null ? entry.room.CurrentActiveTrigger : null;

            foreach (MinigameTrigger trigger in entry.triggers)
            {
                if (trigger == null) continue;

                trigger.gameObject.SetActive(true);

                if (originalPositions.TryGetValue(trigger, out Vector3 originalPos))
                {
                    trigger.transform.position = originalPos;
                }

                if (roomStillUnderAttack && trigger == activeTrigger && !trigger.IsDangerActive)
                {
                    trigger.ActivateDanger();
                }
            }
        }

        currentFocusedEntry = null;

        if (cctvPanel != null)
        {
            cctvPanel.SetActive(true);
        }

        if (backButtonUI != null)
        {
            backButtonUI.SetActive(false);
        }
    }
}