using System.Collections;
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
            // PERBAIKAN: Tahan proses jika game belum di-play (masih di menu)
            while (DayManager.instance != null && !DayManager.instance.waktuBerjalan)
            {
                yield return null;
            }

            float delay = Random.Range(minAttackDelay, maxAttackDelay);
            yield return new WaitForSeconds(delay);

            if (gameOver || (DayManager.instance != null && !DayManager.instance.waktuBerjalan))
                continue;

            AttackRandomRoom();
        }
    }

    private void AttackRandomRoom()
    {
        RoomController room = GetRandomAvailableRoom();

        if (room == null)
            return;

        room.SetUnderAttack();
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
        var available = new System.Collections.Generic.List<RoomController>();

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