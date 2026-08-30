using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SoldierManager : MonoBehaviour
{
    public static SoldierManager instance;
    public GameObject panelUI;
    public int ruanganTerpilih;
    public GameObject prefabTentara;

    public bool sedangCooldown = false;
    public float waktuCooldown = 20f;
    private float timerCooldown = 0f;

    public Button tombolKirim;
    public TextMeshProUGUI teksTombolKirim;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelUI != null) panelUI.SetActive(false);
    }

    void Update()
    {
        if (sedangCooldown)
        {
            timerCooldown -= Time.deltaTime;
            if (teksTombolKirim != null) teksTombolKirim.text = "Tunggu " + Mathf.Ceil(timerCooldown).ToString() + "s";

            if (timerCooldown <= 0)
            {
                sedangCooldown = false;
                if (tombolKirim != null) tombolKirim.interactable = true;
                if (teksTombolKirim != null) teksTombolKirim.text = "Kirim Tentara";
            }
        }
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
        if (sedangCooldown) return; // Cegah spam klik

        panelUI.SetActive(false);

        if (RoomManager.instance != null)
        {
            RoomManager.Room targetRuang = RoomManager.instance.rooms.Find(r => r.roomID == ruanganTerpilih);

            if (targetRuang != null && prefabTentara != null)
            {
                Vector3 posisiBersebelahan = targetRuang.lokasiRuangan.position + new Vector3(0.5f, 0f, 0f);
                GameObject tentaraBaru = Instantiate(prefabTentara, posisiBersebelahan, Quaternion.identity);

                SoldierController sc = tentaraBaru.GetComponent<SoldierController>();
                if (sc != null) sc.myRoomID = ruanganTerpilih;

                sedangCooldown = true;
                timerCooldown = waktuCooldown;
                if (tombolKirim != null) tombolKirim.interactable = false;
            }
        }
    }
}
