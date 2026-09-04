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
    public int securityRoomID = 6;

    [Header("PENGATURAN SPAM ZOMBIE")]
    public float waktuTungguAwal = 5f;
    public float jedaAntarInvasi = 20f;
    public int batasMaksimalInvasi = 4;

    [Header("AUDIO (SFX)")]
    [Tooltip("Ketik nama file SFX saat Zombie Muncul")]
    public string attackSfxID = "Warning";
    [Tooltip("Ketik nama file SFX saat Kalah/Game Over")]
    public string gameOverSfxID = "Lose";

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
        Room ruangSecurity = rooms.Find(r => r.roomID == securityRoomID);
        if (ruangSecurity != null)
        {
            if (ruangSecurity.currentState == RoomState.Diinvasi || ruangSecurity.currentState == RoomState.Hancur)
            {
                TriggerGameOverCustom();
                return;
            }
        }

        int[] targetRooms = { 2, 3, 4, 5 };
        bool semuaEmpatRuanganKalah = true;

        foreach (int id in targetRooms)
        {
            Room r = rooms.Find(room => room.roomID == id);
            if (r != null)
            {
                if (r.currentState == RoomState.Aman)
                {
                    semuaEmpatRuanganKalah = false;
                    break;
                }
            }
        }

        if (semuaEmpatRuanganKalah)
        {
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

        // --- PENEMPATAN KODE AUDIO GAME OVER ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(gameOverSfxID);
        }
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

                // --- PENEMPATAN KODE AUDIO ZOMBIE MUNCUL ---
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(attackSfxID);
                }
            }
        }
    }

    public void SpawnZombieDiRuangAcak()
    {
        int[] daftarRuangTarget = { 2, 3, 4, 5 };
        List<Room> daftarRuangAman = new List<Room>();

        foreach (int id in daftarRuangTarget)
        {
            Room r = rooms.Find(room => room.roomID == id);
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

                // --- PENEMPATAN KODE AUDIO ZOMBIE MUNCUL ---
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(attackSfxID);
                }
            }
        }
    }
}