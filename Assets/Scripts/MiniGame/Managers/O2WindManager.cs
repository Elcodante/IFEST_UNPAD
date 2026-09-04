using System.Collections;
using UnityEngine;

public class O2WindManager : MinigameDragManager
{
    [Header("Wind Settings")]
    public bool isWindBlowing = false;
    public float minCalmTime = 2f;
    public float maxCalmTime = 4f;
    public float minWindTime = 1f;
    public float maxWindTime = 1f;

    [Header("Warning settings")]
    public float warningDuration = 1f;
    public float blinkInterval = 0.2f;

    [Header("Wind Visuals")]
    public GameObject windWarningUI;
    [Tooltip("Masukkan objek partikel angin atau gambar animasi badai di sini")]
    public GameObject windParticlesObject;

    // --- TAMBAHAN AUDIO ---
    [Header("Audio Settings")]
    public string warningSoundID = "SFX_O2_Warning"; // Suara Beep/Alarm
    public string windSoundID = "SFX_O2_Angin";      // Suara Badai/Tornado
    // ----------------------

    public static O2WindManager Instance;
    private Coroutine windRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetWindState();

        if (windRoutine != null) StopCoroutine(windRoutine);
        windRoutine = StartCoroutine(WindCycleRoutine());
    }

    private void OnDisable()
    {
        if (windRoutine != null)
        {
            StopCoroutine(windRoutine);
            windRoutine = null;
        }
        ResetWindState();
    }

    private void ResetWindState()
    {
        isWindBlowing = false;
        if (windWarningUI != null) windWarningUI.SetActive(false);
        if (windParticlesObject != null) windParticlesObject.SetActive(false);

        // AUDIO: Matikan suara angin (looping) jika angin sedang berhenti
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoopingSFX();
        }
    }

    private IEnumerator WindCycleRoutine()
    {
        while (true)
        {
            ResetWindState();

            float calmDuration = Random.Range(minCalmTime, maxCalmTime);
            yield return new WaitForSeconds(calmDuration);

            // Fase Peringatan (Kedip-kedip)
            if (windWarningUI != null)
            {
                float blinkTimer = 0f;
                while (blinkTimer < warningDuration)
                {
                    // Cek apakah UI akan menyala atau mati
                    bool isTurningOn = !windWarningUI.activeSelf;
                    windWarningUI.SetActive(isTurningOn);

                    // JUICE AUDIO: Bunyikan nada Beep HANYA saat ikon peringatannya menyala
                    if (isTurningOn && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(warningSoundID);
                    }

                    yield return new WaitForSeconds(blinkInterval);
                    blinkTimer += blinkInterval;
                }
            }

            // Fase Badai (Angin Bertiup)
            isWindBlowing = true;
            if (windWarningUI != null) windWarningUI.SetActive(true);
            if (windParticlesObject != null) windParticlesObject.SetActive(true);

            // JUICE AUDIO: Nyalakan suara deru angin (Looping)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLoopingSFX(windSoundID);
            }

            float windDuration = Random.Range(minWindTime, maxWindTime);
            yield return new WaitForSeconds(windDuration);
        }
    }
}