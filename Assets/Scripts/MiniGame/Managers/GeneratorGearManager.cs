using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorGearManager : MinigameDragManager
{
    [Header("Gear Jam Settings")]
    [Tooltip("Masukkan semua Drop Zone gerigi ke sini")]
    public DropZone[] gearDropZones;
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 6f;

    private Coroutine hazardRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();

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
                // Cek apakah Drop Zone memiliki gerigi DAN kita sudah menyambungkan kerikilnya
                if (zone != null && zone.currentItem != null)
                {
                    // Gunakan referensi langsung, BUKAN GetComponentInChildren
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
}