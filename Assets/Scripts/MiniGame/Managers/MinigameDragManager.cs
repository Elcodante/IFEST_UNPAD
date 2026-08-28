using System.Collections.Generic;
using UnityEngine;

public class MinigameDragManager : BaseMinigameManager
{
    [Header("Drag Minigame Settings")]
    public int totalRequiredMatches = 3;

    [Header("Item References")]
    public DraggableItem[] allDraggableItems;
    public WireDragItem[] allWireItems;

    [Header("Randomizer Settings (Untuk Kabel)")]
    public bool randomizeOnStart = true;
    public Transform leftContainer;
    public Transform rightContainer;

    [Header("Random Spawner (Untuk Medis/Lantai)")]
    public bool enableRandomSpawning = false;
    [Tooltip("Masukkan objek UI kosong (Panel transparan) sebagai batas area lantai")]
    public RectTransform floorSpawnArea;
    public int minSpawnCount = 4;
    public int maxSpawnCount = 7;
    public int maxSpawnAttempts = 50;
    private int currentMatches = 0;

    public void AddCorrectMatch()
    {
        currentMatches++;
        Debug.Log($"[Drag Minigame] Progress: {currentMatches}/{totalRequiredMatches}");

        if (currentMatches >= totalRequiredMatches)
        {
            TriggerWinCondition();
        }
    }

    protected override void ResetMinigame()
    {
        currentMatches = 0;

        if (randomizeOnStart)
        {
            ShuffleContainer(leftContainer);
            ShuffleContainer(rightContainer);
        }

        // PERBAIKAN: Gunakan Coroutine agar Unity punya waktu menggambar UI
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ResetAndSpawnRoutine());
        }
    }

    private System.Collections.IEnumerator ResetAndSpawnRoutine()
    {
        // Tunggu 1 frame agar panjang & lebar UI terbaca bukan 0
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases(); // Paksa Unity mengkalkulasi UI

        // Setelah ukuran UI asli terbaca, jalankan logika spawn
        if (enableRandomSpawning && floorSpawnArea != null)
        {
            RandomizeAndPlaceItems();
        }
        else
        {
            foreach (DraggableItem item in allDraggableItems)
            {
                if (item != null) item.ReturnToStart();
            }
        }

        foreach (WireDragItem wire in allWireItems)
        {
            if (wire != null)
            {
                wire.ReturnToStart();
                wire.enabled = true;
            }
        }
    }

    private void ShuffleContainer(Transform container)
    {
        if (container == null) return;
        int childCount = container.childCount;
        for (int i = 0; i < childCount; i++)
        {
            container.GetChild(i).SetSiblingIndex(Random.Range(0, childCount));
        }
    }

    /// <summary>
    /// Logika utama untuk menempatkan barang medis secara acak tanpa tabrakan
    /// </summary>
    private void RandomizeAndPlaceItems()
    {
        int targetCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        totalRequiredMatches = targetCount;

        List<Rect> placedRects = new List<Rect>();

        for (int i = 0; i < allDraggableItems.Length; i++)
        {
            DraggableItem item = allDraggableItems[i];
            if (item == null) continue;

            if (i < targetCount)
            {
                item.gameObject.SetActive(true);

                RectTransform itemRT = item.GetComponent<RectTransform>();
                Vector2 newPos = GetRandomNonOverlappingPosition(itemRT, placedRects);

                item.transform.SetParent(floorSpawnArea, false);
                item.SetNewStartData(floorSpawnArea, newPos);

                item.ReturnToStart();
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Mencari koordinat acak di dalam area lantai yang tidak bertabrakan dengan barang lain
    /// </summary>
    private Vector2 GetRandomNonOverlappingPosition(RectTransform itemRT, List<Rect> placedRects)
    {
        float itemWidth = itemRT.rect.width == 0 ? 50 : itemRT.rect.width; // Failsafe jika masih 0
        float itemHeight = itemRT.rect.height == 0 ? 50 : itemRT.rect.height;

        // Tentukan batas aman
        float minX = floorSpawnArea.rect.xMin + (itemWidth / 2);
        float maxX = floorSpawnArea.rect.xMax - (itemWidth / 2);
        float minY = floorSpawnArea.rect.yMin + (itemHeight / 2);
        float maxY = floorSpawnArea.rect.yMax - (itemHeight / 2);

        // Jika ukuran item kebesaran, paksa batasnya
        if (minX > maxX) { minX = floorSpawnArea.rect.xMin; maxX = floorSpawnArea.rect.xMax; }
        if (minY > maxY) { minY = floorSpawnArea.rect.yMin; maxY = floorSpawnArea.rect.yMax; }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            Vector2 randomPos = new Vector2(randomX, randomY);

            Rect potentialRect = new Rect(randomPos.x - (itemWidth / 2), randomPos.y - (itemHeight / 2), itemWidth, itemHeight);

            bool overlaps = false;
            foreach (Rect r in placedRects)
            {
                if (potentialRect.Overlaps(r))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                placedRects.Add(potentialRect);
                return randomPos;
            }
        }
        return Vector2.zero;
    }

    public void RemoveCorrectMatch()
    {
        currentMatches--;
        if(currentMatches < 0)
        {
            currentMatches = 0;
            Debug.Log($"[Minigame] Objek terpental! Progress mundur: {currentMatches}/{totalRequiredMatches}");
        }
    }
}