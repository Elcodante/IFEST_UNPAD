using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class GearJamHazard : MonoBehaviour, IPointerDownHandler
{
    public DropZone parentDropZone;
    public float timeToClear = 2.0f;

    private Coroutine jamRoutine;
    private Image hazardImage;
    private Vector3 originalScale;

    private void Awake()
    {
        hazardImage = GetComponent<Image>();
        originalScale = transform.localScale;
    }

    public void ActivateHazard()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        // Kembalikan ke warna dan skala normal setiap kali muncul
        if (hazardImage != null) hazardImage.color = Color.white;
        transform.localScale = originalScale;

        if (jamRoutine != null) StopCoroutine(jamRoutine);
        jamRoutine = StartCoroutine(JamTimer());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (parentDropZone != null && parentDropZone.minigameManager is GeneratorGearManager gearManager)
        {
            gearManager.HazardCleared();
        }

        ClearHazard();
    }

    private void ClearHazard()
    {
        if (jamRoutine != null)
        {
            StopCoroutine(jamRoutine);

            // Pastikan skala dan warna kembali normal saat dimatikan
            transform.localScale = originalScale;
            if (hazardImage != null) hazardImage.color = Color.white;

            gameObject.SetActive(false);
        }
    }

    private IEnumerator JamTimer()
    {
        float elapsed = 0f;

        while (elapsed < timeToClear)
        {
            elapsed += Time.deltaTime;
            float timeRatio = elapsed / timeToClear; // Bernilai 0 di awal, 1 di akhir

            // JUICE 2: ANIMASI PANIK
            // Getaran kerikil akan makin liar dan warnanya memerah mendekati detik-detik terakhir
            float shakeForce = Mathf.Lerp(0f, 0.2f, timeRatio);
            float randomX = Random.Range(-shakeForce, shakeForce);
            float randomY = Random.Range(-shakeForce, shakeForce);

            transform.localScale = originalScale + new Vector3(randomX, randomY, 0);

            if (hazardImage != null)
            {
                hazardImage.color = Color.Lerp(Color.white, Color.red, timeRatio);
            }

            yield return null;
        }

        // Waktu habis!
        if (parentDropZone != null)
        {
            parentDropZone.EjectItem();
        }

        ClearHazard();
    }
}