using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        titikSekarang = 0;
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
                if (RoomManager.instance != null) RoomManager.instance.MunculkanGameOver();
                yield break;
            }

            if (titikSekarang >= maksimalTitik)
            {
                Debug.Log("Ruangan " + targetRoomID + " HANCUR (100%)! Pintu jebol.");
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

            int jumlahAnak = transform.childCount;
            if (jumlahAnak > 0)
            {
                Destroy(transform.GetChild(jumlahAnak - 1).gameObject);
            }

            if (titikSekarang <= 0)
            {
                // Memanggil fungsi baru di RoomManager agar ruangan kembali Aman
                if (RoomManager.instance != null)
                {
                    RoomManager.instance.JadikanRuanganAman(targetRoomID);
                }

                Debug.Log("Invasi di Ruangan " + targetRoomID + " berhasil dihentikan!");
                Destroy(this.gameObject);
            }
        }
    }
}
