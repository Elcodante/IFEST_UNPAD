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

    // --- TAMBAHAN AUDIO ---
    [Header("Audio Settings")]
    public string tapSoundID = "SFX_Antidote_Klik";
    // ----------------------

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
            transform.localScale = originalScale;
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (tubeImage != null && idleSprite != null) tubeImage.sprite = idleSprite;
        yield return new WaitForSeconds(0.05f);

        if (tubeImage != null && glowSprite != null) tubeImage.sprite = glowSprite;

        float animTime = 0;
        while (animTime < flashDuration)
        {
            animTime += Time.deltaTime;
            float t = animTime / flashDuration;

            float scaleMultiplier = Mathf.Lerp(1.15f, 1.0f, t);
            transform.localScale = new Vector3(originalScale.x * scaleMultiplier, originalScale.y * scaleMultiplier, 1f);

            yield return null;
        }

        transform.localScale = originalScale;

        if (tubeImage != null && idleSprite != null) tubeImage.sprite = idleSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * 0.9f;

        // JUICE AUDIO 4: Suara ketukan jari pemain pada botol.
        // Kita juga samakan nadanya dengan nada mesin agar pemain merasa sedang bermain musik!
        if (AudioManager.Instance != null)
        {
            float nadaBotol = 0.8f + (tubeID * 0.15f);
            AudioManager.Instance.PlaySFXRandomPitch(tapSoundID, nadaBotol, nadaBotol);
        }

        if (manager != null)
        {
            manager.ReceivePlayerInput(tubeID);
        }
    }
}