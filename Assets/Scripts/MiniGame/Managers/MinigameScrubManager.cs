using UnityEngine;

public class MinigameScrubManager : BaseMinigameManager
{
    [Header("Scrubing Minigame")]
    public ScrubbableDirt[] allDirts;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ResetMinigame()
    {
        foreach (ScrubbableDirt dirt in allDirts)
        {
            if (dirt != null)
            {
                dirt.manager = this;
                dirt.ResetDirt();
            }
        }
    }

    public void ChekWinCondition()
    {
        bool allClean = true;
        foreach (ScrubbableDirt dirt in allDirts)
        {
            if (dirt != null && !dirt.CheckIfClean())
            {
                allClean = false;
                break;
            }
        }

        if (allClean) 
        {
            TriggerWinCondition();
        }
    }
}
