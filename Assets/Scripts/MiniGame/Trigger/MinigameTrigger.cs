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
    public UnityEvent OnMinigameStarted;

    private bool isDangerActive = false;
    public bool IsDangerActive => isDangerActive;

    private void Start()
    {
        if (_minigameUI != null) _minigameUI.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isDangerActive)
        {
            ActivateDanger();
        }

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

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 worldPos3D = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 worldPosition = new Vector2(worldPos3D.x, worldPos3D.y);

        // PERBAIKAN: Gunakan OverlapPointAll untuk mendeteksi semua yang tertumpuk
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (var hit in hitColliders)
        {
            if (hit.gameObject == this.gameObject)
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