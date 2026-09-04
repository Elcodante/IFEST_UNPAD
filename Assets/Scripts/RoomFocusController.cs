using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomFocusController : MonoBehaviour
{
    public static RoomFocusController Instance { get; private set; }

    [System.Serializable]
    public class RoomFocusEntry
    {
        public RoomController room;
        public MinigameTrigger[] triggers;

        [Header("Room Visuals")]
        [Tooltip("Masukkan GameObject UI Image background untuk ruangan ini di sini.")]
        public GameObject roomBackground;
    }

    [Header("Room Setup")]
    [Tooltip("Satu entry per room: RoomController-nya, MinigameTrigger miliknya, dan Background-nya.")]
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

    private bool sedangDiInterior = false;
    private Coroutine pengawasVisual;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry.roomBackground != null)
            {
                entry.roomBackground.SetActive(false);
            }

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

        if (backButtonUI != null) backButtonUI.SetActive(false);
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
                    ReturnToCCTV();
                }

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
        if (entry == null) return;
        FocusEntry(entry);
    }

    public void FocusRoomByName(string roomName)
    {
        RoomFocusEntry entry = System.Array.Find(roomEntries, e => e.room != null && e.room.RoomName == roomName);
        if (entry == null) return;
        FocusEntry(entry);
    }

    private void FocusEntry(RoomFocusEntry entry)
    {
        if (entry.room != null)
        {
            int idTarget = entry.room.RoomID;
            ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
            foreach (var z in semuaZombie)
            {
                if (z.targetRoomID == idTarget) return;
            }
        }

        foreach (RoomFocusEntry e in roomEntries)
        {
            if (e.roomBackground != null) e.roomBackground.SetActive(false);
        }

        currentFocusedEntry = entry;

        if (entry.roomBackground != null)
        {
            entry.roomBackground.SetActive(true);
        }

        if (SoldierManager.instance != null)
        {
            SoldierManager.instance.TutupUI();
        }

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

        sedangDiInterior = true;
        AturVisualDunia(false);

        if (pengawasVisual != null) StopCoroutine(pengawasVisual);
        pengawasVisual = StartCoroutine(AwasiVisualInterior());
    }

    /// <summary>
    /// Menyelesaikan serangan pada ruangan yang sedang difokuskan,
    /// mengubah statusnya menjadi Aman, lalu kembali ke CCTV.
    /// </summary>
    public void SelesaikanMinigameRuanganAktif()
    {
        if (currentFocusedEntry != null && currentFocusedEntry.room != null)
        {
            currentFocusedEntry.room.ResolveAttack();
        }

        ReturnToCCTV();
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
            if (entry.roomBackground != null)
            {
                entry.roomBackground.SetActive(false);
            }

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

        sedangDiInterior = false;
        if (pengawasVisual != null) StopCoroutine(pengawasVisual);

        AturVisualDunia(true);

        if (DayManager.instance != null)
        {
            DayManager.instance.LanjutkanSistemWaktu();
        }
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
            if (z != null && z.lokasiSpawn != null) z.lokasiSpawn.gameObject.SetActive(tampilkan);
        }

        SoldierController[] semuaTentara = Object.FindObjectsByType<SoldierController>(FindObjectsSortMode.None);
        foreach (var s in semuaTentara)
        {
            if (s != null)
            {
                SpriteRenderer[] sprites = s.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in sprites) sr.enabled = tampilkan;

                Canvas[] canvases = s.GetComponentsInChildren<Canvas>();
                foreach (Canvas c in canvases) c.enabled = tampilkan;
            }
        }
    }

    private IEnumerator AwasiVisualInterior()
    {
        while (sedangDiInterior)
        {
            ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
            foreach (var z in semuaZombie)
            {
                if (z != null && z.lokasiSpawn != null && z.lokasiSpawn.gameObject.activeSelf)
                {
                    z.lokasiSpawn.gameObject.SetActive(false);
                }
            }

            SoldierController[] semuaTentara = Object.FindObjectsByType<SoldierController>(FindObjectsSortMode.None);
            foreach (var s in semuaTentara)
            {
                if (s != null)
                {
                    SpriteRenderer[] sprites = s.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer sr in sprites) sr.enabled = false;

                    Canvas[] canvases = s.GetComponentsInChildren<Canvas>();
                    foreach (Canvas c in canvases) c.enabled = false;
                }
            }

            foreach (RoomFocusEntry rfe in roomEntries)
            {
                if (rfe != currentFocusedEntry && rfe.triggers != null)
                {
                    foreach (var t in rfe.triggers)
                    {
                        if (t != null && t.gameObject.activeSelf)
                        {
                            t.gameObject.SetActive(false);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}