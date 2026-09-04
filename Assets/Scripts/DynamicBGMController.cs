using UnityEngine;
using System.Collections;

public class DynamicBGMController : MonoBehaviour
{
    [Header("BGM Database IDs")]
    public string bgmUtamaID = "BGM_Pendek_1";
    public string bgmJarangID = "BGM_Pendek_2";

    [Header("Sistem Probabilitas (Peluang)")]
    [Tooltip("Persentase BGM Utama diputar (Contoh: 80 = 80% BGM Utama, 20% BGM Jarang)")]
    [Range(0, 100)] public float peluangBGMUtama = 80f;

    [Header("Pengaturan Transisi")]
    [Tooltip("Waktu perpindahan antar lagu agar tidak patah")]
    public float waktuCrossfade = 1.5f;

    private Coroutine playlistRoutine;

    private void Start()
    {
        // Mulai mainkan daftar putar saat game dimulai
        playlistRoutine = StartCoroutine(PlaylistRoutine());
    }

    private IEnumerator PlaylistRoutine()
    {
        // Tunggu sebentar agar AudioManager selesai inisialisasi di frame pertama
        yield return new WaitForSeconds(0.5f);

        while (true) // Looping abadi selama game berjalan
        {
            // 1. Lempar dadu dari 0 hingga 100
            float lemparDadu = Random.Range(0f, 100f);

            // 2. Tentukan lagu mana yang menang berdasarkan persentase
            string laguPilihan = (lemparDadu <= peluangBGMUtama) ? bgmUtamaID : bgmJarangID;

            // 3. Putar lagu tersebut
            float durasiLagu = 10f; // Durasi darurat jika audio tidak ditemukan
            if (AudioManager.Instance != null)
            {
                durasiLagu = AudioManager.Instance.GetBGMDuration(laguPilihan);
                AudioManager.Instance.PlayBGM(laguPilihan, waktuCrossfade);
            }

            // 4. Hitung kapan kita harus melempar dadu untuk lagu berikutnya.
            // Kita potong durasi lagu dengan waktu crossfade agar lagu baru masuk TEPAT sebelum lagu lama habis!
            float waktuTunggu = durasiLagu - waktuCrossfade;

            // Keamanan jika lagunya sangat pendek (kurang dari waktu crossfade)
            if (waktuTunggu <= 0.5f) waktuTunggu = durasiLagu;

            // 5. DJ Istirahat dan menunggu sampai lagu ini mau habis
            yield return new WaitForSeconds(waktuTunggu);
        }
    }
}