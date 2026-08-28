using UnityEngine;

public class GameplaySystemsController : MonoBehaviour
{
    [Header("Gameplay Scripts")]
    [Tooltip("Masukkan semua script yang hanya boleh aktif saat gameplay berjalan.")]
    [SerializeField] private MonoBehaviour[] gameplayScripts;

    private void Awake()
    {
        SetGameplayActive(false);
    }

    public void StartGameplay()
    {
        SetGameplayActive(true);
    }

    public void StopGameplay()
    {
        SetGameplayActive(false);
    }

    private void SetGameplayActive(bool active)
    {
        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script == null)
                continue;

            script.enabled = active;
        }
    }
}