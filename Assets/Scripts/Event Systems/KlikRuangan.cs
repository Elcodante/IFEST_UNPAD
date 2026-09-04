using UnityEngine;
using UnityEngine.EventSystems;

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
        AturKondisiCollider();

        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (areaSentuh != null && areaSentuh.enabled)
            {
                // Gunakan OverlapPointAll agar klik menembus tumpukan collider
                Collider2D[] semuaHit = Physics2D.OverlapPointAll(mousePos);
                foreach (var hit in semuaHit)
                {
                    if (hit == areaSentuh)
                    {
                        ProsesKlik();
                        break;
                    }
                }
            }
        }
    }

    private void AturKondisiCollider()
    {
        if (RoomManager.instance == null || areaSentuh == null) return;

        RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == idRuangan);
        bool adaZombie = false;

        ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
        foreach (var z in semuaZombie)
        {
            if (z.targetRoomID == idRuangan) adaZombie = true;
        }

        // LOGIKA TEMANMU: Collider ruangan HANYA NYALA jika ada zombie
        if (adaZombie || (dataRuang != null && dataRuang.currentState != RoomManager.RoomState.Aman))
        {
            areaSentuh.enabled = true;
        }
        else
        {
            areaSentuh.enabled = false;
        }
    }

    private void OnMouseDown()
    {
        if (areaSentuh != null && areaSentuh.enabled) ProsesKlik();
    }

    private void ProsesKlik()
    {
        if (RoomManager.instance == null) return;
        RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == idRuangan);

        if (dataRuang != null) dataRuang.currentState = RoomManager.RoomState.Diinvasi;

        if (SoldierManager.instance != null)
        {
            SoldierManager.instance.MunculkanUI(idRuangan);
        }
    }
}