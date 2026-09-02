using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SinglePlate : MonoBehaviour, IDropHandler
{
    public CafeteriaOrderManager manager;

    [System.Serializable]
    public struct FoodVisualMapping
    {
        public string foodID;
        public GameObject foodVisualObject;
    }

    public FoodVisualMapping[] visualMappings;
    private Dictionary<string, GameObject> visualDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var mapping in visualMappings)
        {
            visualDict[mapping.foodID] = mapping.foodVisualObject;

            if (mapping.foodVisualObject != null)
            {
                mapping.foodVisualObject.SetActive(false);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        FoodDispenser dispenser = eventData.pointerDrag?.GetComponent<FoodDispenser>();

        if (dispenser != null)
        {
            bool accepted = manager.TryAddFood(dispenser.foodID);

            if (accepted && visualDict.ContainsKey(dispenser.foodID))
            {
                GameObject visual = visualDict[dispenser.foodID];
                if (visual != null)
                {
                    visual.SetActive(true);

                    // JUICE: Hentikan animasi lama (jika ditumpuk cepat) lalu mulai animasi pantul
                    StopCoroutine("PopAnimation");
                    StartCoroutine(PopAnimation(visual.transform));
                }
            }
            else
            {
                // JUICE: Getarkan piring/layar saat pemain menaruh makanan yang salah
                if (UIShaker.Instance != null)
                {
                    UIShaker.Instance.Shake(0.2f, 10f); // Getaran pendek dan ringan
                }
            }
        }
    }

    public void ClearPlate()
    {
        foreach (var visual in visualDict.Values)
        {
            if (visual != null)
            {
                visual.SetActive(false);
                visual.transform.localScale = Vector3.one; // Reset skala ke normal
            }
        }
    }

    // Coroutine untuk membuat makanan memantul (membesar lalu mengecil) saat ditaruh
    private IEnumerator PopAnimation(Transform target)
    {
        float duration = 0.25f;
        float time = 0;

        // Mulai dari ukuran 0 (hilang)
        target.localScale = Vector3.zero;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Kurva overshoot sederhana: membesar hingga 1.2x lalu menetap di 1.0x
            float scale = Mathf.LerpUnclamped(0f, 1.2f, Mathf.Sin(t * Mathf.PI));
            if (t > 0.5f) scale = Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);

            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}