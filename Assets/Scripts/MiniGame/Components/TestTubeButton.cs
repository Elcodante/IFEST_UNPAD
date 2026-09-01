using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TestTubeButton : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector] public int tubeID;
    [HideInInspector] public AntidoteSequenceManager manager;

    [Header("Visual Feedback (Sprites)")]
    [Tooltip("Masukkan gambar botol saat mati (contoh: biru meninggoy)")]
    public Sprite idleSprite;

    [Tooltip("Masukkan gambar botol saat menyala (contoh: biru nyala)")]
    public Sprite glowSprite;

    public float flashDuration = 0.4f;

    private Image tubeImage;
    private Coroutine flashRoutine;

    private void Awake()
    {
        tubeImage = GetComponent<Image>();

        // Pastikan botol dimulai dalam keadaan mati
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
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 1. Ganti ke gambar menyala
        if (tubeImage != null && idleSprite != null)
        {
            tubeImage.sprite = idleSprite;
        }

        yield return new WaitForSeconds(0.1f);

        if (tubeImage != null && glowSprite != null)
        {
            tubeImage.sprite = glowSprite;
        }

        yield return new WaitForSeconds(flashDuration);

        if (tubeImage != null && idleSprite != null)
        {
            tubeImage.sprite = idleSprite;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.ReceivePlayerInput(tubeID);
        }
    }
}