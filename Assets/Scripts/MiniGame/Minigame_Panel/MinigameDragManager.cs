using UnityEngine;
public class MinigameDragManager : BaseMinigameManager
{
    [Header("Drag Minigame Settings")]
    public int totalRequiredMatches = 3;
    public DraggableItem[] allDraggableItems;
    public WireDragItem[] allWireDragItems;

    public bool randomizeOnStart = true;
    public Transform leftContainer;
    public Transform rightContainer;

    private int currentMatches = 0;

    public void AddCorrectMatch()
    {
        currentMatches++;
        Debug.Log($"[Drag Minigame] Progress: {currentMatches}/{totalRequiredMatches}");

        if (currentMatches >= totalRequiredMatches)
        {
            // Panggil fungsi menang dari Base Class
            TriggerWinCondition();
        }
    }

    // Wajib ada karena kita menggunakan fungsi abstrak di Base
    protected override void ResetMinigame()
    {
        currentMatches = 0;

        if (randomizeOnStart)
        {
            ShuffleContainer(leftContainer);
            ShuffleContainer(rightContainer);
        }

        foreach (DraggableItem item in allDraggableItems)
        {
            if (item != null) item.ReturnToStart();
        }

        foreach (WireDragItem wire in allWireDragItems)
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
            // Pilih satu index acak dari anak-anak yang ada
            int randomIndex = Random.Range(0, childCount);

            // Ambil objek pada urutan saat ini
            Transform child = container.GetChild(i);

            // Pindahkan objek tersebut ke urutan baru (Sibling Index)
            // Layout Group akan otomatis menyesuaikan visual di layar!
            child.SetSiblingIndex(randomIndex);
        }
    }
}