using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class MinigameTrigger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _minigameUI;

    [Header("Minigame Events")]
    public UnityEvent OnMinigameStarted;

    private bool isDangerActive = false;
    public bool IsDangerActive => isDangerActive;

    private void Start()
    {
        if (_minigameUI != null) _minigameUI.SetActive(false);
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectClick();
        }
    }

    public void ActivateDanger()
    {
        if (isDangerActive) return;
        isDangerActive = true;
        if (_minigameUI != null) _minigameUI.SetActive(true);
    }

    public void CancelDanger()
    {
        if (!isDangerActive) return;
        isDangerActive = false;
        if (_minigameUI != null) _minigameUI.SetActive(false);
    }

    private void DetectClick()
    {
        if (!isDangerActive) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 worldPos3D = cam.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 worldPosition = new Vector2(worldPos3D.x, worldPos3D.y);

        // Ambil semua collider yang ada di bawah posisi kursor
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (var hit in hitColliders)
        {
            // Cek apakah collider milik objek ini atau child-nya
            if (hit.gameObject == this.gameObject || hit.transform.IsChildOf(this.transform))
            {
                Debug.Log("Berhasil klik Minigame: " + hit.gameObject.name);
                AcknowledgeWarning();
                return;
            }
        }
    }

    private void AcknowledgeWarning()
    {
        isDangerActive = false;
        if (_minigameUI != null) _minigameUI.SetActive(false);

        if (DayManager.instance != null)
        {
            DayManager.instance.JedaSistemWaktu();
        }

        OnMinigameStarted?.Invoke();
    }
}