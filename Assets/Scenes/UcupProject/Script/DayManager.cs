using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk pindah scene
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("PENGATURAN KEMENANGAN")]
    public float targetWaktuMenang = 180f; // Misal menang dalam 180 detik (3 menit)
    public string namaSceneBerikutnya = "Day2"; // Nama scene selanjutnya untuk tombol Next Day

    [Header("UI & TIMER")]
    public TextMeshProUGUI teksHariUI;
    public TextMeshProUGUI teksWaktuUI;
    public GameObject panelWin; // Wadah untuk UI Panel Menang

    private float detikBertahan = 0f;
    public bool waktuBerjalan = true;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Pastikan panel Win tertutup di awal
        if (panelWin != null) panelWin.SetActive(false);
    }

    void Update()
    {
        if (waktuBerjalan)
        {
            detikBertahan += Time.deltaTime;
            UpdateTeksWaktu();

            // CEK KONDISI MENANG (Jika waktu bertahan sudah mencapai target)
            if (detikBertahan >= targetWaktuMenang)
            {
                MenangGame();
            }
        }
    }

    void UpdateTeksWaktu()
    {
        if (teksWaktuUI != null)
        {
            int menit = Mathf.FloorToInt(detikBertahan / 60);
            int detik = Mathf.FloorToInt(detikBertahan % 60);
            teksWaktuUI.text = string.Format("{0:00}:{1:00}", menit, detik);
        }
    }

    void MenangGame()
    {
        Debug.Log("GAME CLEAR: Waktu bertahan hidup berhasil dicapai!");
        waktuBerjalan = false;

        // Suruh RoomManager matikan mesin pabrik zombienya
        if (RoomManager.instance != null)
        {
            RoomManager.instance.HentikanInvasi();
        }

        // Tampilkan Panel Selamat dan bekukan dunia game
        if (panelWin != null) panelWin.SetActive(true);
        Time.timeScale = 0f;
    }

    // FUNGSI UNTUK TOMBOL "NEXT DAY"
    public void TombolNextDayDitekan()
    {
        Time.timeScale = 1f; // Cairkan waktu game sebelum pindah scene
        SceneManager.LoadScene(namaSceneBerikutnya);
    }

    // FUNGSI UNTUK TOMBOL "MAIN MENU"
    public void TombolMainMenuDitekan()
    {
        Time.timeScale = 1f;
        Debug.Log("Kembali ke Main Menu");
        // SceneManager.LoadScene("NamaSceneMainMenu");
    }
}