using UnityEngine;
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

        // 1. Cari data ruangan di RoomManager
        RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == idRuangan);
        string statusRuang = (dataRuang != null) ? dataRuang.currentState.ToString() : "TIDAK TERDAFTAR";

        // 2. Cari semua zombie di scene dan catat target ruangannya
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

        // 3. Syarat membuka panel: jika ruangan Diinvasi ATAU ada zombie yang menargetkan ruangan ini
        if (adaZombieDiRuanganIni || (dataRuang != null && dataRuang.currentState == RoomState.Diinvasi))
        {
            if (dataRuang != null) dataRuang.currentState = RoomState.Diinvasi;

            if (SoldierManager.instance != null)
            {
                Debug.Log($"<color=green>[BERHASIL]</color> Membuka Panel Tentara untuk Room_{idRuangan}!");
                SoldierManager.instance.MunculkanUI(idRuangan);
            }
            else
            {
                Debug.LogError("[ERROR] SoldierManager.instance bernilai NULL!");
            }
        }
        else
        {
            Debug.LogWarning($"[DITOLAK] Kamu mengklik Room_{idRuangan}, padahal zombie sedang berada di ruangan lain!");
        }
        Debug.Log($"<color=white>==========================================</color>");
    }
}