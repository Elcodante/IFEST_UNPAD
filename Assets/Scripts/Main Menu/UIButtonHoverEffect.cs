using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float clickScale = 0.92f;
    [SerializeField] private float animationSpeed = 12f;

    private Vector3 normalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        normalScale = transform.localScale;
        targetScale = normalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = normalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }
}