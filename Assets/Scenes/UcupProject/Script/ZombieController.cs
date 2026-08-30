using UnityEngine;
using System.Collections;

public class ZombieController : MonoBehaviour
{
    public int targetRoomID;
    public int titikSekarang = 0;
    public int maksimalTitik = 5;
    public float waktuTumbuh = 10f;

    public GameObject prefabTitikMerah;
    public Transform lokasiSpawn;

    private Vector3[] offsetPosisi = new Vector3[] {
        new Vector3(0, 0, 0),
        new Vector3(0.3f, 0.3f, 0),
        new Vector3(-0.3f, 0.3f, 0),
        new Vector3(0.3f, -0.3f, 0),
        new Vector3(-0.3f, -0.3f, 0)
    };

    void Start()
    {
        titikSekarang = 0; // Kunci mutlak agar selalu mulai dari 0
        StartCoroutine(TumbuhTerus());
    }

    IEnumerator TumbuhTerus()
    {
        while (titikSekarang < maksimalTitik)
        {
            MunculkanSatuTitik();
            titikSekarang++;

            if (targetRoomID == 6 && titikSekarang >= 1)
            {
                Debug.Log("GAME OVER! Zombie menerobos masuk ke Security Room!");

                // BARIS INI YANG AKAN MENYALAKAN PANEL UI-NYA:
                if (RoomManager.instance != null) RoomManager.instance.MunculkanGameOver();

                yield break; // Hentikan mesin zombie
            }

            if (titikSekarang >= maksimalTitik)
            {
                Debug.Log("Ruangan " + targetRoomID + " HANCUR (100%)! Pintu jebol.");

                // Memicu reaksi berantai ke RoomManager
                if (RoomManager.instance != null)
                {
                    RoomManager.instance.SebarkanZombieDari(targetRoomID);
                }

                break;
            }

            yield return new WaitForSeconds(waktuTumbuh);
        }
    }

    void MunculkanSatuTitik()
    {
        if (prefabTitikMerah != null && lokasiSpawn != null)
        {
            Vector3 posisiFix = lokasiSpawn.position + offsetPosisi[titikSekarang];
            Instantiate(prefabTitikMerah, posisiFix, Quaternion.identity, this.transform);
        }
    }

    public void KurangiSatuTitik()
    {
        if (titikSekarang > 0)
        {
            titikSekarang--;

            // Hapus 1 visual titik merah (anak objek paling akhir)
            int jumlahAnak = transform.childCount;
            if (jumlahAnak > 0)
            {
                Destroy(transform.GetChild(jumlahAnak - 1).gameObject);
            }

            // Jika titik merah habis, ruangan kembali Aman
            if (titikSekarang <= 0)
            {
                if (RoomManager.instance != null)
                {
                    RoomManager.Room ruang = RoomManager.instance.rooms.Find(r => r.roomID == targetRoomID);
                    if (ruang != null) ruang.currentState = RoomManager.RoomState.Aman;
                }
                Debug.Log("Invasi di Ruangan " + targetRoomID + " berhasil dihentikan!");
                Destroy(this.gameObject); // Hancurkan mesin invasi (hapus dari layar)
            }
        }
    }
}
