using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
public class TestTubeButton : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector] public int tubeID;
    [HideInInspector] public AntidoteSequenceManager manager;

    [Header("Visual Feedback")]
    public Color glowColor = Color.white;
    public float flashDuration = 0.4f;

    private Image tubeImage;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        tubeImage = GetComponent<Image>();
        originalColor = tubeImage.color;
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
        tubeImage.color = glowColor;
        yield return new WaitForSeconds(flashDuration);
        tubeImage.color = originalColor;
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.ReceivePlayerInput(tubeID);
        }
    }
}
