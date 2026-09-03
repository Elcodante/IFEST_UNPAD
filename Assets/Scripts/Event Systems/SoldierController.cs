using UnityEngine;
using System.Collections;

public class SoldierController : MonoBehaviour
{
    public int myRoomID;
    public float waktuSerang = 2f;

    void Start()
    {
        StartCoroutine(ProsesMembasmi());
    }

    IEnumerator ProsesMembasmi()
    {
        yield return new WaitForSeconds(0.1f);
        ZombieController targetZombie = null;
        ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);

        foreach (ZombieController zc in semuaZombie)
        {
            if (zc.targetRoomID == myRoomID)
            {
                targetZombie = zc;
                break;
            }
        }

        if (targetZombie != null)
        {
            Debug.Log("Tentara mulai menembak di Ruangan " + myRoomID);

            while (targetZombie != null && targetZombie.titikSekarang > 0)
            {
                yield return new WaitForSeconds(waktuSerang);

                if (targetZombie != null)
                {
                    targetZombie.KurangiSatuTitik();
                    Debug.Log("DOR! 1 Titik merah hancur.");
                }
            }
        }
        else
        {
            Debug.Log("Ruangan " + myRoomID + " aman, tidak ada zombie. Tentara gabut.");
        }

        Debug.Log("Tugas selesai! Ruangan " + myRoomID + " BERSIH.");
        Destroy(this.gameObject);
    }
}
