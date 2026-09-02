using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class CCTVEffectController : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI recText;
    [SerializeField] private TextMeshProUGUI cameraLabelText;

    [Header("Settings")]
    [SerializeField] private string cameraName = "CAM 01 - MAIN FACILITY";
    [SerializeField] private float recBlinkInterval = 0.8f;

    private void Start()
    {
        if (cameraLabelText != null)
            cameraLabelText.text = cameraName;

        StartCoroutine(BlinkRECRoutine());
    }

    private void Update()
    {
        if (timeText != null)
        {
            DateTime now = DateTime.Now;
            timeText.text = now.ToString("yyyy-MM-dd HH:mm:ss") + $":{now.Millisecond / 10:D2}";
        }
    }

    private IEnumerator BlinkRECRoutine()
    {
        while (true)
        {
            if (recText != null)
                recText.enabled = !recText.enabled;

            yield return new WaitForSeconds(recBlinkInterval);
        }
    }
}