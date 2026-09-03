using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PintuController : MonoBehaviour
{
    public static List<PintuController> semuaPintu = new List<PintuController>();

    [Header("IDENTITAS JALUR")]
    public int ruangA;
    public int ruangB;

    [Header("PENGATURAN VISUAL PINTU")]
    public Sprite gambarTerbuka;
    public Sprite gambarTertutup;

    private SpriteRenderer sr;
    public bool sedangDitutup = false;

    void Awake()
    {
        semuaPintu.Add(this);
    }

    void OnDestroy()
    {
        semuaPintu.Remove(this);
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateStatusPintu();
    }

    void Update()
    {
        UpdateStatusPintu();
    }

    void UpdateStatusPintu()
    {
        if (sr == null) return;

        // FITUR BARU: Abaikan pencarian jika kamu mengisi angka 0
        ZombieController zombieA = (ruangA != 0) ? CariZombieDiRuang(ruangA) : null;
        ZombieController zombieB = (ruangB != 0) ? CariZombieDiRuang(ruangB) : null;

        // PERBAIKAN: Pintu baru bereaksi JIKA titikSekarang > 0 (titik merah sudah muncul)
        bool adaAncamanA = (zombieA != null && zombieA.titikSekarang > 0 && zombieA.titikSekarang < 5);
        bool adaAncamanB = (zombieB != null && zombieB.titikSekarang > 0 && zombieB.titikSekarang < 5);

        bool sudahBobolA = (zombieA != null && zombieA.titikSekarang >= 5);
        bool sudahBobolB = (zombieB != null && zombieB.titikSekarang >= 5);

        if (sudahBobolA || sudahBobolB)
        {
            sedangDitutup = false;
            sr.sprite = gambarTerbuka;
        }
        else if (adaAncamanA || adaAncamanB)
        {
            sedangDitutup = true;
            sr.sprite = gambarTertutup;
        }
        else
        {
            sedangDitutup = false;
            sr.sprite = gambarTerbuka;
        }
    }

    ZombieController CariZombieDiRuang(int idRuang)
    {
        ZombieController[] semuaZombie = Object.FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
        foreach (var zc in semuaZombie)
        {
            if (zc.targetRoomID == idRuang)
            {
                return zc;
            }
        }
        return null;
    }

    public static bool CekJalurDiblokir(int asal, int tujuan)
    {
        foreach (PintuController pintu in semuaPintu)
        {
            if (pintu.sedangDitutup)
            {
                // Sistem blokir tetap berfungsi normal meskipun salah satu ujungnya 0
                if ((pintu.ruangA == asal && pintu.ruangB == tujuan) ||
                    (pintu.ruangA == tujuan && pintu.ruangB == asal) ||
                    (pintu.ruangA == asal && pintu.ruangB == 0) ||
                    (pintu.ruangA == 0 && pintu.ruangB == asal))
                {
                    return true;
                }
            }
        }
        return false;
    }
}