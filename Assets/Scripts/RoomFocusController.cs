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

    private void UpdateStatusTombolRuangan()
    {
        if (RoomManager.instance == null) return;

        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry == null || entry.room == null) continue;

            RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == entry.room.RoomID);

            if (dataRuang != null)
            {
                bool adaZombie = (dataRuang.currentState == RoomManager.RoomState.Diinvasi ||
                                  dataRuang.currentState == RoomManager.RoomState.Hancur);

                if (adaZombie && currentFocusedEntry == entry)
                {
                    Debug.Log($"Ruangan {entry.room.RoomName} diserang zombie! Pemain dikembalikan ke CCTV.");
                    ReturnToCCTV();
                }

                // Matikan SEMUA komponen Button jika ada zombie
                Button[] semuaTombol = entry.room.GetComponentsInChildren<Button>(true);
                foreach (Button btn in semuaTombol)
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
        // --- GEMBOK BAJA: CEK FISIK ZOMBIE ---
        if (entry.room != null)
        {
            int idTarget = entry.room.RoomID;

            ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
            foreach (var z in semuaZombie)
            {
                if (z.targetRoomID == idTarget)
                {
                    Debug.Log($"[DITOLAK] Ruangan {entry.room.RoomName} sedang ada zombie! Transisi interior digagalkan paksa.");
                    return;
                }
            }
        }
        // ------------------------------------

        currentFocusedEntry = entry;

        foreach (MinigameTrigger trigger in allTriggers)
        {
            if (trigger != null) trigger.gameObject.SetActive(false);
        }

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

        if (cctvPanel != null) cctvPanel.SetActive(false);
        if (backButtonUI != null) backButtonUI.SetActive(true);

        // PERBAIKAN: Sembunyikan pintu & zombie
        AturVisualDunia(false);
    }

    public void ReturnToCCTV()
    {
        if (allMinigamePanels != null)
        {
            foreach (GameObject panel in allMinigamePanels)
            {
                if (panel != null) panel.SetActive(false);
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

        if (cctvPanel != null) cctvPanel.SetActive(true);
        if (backButtonUI != null) backButtonUI.SetActive(false);

        // PERBAIKAN: Tampilkan kembali pintu & zombie
        AturVisualDunia(true);
    }

    private void AturVisualDunia(bool tampilkan)
    {
        foreach (var pintu in PintuController.semuaPintu)
        {
            if (pintu != null)
            {
                SpriteRenderer sr = pintu.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = tampilkan;
            }
        }

        ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
        foreach (var z in semuaZombie)
        {
            if (z != null && z.lokasiSpawn != null)
            {
                z.lokasiSpawn.gameObject.SetActive(tampilkan);
            }
        }
    }
}