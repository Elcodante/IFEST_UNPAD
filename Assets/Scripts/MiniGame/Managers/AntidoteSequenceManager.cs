using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class AntidoteSequenceManager : BaseMinigameManager
{
    [Header("Antidote Settings")]
    public TestTubeButton[] testTubes;
    public int sequenceLenght = 5;
    public float timeToInput = 8f;

    [Header("UI Feedback")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    // --- TAMBAHAN AUDIO ---
    [Header("Audio Settings")]
    public string machineSequenceSoundID = "SFX_Antidote_Mesin";
    public string successSoundID = "SFX_Antidote_Menang";
    public string failSoundID = "SFX_Antidote_Gagal";
    // ----------------------

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

        if (statusText != null) statusText.text = "ANALISIS POLA...";
        if (timerText != null)
        {
            timerText.text = "--.--";
            timerText.color = Color.white;
            timerText.transform.localScale = Vector3.one;
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
            // JUICE AUDIO 1: Suara botol mengatur urutan (Mesin). 
            // Pitch dimanipulasi agar setiap botol punya nada berbeda!
            if (AudioManager.Instance != null)
            {
                float nadaBotol = 0.8f + (id * 0.15f); // Menghasilkan pitch 0.8, 0.95, 1.1, dst.
                AudioManager.Instance.PlaySFXRandomPitch(machineSequenceSoundID, nadaBotol, nadaBotol);
            }

            testTubes[id].FlashTube();
            yield return new WaitForSeconds(0.6f);
        }

        isMachinePlaying = false;
        isWaitingForInput = true;
        currentTimer = timeToInput;

        if (statusText != null) statusText.text = "MASUKKAN PENAWAR!";
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            currentTimer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = currentTimer.ToString("F2") + "s";

                if (currentTimer <= 3.0f)
                {
                    timerText.color = Color.red;
                    float pulse = 1f + Mathf.PingPong(Time.time * 5f, 0.2f);
                    timerText.transform.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    timerText.color = Color.white;
                    timerText.transform.localScale = Vector3.one;
                }
            }

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                FailSequence("WAKTU HABIS!");
            }
        }
    }

    public void ReceivePlayerInput(int clickedTubeID)
    {
        if (isMachinePlaying || !isWaitingForInput) return;

        testTubes[clickedTubeID].FlashTube();

        if (clickedTubeID == currentSequence[playerInputIndex])
        {
            playerInputIndex++;

            if (playerInputIndex >= currentSequence.Count)
            {
                isWaitingForInput = false;
                if (statusText != null) statusText.text = "SINTESIS BERHASIL!";
                if (timerText != null) timerText.transform.localScale = Vector3.one;

                // JUICE AUDIO 2: Suara Menang
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(successSoundID);

                TriggerWinCondition();
            }
        }
        else
        {
            FailSequence("URUTAN SALAH!");
        }
    }

    private void FailSequence(string failReason)
    {
        isWaitingForInput = false;
        if (statusText != null) statusText.text = failReason;

        Debug.LogWarning($"[Antidote] Gagal: {failReason}");

        // JUICE AUDIO 3: Suara Gagal (Kaca pecah / kimia mendesis)
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(failSoundID);

        if (UIShaker.Instance != null)
        {
            UIShaker.Instance.Shake(0.35f, 20f);
        }

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        ResetMinigame();
    }
}