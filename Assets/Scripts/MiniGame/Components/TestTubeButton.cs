using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TestTubeButton : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector] public int tubeID;
    [HideInInspector] public AntidoteSequenceManager manager;

    [Header("Visual Feedback (Sprites)")]
    public Sprite idleSprite;
    public Sprite glowSprite;

    public float flashDuration = 0.4f;

    private Image tubeImage;
    private Coroutine flashRoutine;
    private Vector3 originalScale;

    private void Awake()
    {
        tubeImage = GetComponent<Image>();
        originalScale = transform.localScale;

        if (tubeImage != null && idleSprite != null)
        {
            tubeImage.sprite = idleSprite;
        }
    }

    public void FlashTube()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            transform.localScale = originalScale; // Reset skala jika ditekan cepat
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 1. Matikan sesaat untuk memberi ketegangan ketukan (Beat)
        if (tubeImage != null && idleSprite != null) tubeImage.sprite = idleSprite;
        yield return new WaitForSeconds(0.05f);

        // 2. Nyalakan Sprite
        if (tubeImage != null && glowSprite != null) tubeImage.sprite = glowSprite;

        // 3. JUICE: Animasi membal (Squish & Scale) selama menyala
        float animTime = 0;
        while (animTime < flashDuration)
        {
            animTime += Time.deltaTime;
            float t = animTime / flashDuration;

            // Membesar ke 1.15x lalu kembali ke ukuran asli (1.0x) dengan sangat mulus
            float scaleMultiplier = Mathf.Lerp(1.15f, 1.0f, t);
            transform.localScale = new Vector3(originalScale.x * scaleMultiplier, originalScale.y * scaleMultiplier, 1f);

            yield return null;
        }

        transform.localScale = originalScale;

        // 4. Matikan kembali ke kondisi normal
        if (tubeImage != null && idleSprite != null) tubeImage.sprite = idleSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Beri getaran mikro ringan pada tabung saat disentuh pemain (Opsional tapi memuaskan)
        transform.localScale = originalScale * 0.9f;

        if (manager != null)
        {
            manager.ReceivePlayerInput(tubeID);
        }
    }
}