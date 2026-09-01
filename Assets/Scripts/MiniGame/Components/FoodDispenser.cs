using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodDispenser : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Food Identity")]
    public string foodID;
    public Sprite dragIconSprite;

    [Header("Display Settings")]
    [Tooltip("Berapa kali lipat gambar ingin dibesarkan dari ukuran aslinya? (Default: 10)")]
    public float scaleMultiplier = 3f; // Variabel pengali otomatis

    private GameObject ghostIcon;

    public void OnBeginDrag(PointerEventData eventData)
    {
        ghostIcon = new GameObject($"Ghost_{foodID}");

        ghostIcon.transform.SetParent(this.transform.root);
        ghostIcon.transform.SetAsLastSibling();

        Image img = ghostIcon.AddComponent<Image>();
        img.sprite = dragIconSprite;

        // 1. Baca ukuran asli dari gambarnya (misal: 10x13 atau 12x12)
        img.SetNativeSize();

        // 2. Kalikan ukuran tersebut dengan scaleMultiplier (misal: dikali 10)
        RectTransform rt = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x * scaleMultiplier, rt.sizeDelta.y * scaleMultiplier);

        img.raycastTarget = false;

        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) Destroy(ghostIcon);
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (ghostIcon != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)ghostIcon.transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos))
        {
            ghostIcon.transform.localPosition = localPos;
        }
    }
}