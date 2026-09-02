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
    public GameObject windParticlesObject; // --- TAMBAHAN ---

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
                    windWarningUI.SetActive(!windWarningUI.activeSelf);
                    yield return new WaitForSeconds(blinkInterval);
                    blinkTimer += blinkInterval;
                }
            }

            // Fase Badai (Angin Bertiup)
            isWindBlowing = true;
            if (windWarningUI != null) windWarningUI.SetActive(true);

            // JUICE: Nyalakan efek partikel angin kencang melintasi layar
            if (windParticlesObject != null) windParticlesObject.SetActive(true);

            float windDuration = Random.Range(minWindTime, maxWindTime);
            yield return new WaitForSeconds(windDuration);
        }
    }
}