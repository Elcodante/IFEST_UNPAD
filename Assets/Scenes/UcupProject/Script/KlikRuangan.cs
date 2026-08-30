using UnityEngine;
using UnityEngine.EventSystems;

public class KlikRuangan : MonoBehaviour
{
    public int idRuangan;
    private BoxCollider2D areaSentuh;

    void Start()
    {
        // Mengambil data kotak hijau di ruangan ini
        areaSentuh = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            // Mengubah posisi klik di layar komputermu menjadi titik koordinat 2D di dalam game
            Vector2 posisiMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Mengecek apakah titik klik tersebut berada tepat di dalam kotak hijau ruangan ini
            if (areaSentuh == Physics2D.OverlapPoint(posisiMouse))
            {
                Debug.Log("JURUS PAMUNGKAS BERHASIL: Ruangan " + idRuangan + " diklik!");

                if (SoldierManager.instance != null)
                {
                    SoldierManager.instance.MunculkanUI(idRuangan);
                }
                else
                {
                    Debug.LogError("Error: Soldier Manager belum terpasang di layar!");
                }
            }
        }
    }
}
