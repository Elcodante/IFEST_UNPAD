using UnityEngine;
using System.Collections;

public class UIShaker : MonoBehaviour
{
    public static UIShaker Instance;
    private RectTransform rectTransform;
    private Vector3 originalPos;

    // Ganti Awake menjadi OnEnable agar selalu di-refresh setiap panel dinyalakan
    private void OnEnable()
    {
        Instance = this;

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Simpan posisi saat ini sebagai posisi asli setiap kali panel dibuka
        originalPos = rectTransform.anchoredPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        Debug.Log("[UIShaker] Menerima sinyal KORSLETING! Memulai getaran layar...");
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            rectTransform.anchoredPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Kembalikan ke posisi semula
        rectTransform.anchoredPosition = originalPos;
        Debug.Log("[UIShaker] Getaran selesai, posisi dikembalikan.");
    }
}