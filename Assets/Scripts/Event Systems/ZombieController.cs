using System.Collections;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    public int targetRoomID = 1;
    public int titikSekarang = 0;
    public int maksimalTitik = 5;

    [Header("PENGATURAN WAKTU")]
    public float jedaAwal = 0f;
    public float waktuTumbuh = 10f;

    [Header("REFERENSI")]
    public GameObject prefabTitikMerah;
    public Transform lokasiSpawn;

    void Start()
    {
        StartCoroutine(ProsesTumbuh());
    }

    IEnumerator ProsesTumbuh()
    {
        if (jedaAwal > 0f)
        {
            yield return new WaitForSeconds(jedaAwal);
        }

        while (titikSekarang < maksimalTitik)
        {
            if (prefabTitikMerah != null && lokasiSpawn != null)
            {
                GameObject titikBaru = Instantiate(prefabTitikMerah, lokasiSpawn);

                Vector3 posisiTitik = Vector3.zero;
                float jarak = 25f;

                switch (titikSekarang)
                {
                    case 0: posisiTitik = new Vector3(0, 0, 0); break;           // Titik 1: Tengah
                    case 1: posisiTitik = new Vector3(-jarak, jarak, 2); break;  // Titik 2: Kiri Atas
                    case 2: posisiTitik = new Vector3(jarak, jarak, 2); break;   // Titik 3: Kanan Atas
                    case 3: posisiTitik = new Vector3(-jarak, -jarak, 2); break; // Titik 4: Kiri Bawah
                    case 4: posisiTitik = new Vector3(jarak, -jarak, 2); break;  // Titik 5: Kanan Bawah
                }

                titikBaru.transform.localPosition = posisiTitik;
                titikSekarang++;
            }

            if (titikSekarang < maksimalTitik)
            {
                yield return new WaitForSeconds(waktuTumbuh);
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (RoomManager.instance != null)
        {
            RoomManager.instance.SebarkanZombieDari(targetRoomID);
        }
    }

    public void KurangiSatuTitik()
    {
        if (titikSekarang > 0)
        {
            titikSekarang--;

            if (lokasiSpawn != null && lokasiSpawn.childCount > 0)
            {
                Destroy(lokasiSpawn.GetChild(lokasiSpawn.childCount - 1).gameObject);
            }

            if (titikSekarang <= 0)
            {
                if (RoomManager.instance != null)
                {
                    RoomManager.instance.JadikanRuanganAman(targetRoomID);
                }
                Destroy(gameObject);
            }
        }
    }
}


//using System.Collections;
//using UnityEngine;

//public class ZombieController : MonoBehaviour
//{
//    public int targetRoomID = 1;
//    public int titikSekarang = 0;
//    public int maksimalTitik = 5;
//    public float waktuTumbuh = 10f;
//    public GameObject prefabTitikMerah;
//    public Transform lokasiSpawn;

//    void Start()
//    {
//        StartCoroutine(ProsesTumbuh());
//    }

//    IEnumerator ProsesTumbuh()
//    {
//        while (titikSekarang < maksimalTitik)
//        {
//            if (prefabTitikMerah != null && lokasiSpawn != null)
//            {
//                GameObject titikBaru = Instantiate(prefabTitikMerah, lokasiSpawn);

//                // MENGATUR POSISI TITIK (POLA DADU 5)
//                Vector3 posisiTitik = Vector3.zero;

//                // Angka jarak rentangan titik (Silakan ubah misal 0.3f atau 0.5f jika kurang pas)
//                float jarak = 0.3f;

//                switch (titikSekarang)
//                {
//                    case 0: posisiTitik = new Vector3(0, 0, 0); break;           
//                    case 1: posisiTitik = new Vector3(-jarak, jarak, 0); break;  
//                    case 2: posisiTitik = new Vector3(jarak, jarak, 0); break;  
//                    case 3: posisiTitik = new Vector3(-jarak, -jarak, 0); break; 
//                    case 4: posisiTitik = new Vector3(jarak, -jarak, 0); break;  
//                }

//                titikBaru.transform.localPosition = posisiTitik;
//                titikSekarang++;
//            }

//            if (titikSekarang < maksimalTitik)
//            {
//                yield return new WaitForSeconds(waktuTumbuh);
//            }
//        }

//        if (RoomManager.instance != null)
//        {
//            RoomManager.instance.SebarkanZombieDari(targetRoomID);
//        }
//    }

//    public void KurangiSatuTitik()
//    {
//        if (titikSekarang > 0)
//        {
//            titikSekarang--;

//            if (lokasiSpawn != null && lokasiSpawn.childCount > 0)
//            {
//                Destroy(lokasiSpawn.GetChild(lokasiSpawn.childCount - 1).gameObject);
//            }

//            if (titikSekarang <= 0)
//            {
//                if (RoomManager.instance != null)
//                {
//                    RoomManager.instance.JadikanRuanganAman(targetRoomID);
//                }
//                Destroy(gameObject);
//            }
//        }
//    }
//}