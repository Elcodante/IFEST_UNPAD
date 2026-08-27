using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MinigamePlaceholder : MonoBehaviour
{
    [Header("Minigame Events")]
    public UnityEvent OnMinigameCompleted;

    /// <summary>
    /// Fungsi ini akan dipanggil oleh MinigameTrigger saat UI Danger diklik.
    /// </summary>

    public void OpenMinigame()
    {
        gameObject.SetActive(true);
        Debug.Log("[Minigame] Panel Terbuka! Tekan tombol '1' untuk pura-pura selesai.");
    }

    public void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            CompleteMinigame();
        }
    }

    /// <summary>
    /// Fungsi untuk menyelesaikan dan menutup minigame.
    /// </summary>

    private void CompleteMinigame()
    {
        gameObject.SetActive(false);
        OnMinigameCompleted?.Invoke();
        Debug.Log("[Minigame] Panel Ditutup! Minigame selesai.");
    }
}
