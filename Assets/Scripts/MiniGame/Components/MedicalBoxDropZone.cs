using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MedicalBoxDropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Masukkan script Manajer Minigame Drag di sini")]
    public MinigameDragManager minigameManager;

    [System.Serializable]
    public struct MedicalVisual
    {
        public string itemID;
        public GameObject visualObject;
    }

    public MedicalVisual[] visualMappings;
    private Dictionary<string, GameObject> visualDict = new Dictionary<string, GameObject>();

    [Header("Juice Effects")]
    [Tooltip("Masukkan partikel bintang/tanda plus di sini")]
    public ParticleSystem successParticles;

    private void Awake()
    {
        foreach (var mapping in visualMappings)
        {
            visualDict[mapping.itemID] = mapping.visualObject;
        }
    }

    private void OnEnable()
    {
        foreach (var visual in visualDict.Values)
        {
            if (visual != null)
            {
                visual.SetActive(false);
                // Reset skala ke normal setiap game dimulai ulang
                visual.transform.localScale = Vector3.one;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                if (visualDict.ContainsKey(draggedItem.itemID))
                {
                    draggedItem.gameObject.SetActive(false);

                    GameObject visual = visualDict[draggedItem.itemID];
                    if (visual != null)
                    {
                        visual.SetActive(true);

                        // JUICE 1: Hentikan animasi lama lalu mulai animasi membal
                        StopCoroutine("PopAnimation");
                        StartCoroutine(PopAnimation(visual.transform));

                        // JUICE 2: Pindahkan posisi partikel ke barang tersebut dan ledakkan
                        if (successParticles != null)
                        {
                            successParticles.transform.position = visual.transform.position;
                            successParticles.Emit(Random.Range(4, 7)); // Tembakkan 4-7 partikel
                        }
                    }

                    if (minigameManager != null)
                    {
                        minigameManager.AddCorrectMatch();
                    }
                }
                else
                {
                    Debug.Log("[P3K] Barang ini bukan barang medis!");

                    // JUICE 3: Getarkan rak P3K/layar saat pemain memasukkan barang salah
                    if (UIShaker.Instance != null)
                    {
                        UIShaker.Instance.Shake(0.25f, 15f);
                    }
                }
            }
        }
    }

    // Coroutine untuk membuat barang memantul (Snap)
    private IEnumerator PopAnimation(Transform target)
    {
        float duration = 0.25f;
        float time = 0;

        target.localScale = Vector3.zero;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Kurva overshoot: Membesar ke 1.2x lalu menetap di 1.0x
            float scale = Mathf.LerpUnclamped(0f, 1.2f, Mathf.Sin(t * Mathf.PI));
            if (t > 0.5f) scale = Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);

            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}