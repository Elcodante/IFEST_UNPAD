using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public abstract class BaseMinigameManager : MonoBehaviour
{
    [Header("Base Events")]
    public UnityEvent OnMinigameCompleted;

    protected virtual void OnEnable()
    {
        ResetMinigame();
    }

    public void TriggerWinCondition()
    {
        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(1f);
        OnMinigameCompleted?.Invoke();
        gameObject.SetActive(false);
    }

    protected abstract void ResetMinigame();
}
