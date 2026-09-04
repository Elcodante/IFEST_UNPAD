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

    [Header("PENGATURAN SCENE AWAL")]
    public string namaSceneAwal = "Day 1";

    [Header("PENGATURAN LOSE CONDITION")]
    [Tooltip("Masukkan Room ID untuk ruang Security")]
    public int securityRoomID = 6; // Sesuaikan dengan ID Security di Inspector

    [Header("PENGATURAN SPAM ZOMBIE")]
    public float waktuTungguAwal = 5f;
    public float jedaAntarInvasi = 20f;
    public int batasMaksimalInvasi = 4;

    private bool invasiBerjalan = true;
    private bool isGameOverTriggered = false;

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

    void Update()
    {
        // Memantau kondisi kalah secara *real-time* selama invasi berjalan
        if (invasiBerjalan && !isGameOverTriggered)
        {
            CekKondisiKalah();
        }
    }

    IEnumerator MesinSpamZombie()
    {
        while (DayManager.instance != null && !DayManager.instance.waktuBerjalan)
        {
            yield return null;
        }

        yield return new WaitForSeconds(waktuTungguAwal);

        for (int i = 0; i < batasMaksimalInvasi; i++)
        {
            if (!invasiBerjalan) yield break;
            SpawnZombieDiRuangAcak();

            if (i < batasMaksimalInvasi - 1)
            {
                yield return new WaitForSeconds(jedaAntarInvasi);
            }
        }
    }

    private void CekKondisiKalah()
    {
        // 1. KONDISI KALAH: Jika Zombie masuk ke Ruang Security (ID 6)
        Room ruangSecurity = rooms.Find(r => r.roomID == securityRoomID);
        if (ruangSecurity != null)
        {
            if (ruangSecurity.currentState == RoomState.Diinvasi || ruangSecurity.currentState == RoomState.Hancur)
            {
                Debug.Log("GAME OVER: Ruang Security telah dikuasai zombie!");
                TriggerGameOverCustom();
                return;
            }
        }

        // 2. KONDISI KALAH: Jika Room 2, 3, 4, dan 5 semuanya dikuasai (Diinvasi / Hancur)
        int[] targetRooms = { 2, 3, 4, 5 };
        bool semuaEmpatRuanganKalah = true;

        foreach (int id in targetRooms)
        {
            Room r = rooms.Find(room => room.roomID == id);
            if (r != null)
            {
                // Jika masih ada SATU saja dari ruangan ini yang "Aman", berarti belum kalah
                if (r.currentState == RoomState.Aman)
                {
                    semuaEmpatRuanganKalah = false;
                    break;
                }
            }
        }

        if (semuaEmpatRuanganKalah)
        {
            Debug.Log("GAME OVER: Room 2, 3, 4, dan 5 semuanya telah dikuasai zombie!");
            TriggerGameOverCustom();
            return;
        }
    }

    private void TriggerGameOverCustom()
    {
        if (isGameOverTriggered) return;
        isGameOverTriggered = true;
        MunculkanGameOver();
    }

    public void HentikanInvasi()
    {
        invasiBerjalan = false;
    }

    public void JadikanRuanganAman(int id)
    {
        Room r = rooms.Find(x => x.roomID == id);
        if (r != null) r.currentState = RoomState.Aman;
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
        SceneManager.LoadScene(namaSceneAwal);
    }

    public void SebarkanZombieDari(int idAsal)
    {
        Room ruangAsal = rooms.Find(r => r.roomID == idAsal);
        if (ruangAsal != null) ruangAsal.currentState = RoomState.Hancur;

        List<Room> targetAman = new List<Room>();
        foreach (int idTetangga in ruangAsal.neighborIDs)
        {
            if (PintuController.CekJalurDiblokir(idAsal, idTetangga)) continue;

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
        // Daftar ID ruangan yang boleh di-spawn zombie secara acak (Room 2, 3, 4, dan 5)
        int[] daftarRuangTarget = { 2, 3, 4, 5 };
        List<Room> daftarRuangAman = new List<Room>();

        foreach (int id in daftarRuangTarget)
        {
            Room r = rooms.Find(room => room.roomID == id);
            // Masukkan ke daftar jika ruangannya ditemukan dan statusnya sedang Aman
            if (r != null && r.currentState == RoomState.Aman)
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
                
                Debug.Log($"ZOMBIE MUNCUL DI: Room ID {target.roomID}");
            }
        }
        else
        {
            Debug.Log("Semua ruang target sudah diinvasi atau hancur!");
        }
    }
}