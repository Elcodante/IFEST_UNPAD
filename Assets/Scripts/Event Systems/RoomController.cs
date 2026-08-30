using UnityEngine;
using UnityEngine.UI;

public class RoomController : MonoBehaviour
{
    [Header("Room Identity")]
    [SerializeField] private string roomName = "Room";

    [Header("Initial Status")]
    [SerializeField] private RoomStatus initialStatus = RoomStatus.Aman;

    [Header("Attack Settings")]
    [Tooltip("Cooldown sebelum Diserang berubah menjadi Dikuasai.")]
    [SerializeField] private float captureCooldown = 10f;

    [Header("Controlled Room Effect")]
    [Tooltip("Resource yang terkena efek ketika room dikuasai.")]
    [SerializeField] private RoomResourceType controlledResource = RoomResourceType.Health;

    [Tooltip("Jumlah resource yang berkurang setiap tick.")]
    [SerializeField] private float damageAmount = 5f;

    [Tooltip("Jarak waktu antar damage tick.")]
    [SerializeField] private float damageInterval = 3f;

    [Header("Attack Hunger Settings")]
    [Tooltip("Aktifkan bila room ini adalah room khusus Hunger.")]
    [SerializeField] private bool affectsHungerWhenAttacked = false;

    [Tooltip("Jumlah Hunger yang berkurang setiap tick saat room sedang Diserang.")]
    [SerializeField] private float hungerDrainAmount = 5f;

    [Tooltip("Interval pengurangan Hunger.")]
    [SerializeField] private float hungerDrainInterval = 2f;

    [Header("UI Notification")]
    [SerializeField] private GameObject attackedNotification;
    [SerializeField] private GameObject controlledNotification;

    [Header("Minigame Triggers")]
    [Tooltip("Semua MinigameTrigger yang ada di room ini. Saat room diserang, salah satu akan dipilih secara acak untuk diaktifkan.")]
    [SerializeField] private MinigameTrigger[] minigameTriggers;

    private RoomStatus currentStatus;
    private float captureTimer;
    private float damageTimer;
    private float hungerTimer;

    public RoomStatus CurrentStatus => currentStatus;
    public string RoomName => roomName;

    /// <summary>
    /// Trigger yang sedang aktif (dipilih random) untuk serangan saat ini, kalau ada.
    /// Null kalau room sedang Aman/Dikuasai atau minigame-nya sudah diklik/diselesaikan.
    /// </summary>
    public MinigameTrigger CurrentActiveTrigger { get; private set; }

    private void Awake()
    {
        ResetRoom();
    }

    private void Update()
    {
        HandleAttackState();
        HandleControlledState();
        HandleNotification();
    }

    // =========================================================
    // STATUS
    // =========================================================

    public void SetStatus(RoomStatus newStatus)
    {
        // Room yang sudah dikuasai tidak boleh kembali menjadi Diserang.
        if (currentStatus == RoomStatus.Dikuasai &&
            newStatus == RoomStatus.Diserang)
        {
            return;
        }

        currentStatus = newStatus;

        if (currentStatus == RoomStatus.Diserang)
        {
            captureTimer = captureCooldown;
            hungerTimer = hungerDrainInterval;
        }

        if (currentStatus == RoomStatus.Dikuasai)
        {
            damageTimer = damageInterval;
        }
    }

    public void SetUnderAttack()
    {
        if (currentStatus == RoomStatus.Dikuasai)
            return;

        SetStatus(RoomStatus.Diserang);
        ActivateRandomMinigameTrigger();
    }

    /// <summary>
    /// Dipanggil oleh minigame ketika player berhasil
    /// mengamankan room.
    /// </summary>
    public void ResolveAttack()
    {
        if (currentStatus != RoomStatus.Diserang)
            return;

        SetStatus(RoomStatus.Aman);
        CancelAllMinigameTriggers();
    }

    // =========================================================
    // MINIGAME TRIGGERS
    // =========================================================

    /// <summary>
    /// Memilih salah satu MinigameTrigger di room ini secara acak dan mengaktifkan
    /// warning-nya. Dipanggil setiap kali room mulai diserang.
    /// </summary>
private void ActivateRandomMinigameTrigger()
    {
        if (minigameTriggers == null || minigameTriggers.Length == 0)
            return;

        int index = Random.Range(0, minigameTriggers.Length);
        MinigameTrigger chosen = minigameTriggers[index];

        CurrentActiveTrigger = chosen;

        if (chosen != null)
        {
            chosen.ActivateDanger();
        }
    }

    /// <summary>
    /// Mematikan warning pada semua MinigameTrigger di room ini tanpa memicu minigame.
    /// Dipanggil saat attack sudah diselesaikan atau room di-reset, supaya tidak ada
    /// warning yang nyangkut menyala.
    /// </summary>
private void CancelAllMinigameTriggers()
    {
        CurrentActiveTrigger = null;

        if (minigameTriggers == null)
            return;

        foreach (MinigameTrigger trigger in minigameTriggers)
        {
            if (trigger != null)
            {
                trigger.CancelDanger();
            }
        }
    }

    private void HandleAttackState()
    {
        if (currentStatus != RoomStatus.Diserang)
            return;

        captureTimer -= Time.deltaTime;

        if (captureTimer <= 0f)
        {
            SetStatus(RoomStatus.Dikuasai);
        }

        // Room khusus Hunger.
        if (affectsHungerWhenAttacked)
        {
            hungerTimer -= Time.deltaTime;

            if (hungerTimer <= 0f)
            {
                hungerTimer = hungerDrainInterval;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RemoveHunger(hungerDrainAmount);
                }
            }
        }
    }

    private void HandleControlledState()
    {
        if (currentStatus != RoomStatus.Dikuasai)
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer > 0f)
            return;

        damageTimer = damageInterval;

        if (GameManager.Instance == null)
            return;

        switch (controlledResource)
        {
            case RoomResourceType.Health:
                GameManager.Instance.RemoveHealth(damageAmount);
                break;

            case RoomResourceType.Hunger:
                GameManager.Instance.RemoveHunger(damageAmount);
                break;
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetRoom()
    {
        currentStatus = initialStatus;

        captureTimer = captureCooldown;
        damageTimer = damageInterval;
        hungerTimer = hungerDrainInterval;

        CancelAllMinigameTriggers();
        UpdateNotificationInstant();
    }

    // =========================================================
    // UI
    // =========================================================

    private void HandleNotification()
    {
        if (attackedNotification != null)
        {
            attackedNotification.SetActive(
                currentStatus == RoomStatus.Diserang
            );
        }

        if (controlledNotification != null)
        {
            controlledNotification.SetActive(
                currentStatus == RoomStatus.Dikuasai
            );
        }
    }

    private void UpdateNotificationInstant()
    {
        if (attackedNotification != null)
        {
            attackedNotification.SetActive(
                currentStatus == RoomStatus.Diserang
            );
        }

        if (controlledNotification != null)
        {
            controlledNotification.SetActive(
                currentStatus == RoomStatus.Dikuasai
            );
        }
    }

    public void OpenMiniGameList()
    {
        if (RoomMiniGameUI.Instance == null)
            return;

        RoomMiniGameUI.Instance.OpenForRoom(this);
    }
}