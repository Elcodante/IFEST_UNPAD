using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public abstract class BaseMinigameManager : MonoBehaviour
{
    [Header("Base Events")]
    public UnityEvent OnMinigameCompleted;

    [HideInInspector]
    public bool isGameInProgress = false;

    protected virtual void OnEnable()
    {
        if (!isGameInProgress)
        {
            ResetMinigame();
            isGameInProgress = true;
        }
    }

    public void TriggerWinCondition()
    {
        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(1f);

        isGameInProgress = false;
        OnMinigameCompleted?.Invoke();

        // 1. Jika ada RoomFocusController, selesaikan serangan ruangan & kembali ke CCTV
        if (RoomFocusController.Instance != null)
        {
            RoomFocusController.Instance.SelesaikanMinigameRuanganAktif();
        }
        else
        {
            // 2. Fallback manual jika RoomFocusController tidak digunakan
            if (DayManager.instance != null)
            {
                DayManager.instance.LanjutkanSistemWaktu();
            }
        }

        // 3. Pastikan panel minigame ini ditutup
        gameObject.SetActive(false);
    }

    protected abstract void ResetMinigame();

    public virtual void ForceResetState()
    {
        isGameInProgress = false;
    }
}