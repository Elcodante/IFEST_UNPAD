using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class GearJamHazard : MonoBehaviour, IPointerDownHandler
{
    public DropZone parentDropZone;
    public float timeToClear = 2.0f;
    private Coroutine jamRoutine;

    public void ActivateHazard()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        if(jamRoutine != null )
        {
            StopCoroutine(jamRoutine);            
        }
        jamRoutine = StartCoroutine(JamTimer());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("[Hazard] Kerikil berhasil dibersihkan!");
        ClearHazard();
    }

    private void ClearHazard()
    {
        if (jamRoutine != null)
        {
            StopCoroutine(jamRoutine);
            gameObject.SetActive(false);
        }
    }

    private IEnumerator JamTimer()
    {
        yield return new WaitForSeconds(timeToClear);

        Debug.LogWarning("[Hazard] Terlambat! Mesin macet dan gerigi terpental.");
        if (parentDropZone != null)
        {
            parentDropZone.EjectItem();
        }

        gameObject.SetActive(false);
    }
}
