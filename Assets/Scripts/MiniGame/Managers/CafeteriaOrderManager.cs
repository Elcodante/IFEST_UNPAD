using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CafeteriaOrderManager : BaseMinigameManager
{
    [Header("UI References")]
    public SinglePlate plateObject;
    public RectTransform plateRect; // Untuk animasi
    public TextMeshProUGUI orderListText; // Teks di kiri atas

    [Header("Animation Settings")]
    public Vector2 plateOnScreenPos = new Vector2(0, -150f); // Posisi piring di tengah meja
    public Vector2 plateOffScreenPos = new Vector2(0, -600f); // Posisi piring sembunyi di bawah
    public float animationDuration = 0.4f;

    [Header("Menu & Order Settings")]
    public string[] availableFoodTypes = { "Burger", "Sup", "Apel", "Air" };
    public int totalPlatesRequired = 4;

    private int currentPlateIndex = 0;
    private Dictionary<string, int> currentOrder = new Dictionary<string, int>();
    private Dictionary<string, int> currentProgress = new Dictionary<string, int>();

    private bool isAnimating = false;

    protected override void OnEnable()
    {
        base.OnEnable(); // Memanggil ResetMinigame jika belum berjalan
    }

    protected override void ResetMinigame()
    {
        currentPlateIndex = 0;
        isAnimating = false;

        // Posisikan piring di bawah layar, bersihkan, lalu mulai ronde pertama
        plateRect.anchoredPosition = plateOffScreenPos;
        plateObject.ClearPlate();

        StartCoroutine(StartNextPlateRound());
    }

    private IEnumerator StartNextPlateRound()
    {
        isAnimating = true;

        // 1. Generate Pesanan Baru untuk Piring Ini
        GenerateRandomOrder();
        UpdateOrderUI();

        // 2. Animasi Piring Naik ke Atas Meja
        yield return StartCoroutine(MovePlate(plateOffScreenPos, plateOnScreenPos));

        isAnimating = false;
    }

    private void GenerateRandomOrder()
    {
        currentOrder.Clear();
        currentProgress.Clear();

        // Acak minta 1 sampai 3 jenis makanan di 1 piring
        int itemsToOrder = Random.Range(1, 4);

        for (int i = 0; i < itemsToOrder; i++)
        {
            string randomFood = availableFoodTypes[Random.Range(0, availableFoodTypes.Length)];
            if (currentOrder.ContainsKey(randomFood))
            {
                currentOrder[randomFood]++; // Tambah porsi
            }
            else
            {
                currentOrder[randomFood] = 1; // Pesanan baru
                currentProgress[randomFood] = 0;
            }
        }
    }

    // Dipanggil oleh SinglePlate.cs saat pemain menaruh makanan
    public bool TryAddFood(string foodID)
    {
        if (isAnimating) return false; // Jangan terima makanan saat piring masih bergerak

        // Jika piring butuh makanan ini dan kuotanya belum penuh
        if (currentOrder.ContainsKey(foodID) && currentProgress[foodID] < currentOrder[foodID])
        {
            currentProgress[foodID]++;
            UpdateOrderUI();
            CheckPlateComplete();
            return true;
        }

        Debug.Log($"[Kafe] Salah pesanan! Piring tidak butuh {foodID} lagi.");
        return false;
    }

    private void UpdateOrderUI()
    {
        string text = $"PIRING {currentPlateIndex + 1}/{totalPlatesRequired}\n\nPESANAN:\n";
        foreach (var order in currentOrder)
        {
            string foodName = order.Key;
            int needed = order.Value;
            int current = currentProgress[foodName];

            // Coret warna hijau jika pesanan spesifik ini sudah terpenuhi
            if (current >= needed)
                text += $"<color=#00FF00><s>{foodName} {current}/{needed}</s></color>\n";
            else
                text += $"{foodName} {current}/{needed}\n";
        }
        orderListText.text = text;
    }

    private void CheckPlateComplete()
    {
        foreach (var order in currentOrder)
        {
            if (currentProgress[order.Key] < order.Value) return; // Belum beres
        }

        // Jika semua pesanan di piring beres
        currentPlateIndex++;
        StartCoroutine(CompletePlateRoutine());
    }

    private IEnumerator CompletePlateRoutine()
    {
        isAnimating = true;

        // Tunggu 0.5 detik biar pemain bisa melihat piringnya penuh
        yield return new WaitForSeconds(0.5f);

        // Animasi Piring Turun
        yield return StartCoroutine(MovePlate(plateOnScreenPos, plateOffScreenPos));

        plateObject.ClearPlate();

        if (currentPlateIndex >= totalPlatesRequired)
        {
            // MENANG KESELURUHAN
            TriggerWinCondition();
        }
        else
        {
            // LANJUT PIRING BERIKUTNYA
            StartCoroutine(StartNextPlateRound());
        }
    }

    private IEnumerator MovePlate(Vector2 startPos, Vector2 endPos)
    {
        float time = 0;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animationDuration);
            plateRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        plateRect.anchoredPosition = endPos;
    }
}