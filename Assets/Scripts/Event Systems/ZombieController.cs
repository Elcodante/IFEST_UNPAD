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
        // 1. Tahan Jeda Awal jika game dipause (sedang buka minigame)
        if (jedaAwal > 0f)
        {
            float timerAwal = 0f;
            while (timerAwal < jedaAwal)
            {
                if (DayManager.instance != null && DayManager.instance.waktuBerjalan) timerAwal += Time.deltaTime;
                yield return null;
            }
        }

        while (titikSekarang < maksimalTitik)
        {
            // Tahan proses kalau player sedang berada di dalam minigame
            while (DayManager.instance != null && !DayManager.instance.waktuBerjalan) yield return null;

            if (prefabTitikMerah != null && lokasiSpawn != null)
            {
                GameObject titikBaru = Instantiate(prefabTitikMerah, lokasiSpawn);
                Vector3 posisiTitik = Vector3.zero;
                float jarak = 25f;

                switch (titikSekarang)
                {
                    case 0: posisiTitik = new Vector3(0, 0, 0); break;
                    case 1: posisiTitik = new Vector3(-jarak, jarak, 2); break;
                    case 2: posisiTitik = new Vector3(jarak, jarak, 2); break;
                    case 3: posisiTitik = new Vector3(-jarak, -jarak, 2); break;
                    case 4: posisiTitik = new Vector3(jarak, -jarak, 2); break;
                }

                titikBaru.transform.localPosition = posisiTitik;
                titikSekarang++;
            }

            if (titikSekarang < maksimalTitik)
            {
                // 2. Timer Manual Waktu Tumbuh agar ikut berhenti saat minigame aktif
                float timerTumbuh = 0f;
                while (timerTumbuh < waktuTumbuh)
                {
                    if (DayManager.instance != null && DayManager.instance.waktuBerjalan) timerTumbuh += Time.deltaTime;
                    yield return null;
                }
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
                if (RoomManager.instance != null) RoomManager.instance.JadikanRuanganAman(targetRoomID);
                Destroy(gameObject);
            }
        }
    }
}