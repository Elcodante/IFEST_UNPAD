using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("PENGATURAN KEMENANGAN & HARI")]
    public float targetWaktuMenang = 180f;
    public string namaSceneBerikutnya = "Day3";

    public int hariSaatIni = 1;

    [Header("UI & TIMER")]
    public TextMeshProUGUI teksHariUI;
    public TextMeshProUGUI teksWaktuUI;
    public GameObject panelWin;

    private float detikBertahan = 0f;

    // PERBAIKAN: Dimatikan dari awal agar menunggu Main Menu
    public bool waktuBerjalan = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        waktuBerjalan = false;
        detikBertahan = 0f;

        if (panelWin != null) panelWin.SetActive(false);

        UpdateUIHari();
        UpdateTeksWaktu(); 
    }

    void Update()
    {
        if (waktuBerjalan)
        {
            detikBertahan += Time.deltaTime;
            UpdateTeksWaktu();

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

    void UpdateUIHari()
    {
        if (teksHariUI != null) teksHariUI.text = "DAY " + hariSaatIni;
    }

    void MenangGame()
    {
        Debug.Log("GAME CLEAR: Waktu bertahan hidup berhasil dicapai!");
        waktuBerjalan = false;

        if (RoomManager.instance != null)
        {
            RoomManager.instance.HentikanInvasi();
        }

        if (panelWin != null) panelWin.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TombolNextDayDitekan()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(namaSceneBerikutnya);
    }

    public void TombolMainMenuDitekan()
    {
        Time.timeScale = 1f;
        Debug.Log("Kembali ke Main Menu");
    }
}