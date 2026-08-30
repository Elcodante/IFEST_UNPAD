using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menangani perpindahan tampilan dari panel CCTV utama ke "fokus" pada satu room.
/// Alih-alih menggerakkan kamera, script ini memindahkan MinigameTrigger milik room
/// yang dipilih ke posisi di depan kamera (targetFocusPosition), menyembunyikan
/// trigger milik room lain supaya tidak ikut terlihat, dan mengembalikan semuanya
/// ke posisi/status asal saat player kembali ke CCTV.
///
/// Pasang script ini pada satu GameObject saja di scene (mis. "Room Focus Controller"),
/// lalu isi array roomEntries lewat Inspector: setiap RoomController dipasangkan dengan
/// 2 MinigameTrigger miliknya (harus sama persis dengan yang diisi di field
/// "Minigame Triggers" pada RoomController itu).
/// </summary>
public class RoomFocusController : MonoBehaviour
{
    [System.Serializable]
    public class RoomFocusEntry
    {
        public RoomController room;
        public MinigameTrigger[] triggers;
    }

    [Header("Room Setup")]
    [Tooltip("Satu entry per room: RoomController-nya, dan MinigameTrigger yang akan dipindahkan ke depan kamera saat room ini difokus.")]
    [SerializeField] private RoomFocusEntry[] roomEntries;

    [Header("Focus Position")]
    [Tooltip("Posisi di depan kamera tempat trigger akan ditempatkan saat room difokus.")]
    [SerializeField] private Vector3 targetFocusPosition = Vector3.zero;

    [Tooltip("Jarak antar trigger saat keduanya ditampilkan bersamaan di posisi fokus, supaya tidak saling menumpuk.")]
    [SerializeField] private float spacingBetweenTriggers = 2f;

    [Header("UI References")]
    [Tooltip("Panel CCTV utama (berisi tombol-tombol room). Akan disembunyikan saat fokus aktif.")]
    [SerializeField] private GameObject cctvPanel;

    [Tooltip("Opsional: tombol atau panel 'Kembali ke CCTV' yang muncul hanya saat sedang fokus ke room.")]
    [SerializeField] private GameObject backButtonUI;

    [Header("Minigame Panels")]
    [Tooltip("Semua Panel_Minigame_* yang ada di scene. Dipakai supaya tombol Exit/Kembali bisa menutup minigame yang sedang terbuka, dari mana saja, kapan saja (termasuk saat exit di tengah permainan).")]
    [SerializeField] private GameObject[] allMinigamePanels;

    private readonly Dictionary<MinigameTrigger, Vector3> originalPositions = new Dictionary<MinigameTrigger, Vector3>();
    private readonly List<MinigameTrigger> allTriggers = new List<MinigameTrigger>();
    private RoomFocusEntry currentFocusedEntry;

    private void Awake()
    {
        // Simpan posisi asal semua trigger di awal, sebelum ada yang dipindah,
        // dan kumpulkan semua trigger jadi satu daftar supaya mudah disembunyikan/ditampilkan.
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

    /// <summary>
    /// Dipanggil dari tombol room di panel CCTV (mis. lewat OnClick Button -> RoomFocusController.FocusRoom).
    /// Karena UnityEvent Button tidak bisa langsung mengirim parameter RoomController secara umum di Inspector
    /// tanpa referensi spesifik, sediakan juga varian FocusRoomByName di bawah untuk dipanggil dengan string.
    /// </summary>
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

    /// <summary>
    /// Alternatif pemanggilan lewat nama room (roomName di RoomController), supaya bisa langsung
    /// di-assign di Inspector Button.OnClick tanpa perlu drag referensi RoomController secara manual.
    /// </summary>
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
        currentFocusedEntry = entry;

        // Sembunyikan dulu SEMUA trigger di scene, supaya hanya milik room yang difokus yang terlihat.
        foreach (MinigameTrigger trigger in allTriggers)
        {
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }
        }

        // Tampilkan dan pindahkan trigger milik room yang difokus ke depan kamera.
        if (entry.triggers != null)
        {
            for (int i = 0; i < entry.triggers.Length; i++)
            {
                MinigameTrigger trigger = entry.triggers[i];
                if (trigger == null) continue;

                trigger.gameObject.SetActive(true);

                // Sebar horizontal supaya 2 trigger tidak saling menumpuk persis di titik yang sama.
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

    /// <summary>
    /// Dipanggil dari tombol "Kembali"/"Exit" untuk kembali ke panel CCTV - baik dari tampilan
    /// room (belum buka minigame) maupun dari tengah-tengah minigame yang sedang dimainkan.
    /// Menutup semua panel minigame yang mungkin sedang terbuka, mengembalikan semua trigger
    /// ke posisi asalnya di denah, dan menampilkan lagi panel CCTV.
    /// </summary>
public void ReturnToCCTV()
    {
        // Tutup semua panel minigame yang mungkin sedang aktif, apa pun yang sedang dimainkan.
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

        // Kembalikan semua trigger ke posisi asal dan tampilkan lagi. Kalau room pemilik trigger
        // itu masih berstatus Diserang dan trigger tersebut adalah trigger yang sedang aktif untuk
        // serangan ini (CurrentActiveTrigger, dipilih random oleh RoomController), nyalakan ulang
        // warning-nya (ActivateDanger) supaya player tetap bisa lanjut mengerjakan minigame yang
        // SAMA nanti - bukan menyalakan kedua trigger sekaligus.
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
