using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorGearManager : MinigameDragManager
{
    [Header("Gear Jam Settings")]
    public DropZone[] gearDropZones;
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 6f;

    [Header("Win Condition")]
    public int targetHazardsToClear = 5;
    public int currentHazardsCleared = 0;

    private Coroutine hazardRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentHazardsCleared = 0;

        if (gearDropZones != null)
        {
            foreach (DropZone zone in gearDropZones)
            {
                if (zone != null) zone.currentItem = null;
            }
        }

        if (hazardRoutine != null) StopCoroutine(hazardRoutine);
        hazardRoutine = StartCoroutine(HazardSpawner());
    }

    private void OnDisable()
    {
        if (hazardRoutine != null) StopCoroutine(hazardRoutine);
    }

    private IEnumerator HazardSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            List<DropZone> eligibleZones = new List<DropZone>();
            foreach (DropZone zone in gearDropZones)
            {
                if (zone != null && zone.currentItem != null)
                {
                    if (zone.myHazard != null && !zone.myHazard.gameObject.activeInHierarchy)
                    {
                        eligibleZones.Add(zone);
                    }
                }
            }

            if (eligibleZones.Count > 0)
            {
                DropZone targetZone = eligibleZones[Random.Range(0, eligibleZones.Count)];

                if (targetZone.myHazard != null)
                {
                    targetZone.myHazard.ActivateHazard();
                    Debug.Log($"[Hazard] Kerikil muncul di {targetZone.name}!");
                }
            }
        }
    }

    public void HazardCleared()
    {
        currentHazardsCleared++;

        if (currentHazardsCleared >= targetHazardsToClear)
        {
            if (hazardRoutine != null) StopCoroutine(hazardRoutine);
            Debug.Log("[Hazard] Kuota kerikil TERCAPAI! Mesin stabil.");
        }

        CheckGeneratorWinCondition();
    }

    public void CheckGeneratorWinCondition()
    {
        bool allGearsPlaced = true;
        int placedCount = 0;
        int totalValidZones = 0;

        foreach (DropZone zone in gearDropZones)
        {
            if (zone == null) continue; // Abaikan jika ada slot array yang kosong di Inspector

            totalValidZones++;

            if (zone.currentItem == null)
            {
                allGearsPlaced = false;
            }
            else
            {
                placedCount++;
            }
        }

        bool isHazardQuotaMet = currentHazardsCleared >= targetHazardsToClear;

        // Tracker transparan di Console Unity
        Debug.Log($"[WinCheck] Gerigi: {placedCount}/{totalValidZones} | Kerikil: {currentHazardsCleared}/{targetHazardsToClear}");

        if (allGearsPlaced && isHazardQuotaMet)
        {
            Debug.Log("[WinCheck] KEDUA SYARAT TERPENUHI! GAME SELESAI!");
            TriggerWinCondition();
        }
    }
}