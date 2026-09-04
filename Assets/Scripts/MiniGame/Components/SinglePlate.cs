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

    // --- PENGATURAN AUDIO ---
    [Header("Audio Settings")]
    public string dropFoodSoundID = "SFX_Taruh_Makanan";
    public string errorSoundID = "SFX_Salah";
    public string slidePlateSoundID = "SFX_Geser_Piring";

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
                // JUICE AUDIO: Suara makanan mendarat di piring
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFXRandomPitch(dropFoodSoundID);
                }

                GameObject visual = visualDict[dispenser.foodID];
                if (visual != null)
                {
                    visual.SetActive(true);

                    StopCoroutine("PopAnimation");
                    StartCoroutine(PopAnimation(visual.transform));
                }
            }
            else
            {
                // JUICE AUDIO: Suara makanan salah/ditolak
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(errorSoundID);
                }

                if (UIShaker.Instance != null)
                {
                    UIShaker.Instance.Shake(0.2f, 10f);
                }
            }
        }
    }

    // Fungsi ini biasanya dipanggil oleh CafeteriaOrderManager saat pesanan selesai/direset
    public void ClearPlate()
    {
        // JUICE AUDIO: Suara piring bergeser/ditarik 
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(slidePlateSoundID);
        }

        foreach (var visual in visualDict.Values)
        {
            if (visual != null)
            {
                visual.SetActive(false);
                visual.transform.localScale = Vector3.one;
            }
        }
    }

    // (Opsional) Jika piring bergeser masuk saat level baru mulai, panggil fungsi ini dari Manajer
    public void SlidePlateIn()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(slidePlateSoundID);
        }
    }

    private IEnumerator PopAnimation(Transform target)
    {
        float duration = 0.25f;
        float time = 0;

        target.localScale = Vector3.zero;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float scale = Mathf.LerpUnclamped(0f, 1.2f, Mathf.Sin(t * Mathf.PI));
            if (t > 0.5f) scale = Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);

            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}