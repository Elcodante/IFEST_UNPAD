using UnityEngine;
using System.Collections.Generic;

public enum GasDirection { Atas, Kanan, Bawah, Kiri }

[System.Serializable]
public class PipeLevel
{
    public string levelName = "Variasi Level";
    public GameObject levelContainerUI;
    public PipeNode[] gridPipes;

    [Header("Titik Masuk (Start)")]
    public int startIndex;
    public GasDirection masukDariArah;

    [Header("Titik Keluar (exit)")]
    public int endIndex;
    public GasDirection keluarKeArah;

}

public class PipePuzzleManager : BaseMinigameManager
{
    [Header("Level Pooling")]
    public PipeLevel[] kumpulanLevel;

    private PipeLevel levelAktif;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ResetMinigame()
    {
        foreach (PipeLevel level in kumpulanLevel)
        {
            if (level.levelContainerUI != null)
            {
                level.levelContainerUI.SetActive(false);
            }
        }

        levelAktif = kumpulanLevel[Random.Range(0, kumpulanLevel.Length)];
        levelAktif.levelContainerUI.SetActive(true);

        foreach (PipeNode pipe in levelAktif.gridPipes)
        {
            if (pipe != null)
            {
                pipe.InitPipe(this);
                pipe.RandomizeRotation();
            }
        }

        EvaluateFlow();
    }

    public void EvaluateFlow()
    {
        if (levelAktif == null)
        {
            return;
        }

        PipeNode[] pipes = levelAktif.gridPipes;

        foreach (PipeNode pipe in pipes)
        {
            if (pipe != null)
            {
                pipe.SetFlowState(false);
            }
        }

        if (!CekArah(pipes[levelAktif.startIndex], levelAktif.masukDariArah))
        {
            return;
        }

        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(levelAktif.startIndex);
        visited.Add(levelAktif.startIndex);

        bool reachedEnd = false;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            pipes[current].SetFlowState(true);

            if (current == levelAktif.endIndex) reachedEnd = true;

            int x = current % 3;
            int y = current / 3;
            PipeNode currentPipe = pipes[current];

            // Cek Atas
            if (y > 0 && currentPipe.HasTop() && !visited.Contains(current - 3) && pipes[current - 3].HasBottom())
            {
                visited.Add(current - 3); queue.Enqueue(current - 3);
            }
            // Cek Bawah
            if (y < 2 && currentPipe.HasBottom() && !visited.Contains(current + 3) && pipes[current + 3].HasTop())
            {
                visited.Add(current + 3); queue.Enqueue(current + 3);
            }
            // Cek Kiri
            if (x > 0 && currentPipe.HasLeft() && !visited.Contains(current - 1) && pipes[current - 1].HasRight())
            {
                visited.Add(current - 1); queue.Enqueue(current - 1);
            }
            // Cek Kanan
            if (x < 2 && currentPipe.HasRight() && !visited.Contains(current + 1) && pipes[current + 1].HasLeft())
            {
                visited.Add(current + 1); queue.Enqueue(current + 1);
            }
        }

        // Cek apakah gas sampai ke ujung DAN menghadap ke tangki luar dengan benar
        bool isOutputConnected = CekArah(pipes[levelAktif.endIndex], levelAktif.keluarKeArah);

        if (reachedEnd && isOutputConnected)
        {
            TriggerWinCondition();
        }
    }

    private bool CekArah(PipeNode pipe, GasDirection arah)
    {
        switch (arah)
        {
            case GasDirection.Atas: return pipe.HasTop();
            case GasDirection.Kanan: return pipe.HasRight();
            case GasDirection.Bawah: return pipe.HasBottom();
            case GasDirection.Kiri: return pipe.HasLeft();
            default: return false;
        }
    }
}