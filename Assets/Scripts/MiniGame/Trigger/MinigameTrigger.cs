using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
[RequireComponent(typeof(Collider2D))]
public class MinigameTrigger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _minigameUI;

    [Header("Minigame Events")]
    [Tooltip("Event yang akan dipanggil saat ui danger di klik")]
    public UnityEvent OnMinigameStarted;

    private bool isDangerActive = false;

    /// <summary>
    /// Status saat ini apakah trigger ini sedang menampilkan warning (belum diklik player).
    /// </summary>
    public bool IsDangerActive => isDangerActive;

    private void Start()
    {
        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }
    }

    private void Update()
    {
    //Catatan: Code dibawah ini untuk testing
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isDangerActive)
        {
            ActivateDanger();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectClick();
        }
    }
    /// <summary>
    /// Fungsi ini bisa dipanggil oleh sistem ruangan saat mesin tiba-tiba rusak.
    /// </summary>

    public void ActivateDanger()
    {
        if (isDangerActive) return;

        isDangerActive = true;

        if (_minigameUI != null)
        {
            _minigameUI.SetActive(true);
        }

        Debug.Log("Danger activated! Minigame UI is now active.");
    }

    /// <summary>
    /// Membatalkan warning tanpa memicu minigame. Dipanggil misalnya oleh RoomController
    /// saat room di-reset atau attack diselesaikan lewat trigger lain di room yang sama,
    /// supaya warning yang belum sempat diklik player tidak nyangkut menyala.
    /// </summary>
    public void CancelDanger()
    {
        if (!isDangerActive) return;

        isDangerActive = false;

        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }
    }

    /// <summary>
    /// Logika deteksi klik kursor pada dunia 2D
    /// </summary>

    private void DetectClick()
    {

        if (!isDangerActive) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);

        if (hitCollider != null && hitCollider.gameObject == this.gameObject)
        {
            AcknowledgeWarning();
        }
    }

    /// <summary>
    /// Memproses logika setelah pemain mengklik objek peringatan.
    /// </summary>

    private void AcknowledgeWarning()
    {
        isDangerActive = false;

        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }

        Debug.Log($"[Danger UI] Dihilangkan. Memulai Minigame di: {gameObject.name}");

        OnMinigameStarted?.Invoke();
    }
}