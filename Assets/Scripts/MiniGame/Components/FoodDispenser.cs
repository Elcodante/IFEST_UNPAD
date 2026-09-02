using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class FoodDispenser : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Food Identity")]
    public string foodID;
    public Sprite dragIconSprite;

    [Header("Display Settings")]
    public float scaleMultiplier = 3f;

    private GameObject ghostIcon;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // JUICE: Mainkan efek wadah memipih saat kursor mulai menarik
        StopCoroutine("SquishAnimation");
        StartCoroutine(SquishAnimation());

        ghostIcon = new GameObject($"Ghost_{foodID}");

        // --- PERBAIKAN SKALA ---
        // Tambahkan kata 'false' agar Unity tidak merusak ukuran aslinya
        ghostIcon.transform.SetParent(this.transform.root, false);

        // Kunci paksa skala ke ukuran normal (1x)
        ghostIcon.transform.localScale = Vector3.one;
        // -----------------------

        ghostIcon.transform.SetAsLastSibling();

        Image img = ghostIcon.AddComponent<Image>();
        img.sprite = dragIconSprite;

        // Baca ukuran asli gambar pixel-mu
        img.SetNativeSize();

        // Kalikan dengan multiplier (Cek Inspector, jika 3 masih terlalu besar, ubah jadi 1 atau 2)
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

    // Coroutine untuk membuat wadah memipih sebentar (Squish)
    private IEnumerator SquishAnimation()
    {
        float duration = 0.15f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // X melebar sedikit (1.1), Y memendek (0.8)
            float scaleX = Mathf.Lerp(originalScale.x, originalScale.x * 1.1f, Mathf.PingPong(t * 2, 1));
            float scaleY = Mathf.Lerp(originalScale.y, originalScale.y * 0.8f, Mathf.PingPong(t * 2, 1));

            transform.localScale = new Vector3(scaleX, scaleY, 1f);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}