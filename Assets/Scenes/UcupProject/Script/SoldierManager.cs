using UnityEngine;

public class SoldierManager : MonoBehaviour
{
    public static SoldierManager instance;
    public GameObject panelUI;
    public int ruanganTerpilih;

    public GameObject prefabTentara; // Wadah cetakan Titik Hijau

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelUI != null) panelUI.SetActive(false);
    }

    public void MunculkanUI(int idRuangan)
    {
        ruanganTerpilih = idRuangan;
        panelUI.SetActive(true);
    }

    public void TutupUI()
    {
        panelUI.SetActive(false);
    }

    public void TombolKirimDitekan()
    {
        panelUI.SetActive(false);

        // Cari tahu letak koordinat ruangan yang dipilih dari RoomManager
        if (RoomManager.instance != null)
        {
            RoomManager.Room targetRuang = RoomManager.instance.rooms.Find(r => r.roomID == ruanganTerpilih);

            if (targetRuang != null && prefabTentara != null)
            {
                Debug.Log("Tentara meluncur ke Ruangan: " + ruanganTerpilih);

                Vector3 posisiBersebelahan = targetRuang.lokasiRuangan.position + new Vector3(0.5f, 0f, 0f);
                GameObject tentaraBaru = Instantiate(prefabTentara, posisiBersebelahan, Quaternion.identity);

                // Beritahu tentara ini dia sedang berada di ruangan mana
                SoldierController sc = tentaraBaru.GetComponent<SoldierController>();
                if (sc != null) sc.myRoomID = ruanganTerpilih;
            }
        }
    }
}
