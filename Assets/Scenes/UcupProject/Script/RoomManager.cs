using System.Collections.Generic;
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

    public GameObject panelGameOver; // Wadah untuk UI Layar Hitam

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Matikan layar Game Over saat game baru mulai
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    // Fungsi untuk memunculkan layar hitam
    public void MunculkanGameOver()
    {
        if (panelGameOver != null) panelGameOver.SetActive(true);
        Time.timeScale = 0f; // Bekukan game
    }

    // Fungsi untuk tombol Restart
    public void TombolRestartDitekan()
    {
        Time.timeScale = 1f; // Cairkan waktu kembali
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fungsi untuk tombol Main Menu (Quit)
    public void TombolQuitDitekan()
    {
        Time.timeScale = 1f;
        Debug.Log("Kembali ke Main Menu!");
        // SceneManager.LoadScene("NamaSceneMenu"); // Temanmu akan mengaktifkan ini nanti
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
}
