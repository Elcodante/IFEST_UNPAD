using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    public enum RoomState { Aman, Diinvasi, Hancur }

    [System.Serializable]
    public class Room
    {
        public int roomID;
        public RoomState currentState = RoomState.Aman;
        public List<int> neighborIDs;
        public Transform lokasiRuangan;
    }

    public List<Room> rooms = new List<Room>();
    public GameObject prefabEventInvasi;
    public GameObject panelGameOver;

    [Header("PENGATURAN SPAM ZOMBIE")]
    public float waktuTungguAwal = 5f;
    public float jedaAntarInvasi = 20f;

    [Header("BATAS INVASI SCENE INI")]
    // Tambahan baru: Batas maksimal zombie yang keluar di hari ini
    public int batasMaksimalInvasi = 4;

    private bool invasiBerjalan = true;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelGameOver != null) panelGameOver.SetActive(false);

        Room ruangAwal = rooms.Find(r => r.roomID == 1);
        if (ruangAwal != null) ruangAwal.currentState = RoomState.Diinvasi;

        StartCoroutine(MesinSpamZombie());
    }

    IEnumerator MesinSpamZombie()
    {
        Debug.Log("WAVE SYSTEM: Menunggu persiapan awal...");
        yield return new WaitForSeconds(waktuTungguAwal);

        // Hanya akan melempar zombie sebanyak angka 'batasMaksimalInvasi'
        for (int i = 0; i < batasMaksimalInvasi; i++)
        {
            // Cek apakah game belum game over/menang
            if (!invasiBerjalan) yield break;

            SpawnZombieDiRuangAcak();

            // Tunggu jeda sebelum melempar zombie berikutnya (jika belum yang terakhir)
            if (i < batasMaksimalInvasi - 1)
            {
                yield return new WaitForSeconds(jedaAntarInvasi);
            }
        }

        Debug.Log("WAVE SYSTEM: Seluruh " + batasMaksimalInvasi + " invasi untuk hari ini telah selesai dijatuhkan!");
    }

    public void HentikanInvasi()
    {
        invasiBerjalan = false;
    }

    public void JadikanRuanganAman(int id)
    {
        Room r = rooms.Find(x => x.roomID == id);
        if (r != null)
        {
            r.currentState = RoomState.Aman;
            Debug.Log("Sistem: Ruangan " + id + " sekarang kembali AMAN.");
        }
    }

    public void MunculkanGameOver()
    {
        HentikanInvasi();
        if (DayManager.instance != null) DayManager.instance.waktuBerjalan = false;

        if (panelGameOver != null) panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TombolRestartDitekan()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SebarkanZombieDari(int idAsal)
    {
        Room ruangAsal = rooms.Find(r => r.roomID == idAsal);
        if (ruangAsal != null) ruangAsal.currentState = RoomState.Hancur;

        List<Room> targetAman = new List<Room>();
        foreach (int idTetangga in ruangAsal.neighborIDs)
        {
            Room tetangga = rooms.Find(r => r.roomID == idTetangga);
            if (tetangga != null && tetangga.currentState == RoomState.Aman)
            {
                targetAman.Add(tetangga);
            }
        }

        if (targetAman.Count > 0)
        {
            int acak = Random.Range(0, targetAman.Count);
            Room target = targetAman[acak];
            target.currentState = RoomState.Diinvasi;

            if (prefabEventInvasi != null)
            {
                GameObject invasiBaru = Instantiate(prefabEventInvasi);
                ZombieController zc = invasiBaru.GetComponent<ZombieController>();
                zc.targetRoomID = target.roomID;
                zc.lokasiSpawn = target.lokasiRuangan;
            }
        }
    }

    public void SpawnZombieDiRuangAcak()
    {
        List<Room> daftarRuangAman = new List<Room>();
        foreach (Room r in rooms)
        {
            if (r.currentState == RoomState.Aman && r.roomID != 6)
            {
                daftarRuangAman.Add(r);
            }
        }

        if (daftarRuangAman.Count > 0)
        {
            int indexAcak = Random.Range(0, daftarRuangAman.Count);
            Room target = daftarRuangAman[indexAcak];

            target.currentState = RoomState.Diinvasi;

            if (prefabEventInvasi != null)
            {
                GameObject invasiBaru = Instantiate(prefabEventInvasi);
                ZombieController zc = invasiBaru.GetComponent<ZombieController>();
                zc.targetRoomID = target.roomID;
                zc.lokasiSpawn = target.lokasiRuangan;

                Debug.Log("WAVE SYSTEM: Muncul invasi baru secara acak di Ruangan " + target.roomID);
            }
        }
    }
}