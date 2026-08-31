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
        //gameObject.SetActive(false);
    }

    protected abstract void ResetMinigame();

    //Fungsi jika dibutuhkan 

    public virtual void ForceResetState()
    {
        isGameInProgress = false;
    }
}
