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
        if (Time.timeScale == 0f) return;

        // Cegah klik tembus jika pemain sedang menekan UI/Tombol lain
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 posisiMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (areaSentuh == Physics2D.OverlapPoint(posisiMouse))
            {
                // FILTER BARU: Cek status ruangan di RoomManager
                if (RoomManager.instance != null)
                {
                    RoomManager.Room dataRuang = RoomManager.instance.rooms.Find(r => r.roomID == idRuangan);

                    // Hanya munculkan UI jika ruangan dalam kondisi 'Diinvasi'
                    if (dataRuang != null && dataRuang.currentState == RoomState.Diinvasi)
                    {
                        Debug.Log("AKSI: Ruangan " + idRuangan + " terinfeksi! Membuka UI Tentara.");

                        if (SoldierManager.instance != null)
                        {
                            SoldierManager.instance.MunculkanUI(idRuangan);
                        }
                    }
                    else
                    {
                        Debug.Log("ABORT: Ruangan " + idRuangan + " masih aman/bersih. UI tidak muncul.");
                    }
                }
            }
        }
    }
}