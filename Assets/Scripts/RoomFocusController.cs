using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menangani perpindahan tampilan dari panel CCTV utama ke "fokus" pada satu room.
/// Alih-alih menggerakkan kamera, script ini memindahkan MinigameTrigger milik room
/// yang dipilih ke posisi di depan kamera (targetFocusPosition), lalu mengembalikannya
/// ke posisi asal saat player kembali ke CCTV.
///
/// Pasang script ini pada satu GameObject saja di scene (mis. "Game Manager" atau
/// GameObject baru "Room Focus Controller"), lalu isi array roomEntries lewat Inspector:
/// setiap RoomController dipasangkan dengan 2 MinigameTrigger miliknya (harus sama
/// persis dengan yang diisi di field "Minigame Triggers" pada RoomController itu).
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

    private readonly Dictionary<MinigameTrigger, Vector3> originalPositions = new Dictionary<MinigameTrigger, Vector3>();
    private RoomFocusEntry currentFocusedEntry;

    private void Awake()
    {
        // Simpan posisi asal semua trigger di awal, sebelum ada yang dipindah.
        foreach (RoomFocusEntry entry in roomEntries)
        {
            if (entry?.triggers == null) continue;

            foreach (MinigameTrigger trigger in entry.triggers)
            {
                if (trigger != null && !originalPositions.ContainsKey(trigger))
                {
                    originalPositions[trigger] = trigger.transform.position;
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
        // Kalau lagi fokus ke room lain, kembalikan dulu triggernya ke posisi asal.
        if (currentFocusedEntry != null && currentFocusedEntry != entry)
        {
            RestoreEntryPositions(currentFocusedEntry);
        }

        currentFocusedEntry = entry;

        if (entry.triggers != null)
        {
            for (int i = 0; i < entry.triggers.Length; i++)
            {
                MinigameTrigger trigger = entry.triggers[i];
                if (trigger == null) continue;

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
    /// Dipanggil dari tombol "Kembali" untuk kembali ke panel CCTV. Mengembalikan
    /// semua trigger yang sedang difokus ke posisi asalnya di denah.
    /// </summary>
    public void ReturnToCCTV()
    {
        if (currentFocusedEntry != null)
        {
            RestoreEntryPositions(currentFocusedEntry);
            currentFocusedEntry = null;
        }

        if (cctvPanel != null)
        {
            cctvPanel.SetActive(true);
        }

        if (backButtonUI != null)
        {
            backButtonUI.SetActive(false);
        }
    }

    private void RestoreEntryPositions(RoomFocusEntry entry)
    {
        if (entry?.triggers == null) return;

        foreach (MinigameTrigger trigger in entry.triggers)
        {
            if (trigger != null && originalPositions.TryGetValue(trigger, out Vector3 originalPos))
            {
                trigger.transform.position = originalPos;
            }
        }
    }
}
