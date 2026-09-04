using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Rooms")]
    [SerializeField] private RoomController[] rooms;

    [Header("Random Room Attack")]
    [SerializeField] private bool enableRandomAttacks = true;

    [Tooltip("Waktu minimum sebelum room berikutnya diserang.")]
    [SerializeField] private float minAttackDelay = 5f;

    [Tooltip("Waktu maksimum sebelum room berikutnya diserang.")]
    [SerializeField] private float maxAttackDelay = 12f;

    [Tooltip("Mencegah room yang sudah Dikuasai dipilih.")]
    [SerializeField] private bool excludeControlledRooms = true;

    [Header("Player")]
    [SerializeField] private PlayerResourceManager playerResources;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Audio Settings")]
    [Tooltip("ID SFX yang akan dipanggil di AudioManager saat diserang")]
    [SerializeField] private string attackSfxID = "Minigame_Bersih_Filter"; // Sesuaikan dengan ID audio serangan Anda

    private bool gameOver;
    private Coroutine attackRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetGame();

        if (enableRandomAttacks)
        {
            attackRoutine = StartCoroutine(RandomAttackRoutine());
        }
    }

    private void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.Alpha7))
        {
            RestartLevel();
        }
    }

    // =========================================================
    // RANDOM ATTACK
    // =========================================================

    private IEnumerator RandomAttackRoutine()
    {
        while (!gameOver)
        {
            // Tahan proses jika game belum di-play (masih di menu)
            while (DayManager.instance != null && !DayManager.instance.waktuBerjalan)
            {
                yield return null;
            }

            float delay = Random.Range(minAttackDelay, maxAttackDelay);
            float timer = 0f;

            while (timer < delay)
            {
                if (DayManager.instance != null && DayManager.instance.waktuBerjalan)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }

            if (gameOver) break;

            AttackRandomRoom();
        }
    }

    private void AttackRandomRoom()
    {
        RoomController room = GetRandomAvailableRoom();

        if (room == null)
            return;

        room.SetUnderAttack();

        // MENGGUNAKAN AUDIOMANAGER ANDA
        if (AudioManager.Instance != null)
        {
            // Saya menggunakan attackSfxID agar Anda bisa mengubah string-nya dari Inspector
            AudioManager.Instance.PlaySFX("Warning");

            // Catatan: Jika SFX serangan bukan tipe looping, 
            // Anda bisa menggantinya dengan: AudioManager.Instance.PlaySFX(attackSfxID);
        }
    }

    private RoomController GetRandomAvailableRoom()
    {
        RoomController[] availableRooms = GetAvailableRooms();

        if (availableRooms.Length == 0)
            return null;

        int index = Random.Range(0, availableRooms.Length);

        return availableRooms[index];
    }

    private RoomController[] GetAvailableRooms()
    {
        var available = new List<RoomController>();

        foreach (RoomController room in rooms)
        {
            if (room == null)
                continue;

            if (room.CurrentStatus == RoomStatus.Aman)
            {
                available.Add(room);
                continue;
            }

            if (!excludeControlledRooms &&
                room.CurrentStatus != RoomStatus.Diserang)
            {
                available.Add(room);
            }
        }

        return available.ToArray();
    }

    // =========================================================
    // RESOURCE & RESET
    // =========================================================

    public void RemoveHealth(float amount)
    {
        if (playerResources != null) playerResources.RemoveHealth(amount);
    }

    public void RemoveHunger(float amount)
    {
        if (playerResources != null) playerResources.RemoveHunger(amount);
    }

    public void ResetGame()
    {
        gameOver = false;
        Time.timeScale = 1f;

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (playerResources != null) playerResources.ResetResources();

        foreach (RoomController room in rooms)
        {
            if (room != null) room.ResetRoom();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOver) return;

        gameOver = true;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (gameOverUI != null) gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}