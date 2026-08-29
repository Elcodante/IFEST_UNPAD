using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance; // Kunci akses antar script

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
    public GameObject prefabEventInvasi; // Wadah untuk men-spawn invasi baru

    void Awake()
    {
        // Menyimpan diri sendiri sebagai instance utama saat game dimulai
        instance = this;
    }

    void Start()
    {
        Debug.Log("Room Manager aktif! Total ruangan terdaftar: " + rooms.Count);
    }

    // Fungsi otomatis untuk menyebarkan zombie
    public void SebarkanZombieDari(int idAsal)
    {
        // Ruangan yang hancur
        Room ruangAsal = rooms.Find(r => r.roomID == idAsal);
        if (ruangAsal != null) ruangAsal.currentState = RoomState.Hancur;

        //Jumlah room sebelah yang aman
        List<Room> targetAman = new List<Room>();
        foreach (int idTetangga in ruangAsal.neighborIDs)
        {
            Room tetangga = rooms.Find(r => r.roomID == idTetangga);
            if (tetangga != null && tetangga.currentState == RoomState.Aman)
            {
                targetAman.Add(tetangga);
            }
        }

        // Room tetangga yang aman bakal di invasi
        if (targetAman.Count > 0)
        {
            int acak = Random.Range(0, targetAman.Count);
            Room target = targetAman[acak];
            target.currentState = RoomState.Diinvasi; // Ubah status agar tidak diserang ganda

            Debug.Log("Penyebaran: Ruangan " + idAsal + " menular ke Ruangan " + target.roomID);

            // 4. Munculkan mesin invasi baru di ruangan target tersebut
            if (prefabEventInvasi != null)
            {
                GameObject invasiBaru = Instantiate(prefabEventInvasi);
                ZombieController zc = invasiBaru.GetComponent<ZombieController>();
                zc.targetRoomID = target.roomID;
                zc.lokasiSpawn = target.lokasiRuangan;
            }
        }
        else
        {
            Debug.Log("Ruangan " + idAsal + " hancur, tapi semua tetangga sudah terinfeksi/hancur.");
        }
    }
}
