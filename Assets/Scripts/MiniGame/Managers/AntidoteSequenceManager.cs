using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
public class AntidoteSequenceManager : BaseMinigameManager
{
    [Header("Andtidote Settings")]
    public TestTubeButton[] testTubes;
    public int sequenceLenght = 5;
    public float timeToInput = 8f;

    [Header("UI Feedback")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    private List<int> currentSequence = new List<int>();
    private int playerInputIndex = 0;

    private bool isWaitingForInput = false;
    private bool isMachinePlaying = false;
    private float currentTimer;

    protected override void OnEnable()
    {
        base.OnEnable();

        for (int i = 0; i < testTubes.Length; i++)
        {
            if (testTubes[i] != null)
            {
                testTubes[i].tubeID = i;
                testTubes[i].manager = this;
            }
        }
    }

    protected override void ResetMinigame()
    {
        StopAllCoroutines();
        isWaitingForInput = false;
        isMachinePlaying = true;
        playerInputIndex = 0;
        currentSequence.Clear();

        if (statusText != null)
        {
            statusText.text = "ANALISIS POLA...";
        }
        if (timerText != null)
        {
            timerText.text = "--.--";
        }

        for (int i = 0; i < sequenceLenght; i++)
        {
            currentSequence.Add(Random.Range(0, testTubes.Length));
        }

        StartCoroutine(PlayMachineSequence());
    }

    private IEnumerator PlayMachineSequence()
    {
        yield return new WaitForSeconds(1f);

        foreach (int id in currentSequence)
        {
            testTubes[id].FlashTube();

            yield return new WaitForSeconds(0.6f);
        }

        isMachinePlaying = false;
        isWaitingForInput = true;
        currentTimer = timeToInput;

        if (statusText != null)
        {
            statusText.text = "Masukkan Penawar!";
        }
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            currentTimer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = currentTimer.ToString("F2") + "s";
            }

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                FailSequence("Waktu Habis");
            }
        }
    }

    public void ReceivePlayerInput(int clickedTubeID)
    {
        if (isMachinePlaying || !isWaitingForInput)
        {
            return;
        }

        testTubes[clickedTubeID].FlashTube();

        if (clickedTubeID == currentSequence[playerInputIndex])
        {
            playerInputIndex++;

            if (playerInputIndex >= currentSequence.Count)
            {
                isWaitingForInput = false;
                if (statusText != null) statusText.text = "SINTESIS BERHASIL!";
                TriggerWinCondition();
            }
        }
        else
        {
            FailSequence("Urutan Salah");
        }
    }

    private void FailSequence(string failReason)
    {
        isWaitingForInput = false;
        if (statusText != null)
        {
            statusText.text = failReason;
        }
        Debug.LogWarning($"[Antidote] Gagal: {failReason}");

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        ResetMinigame(); 
    }
}