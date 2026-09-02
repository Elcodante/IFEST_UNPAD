using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI TUTORIAL")]
    public GameObject panelTutorial;
    public TextMeshProUGUI teksDialog;

    [TextArea]
    public string pesanTutorial = "Komandan! Ada pergerakan zombie di Ruangan 1! Segera kirim tentara untuk membersihkannya sebelum menyebar!";
    public float kecepatanKetik = 0.5f;

    public bool sedangTutorial = false;
    private bool tutorialPernahMuncul = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelTutorial != null) panelTutorial.SetActive(false);
    }

    // FUNGSI BARU: Dipanggil oleh zombie dengan menyertakan waktu jeda
    public void MulaiTutorialDenganJeda(float waktuJeda)
    {
        if (tutorialPernahMuncul) return;
        tutorialPernahMuncul = true;

        StartCoroutine(ProsesJedaTutorial(waktuJeda));
    }

    IEnumerator ProsesJedaTutorial(float jeda)
    {
        // Tunggu beberapa saat agar pemain melihat titik merahnya dulu
        yield return new WaitForSeconds(jeda);

        sedangTutorial = true; // Nyalakan lampu merah (zombie lain berhenti)
        if (panelTutorial != null) panelTutorial.SetActive(true);
        StartCoroutine(EfekMengetik());
    }

    IEnumerator EfekMengetik()
    {
        teksDialog.text = "";
        foreach (char huruf in pesanTutorial.ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
    }

    public void TombolOkDitekan()
    {
        TutupTutorial();
    }

    public void TombolSkipDitekan()
    {
        TutupTutorial();
    }

    void TutupTutorial()
    {
        StopAllCoroutines();
        sedangTutorial = false;
        if (panelTutorial != null) panelTutorial.SetActive(false);
    }
}