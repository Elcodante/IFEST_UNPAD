using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationLogUI : MonoBehaviour
{
    public static NotificationLogUI Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Container tempat notifikasi muncul (Vertical Layout Group di pojok kanan bawah).")]
    [SerializeField] private Transform logContainer;

    [Tooltip("Prefab berisi TextMeshProUGUI untuk setiap baris pesan.")]
    [SerializeField] private GameObject logItemPrefab;

    [Tooltip("Berapa lama teks log tampil sebelum hilang (dalam detik).")]
    [SerializeField] private float displayDuration = 3.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Menampilkan notifikasi teks dengan warna tertentu.
    /// </summary>
    public void ShowLog(string message, Color textColor)
    {
        if (logItemPrefab == null || logContainer == null)
            return;

        GameObject logItem = Instantiate(logItemPrefab, logContainer);
        TextMeshProUGUI tmpText = logItem.GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.color = textColor;
        }

        // Hancurkan log item setelah beberapa detik
        Destroy(logItem, displayDuration);
    }
}