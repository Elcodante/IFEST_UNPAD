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
    public Sprite gambarTerbuka;        // Sprite Hijau (Normal / Jebol)
    public Sprite gambarTertutup;       // Sprite Merah (Otomatis Karantina)

    private SpriteRenderer sr;
    public bool sedangDitutup = false;  // Dibaca oleh RoomManager untuk blokir jalur

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
        // Jalankan pengecekan otomatis setiap frame tanpa perlu diklik pemain
        UpdateStatusPintu();
    }

    void UpdateStatusPintu()
    {
        if (sr == null) return;

        // Cari apakah ada zombie di Ruang A atau Ruang B
        ZombieController zombieA = CariZombieDiRuang(ruangA);
        ZombieController zombieB = CariZombieDiRuang(ruangB);

        bool adaAncamanA = (zombieA != null && zombieA.titikSekarang > 0 && zombieA.titikSekarang < 5);
        bool adaAncamanB = (zombieB != null && zombieB.titikSekarang > 0 && zombieB.titikSekarang < 5);

        bool sudahBobolA = (zombieA != null && zombieA.titikSekarang >= 5);
        bool sudahBobolB = (zombieB != null && zombieB.titikSekarang >= 5);

        // LOGIKA OTOMATIS:
        // 1. Jika ruangan sudah mencapai 5 titik (bobol/hancur), pintu jadi HIJAU dan TERBUKA (jalur terbuka kembali)
        if (sudahBobolA || sudahBobolB)
        {
            sedangDitutup = false;
            sr.sprite = gambarTerbuka;
        }
        // 2. Jika ada zombie (1-4 titik), pintu otomatis MERAH dan DITUTUP (mengunci/karantina otomatis)
        else if (adaAncamanA || adaAncamanB)
        {
            sedangDitutup = true;
            sr.sprite = gambarTertutup;
        }
        // 3. Jika kedua ruangan aman, pintu HIJAU dan TERBUKA
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

    // Fungsi otomatis yang ditanya oleh RoomManager untuk menahan penyebaran
    public static bool CekJalurDiblokir(int asal, int tujuan)
    {
        foreach (PintuController pintu in semuaPintu)
        {
            if (pintu.sedangDitutup)
            {
                if ((pintu.ruangA == asal && pintu.ruangB == tujuan) ||
                    (pintu.ruangA == tujuan && pintu.ruangB == asal))
                {
                    return true; // Jalur terkunci merah, virus gagal menyebar!
                }
            }
        }
        return false;
    }
}