using UnityEngine;
using System.Collections.Generic;

public enum GasDirection { Atas, Kanan, Bawah, Kiri }

[System.Serializable]
public class PipeLevel
{
    public string levelName = "Variasi Level";
    public GameObject levelContainerUI;

    [Header("Ukuran Grid")]
    [Tooltip("Contoh: 6 untuk grid 6x4")]
    public int jumlahKolom = 6;
    [Tooltip("Contoh: 4 untuk grid 6x4")]
    public int jumlahBaris = 4;

    public PipeNode[] gridPipes;

    [Header("Titik Masuk (Start)")]
    public int startIndex;
    public GasDirection masukDariArah;

    [Header("Titik Keluar (Exit)")]
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
        if (levelAktif == null) return;

        PipeNode[] pipes = levelAktif.gridPipes;
        int cols = levelAktif.jumlahKolom;
        int rows = levelAktif.jumlahBaris;

        foreach (PipeNode pipe in pipes)
        {
            if (pipe != null) pipe.SetFlowState(false);
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

            int x = current % cols;
            int y = current / cols;
            PipeNode currentPipe = pipes[current];

            // Cek Atas (Kurangi indeks dengan jumlah kolom)
            if (y > 0 && currentPipe.HasTop() && !visited.Contains(current - cols) && pipes[current - cols].HasBottom())
            {
                visited.Add(current - cols); queue.Enqueue(current - cols);
            }
            // Cek Bawah (Tambahkan indeks dengan jumlah kolom)
            if (y < rows - 1 && currentPipe.HasBottom() && !visited.Contains(current + cols) && pipes[current + cols].HasTop())
            {
                visited.Add(current + cols); queue.Enqueue(current + cols);
            }
            // Cek Kiri (Kurangi indeks dengan 1)
            if (x > 0 && currentPipe.HasLeft() && !visited.Contains(current - 1) && pipes[current - 1].HasRight())
            {
                visited.Add(current - 1); queue.Enqueue(current - 1);
            }
            // Cek Kanan (Tambahkan indeks dengan 1)
            if (x < cols - 1 && currentPipe.HasRight() && !visited.Contains(current + 1) && pipes[current + 1].HasLeft())
            {
                visited.Add(current + 1); queue.Enqueue(current + 1);
            }
        }

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