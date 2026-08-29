using System.Collections;
using UnityEngine;

public class O2WindManager : MinigameDragManager
{
    [Header("Wind Settings")]
    public bool isWindBlowing = false;
    public float minCalmTime = 2f;
    public float maxCalmTime = 4f;
    public float minWindTime = 1f;
    public float maxWindTime = 1f;

    [Header("Wind Visuals")]
    public GameObject windWarningUI;

    public static O2WindManager Instance;

    private Coroutine windRoutine;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable(); 

        isWindBlowing = false;
        if (windWarningUI != null)
        {
            windWarningUI.SetActive(false);
        }

        if (windRoutine != null)
        {
            StopCoroutine(windRoutine);
        }

        windRoutine = StartCoroutine(WindCycleRoutine());
    }

    private void OnDisable()
    {
        if (windRoutine != null)
        {
            StopCoroutine(windRoutine);
            windRoutine = null; 
        }
    }

    private IEnumerator WindCycleRoutine()
    {
        while (true)
        {
            isWindBlowing = false;
            if (windWarningUI != null)
            {
                windWarningUI.SetActive(false);
            }

            float calmDuration = Random.Range(minCalmTime, maxCalmTime);
            yield return new WaitForSeconds(calmDuration);

            isWindBlowing = true;
            if(windWarningUI != null)
            {
                windWarningUI.SetActive(true);
            }

            float windDuration = Random.Range(minWindTime, maxWindTime);
            yield return new WaitForSeconds(windDuration);
        }
    }

}
