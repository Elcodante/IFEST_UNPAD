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
        if (jamRoutine != null)
        {
            StopCoroutine(jamRoutine);
        }
        jamRoutine = StartCoroutine(JamTimer());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Lapor ke Manajer DULU
        if (parentDropZone != null && parentDropZone.minigameManager is GeneratorGearManager gearManager)
        {
            gearManager.HazardCleared();
        }
        else
        {
            Debug.LogError("[Hazard Error] parentDropZone belum tersambung ke kerikil ini!");
        }

        // 2. Baru matikan visualnya
        Debug.Log("[Hazard] Kerikil berhasil dibersihkan dari UI!");
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