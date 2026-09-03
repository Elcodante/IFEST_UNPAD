using UnityEngine;
using UnityEngine.EventSystems;
using static RoomManager;

public class KlikRuangan : MonoBehaviour
{
    public int idRuangan;
    private BoxCollider2D areaSentuh;

    void Start()
    {
        areaSentuh = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (areaSentuh != null && areaSentuh == Physics2D.OverlapPoint(mousePos))
            {
                ProsesKlik();
            }
        }
    }

    private void OnMouseDown()
    {
        ProsesKlik();
    }

    private void ProsesKlik()
    {
        Debug.Log($"<color=white>==========================================</color>");
        Debug.Log($"[KLIK] Kamu mengklik: <b>Room_{idRuangan}</b>");

        if (RoomManager.instance == null)
        {
            Debug.LogError("[ERROR] RoomManager.instance tidak ditemukan!");
            return;
        }

        RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == idRuangan);
        string statusRuang = (dataRuang != null) ? dataRuang.currentState.ToString() : "TIDAK TERDAFTAR";

        ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
        bool adaZombieDiRuanganIni = false;

        string infoSemuaZombie = "";
        foreach (var z in semuaZombie)
        {
            infoSemuaZombie += $"[Zombie di Room_{z.targetRoomID}] ";
            if (z.targetRoomID == idRuangan)
            {
                adaZombieDiRuanganIni = true;
            }
        }

        if (semuaZombie.Length == 0) infoSemuaZombie = "TIDAK ADA ZOMBIE SAMA SEKALI DI SCENE";
        Debug.Log($"[STATUS SCENE] Daftar Zombie Aktif: {infoSemuaZombie}");
        Debug.Log($"[STATUS RUANGAN] Room_{idRuangan} status di RoomManager: <b>{statusRuang}</b>");

        // PERBAIKAN: Logika Prioritas Zombie! Jika ada zombie, paksa panggil tentara!
        if (adaZombieDiRuanganIni || (dataRuang != null && dataRuang.currentState != RoomState.Aman))
        {
            Debug.Log($"<color=green>[PRIORITAS]</color> Ruangan {idRuangan} diserang! Akses interior ditutup, memanggil UI Tentara.");

            if (dataRuang != null) dataRuang.currentState = RoomState.Diinvasi;

            if (SoldierManager.instance != null)
            {
                SoldierManager.instance.MunculkanUI(idRuangan);
            }
            return; // Hentikan script di sini agar tidak memicu transisi minigame!
        }
        else
        {
            Debug.Log($"[AMAN] Ruangan {idRuangan} tidak memiliki zombie. Klik diteruskan ke sistem Minigame.");
        }

        Debug.Log($"<color=white>==========================================</color>");
    }
}