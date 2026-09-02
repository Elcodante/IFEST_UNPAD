using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton Instance agar bisa diakses global
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Daftar SFX (Opsional, bisa diatur lewat Inspector)")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip coinSound;
    public AudioClip hitSound;

    private void Awake()
    {
        // Memastikan hanya ada 1 AudioManager di seluruh game
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Tidak hancur saat ganti scene

        // Ambil atau buat AudioSource otomatis jika belum dipasang
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Setting default AudioSource untuk SFX 2D
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 0f = 2D Sound murni
    }

    /// <summary>
    /// Memutar SFX langsung menggunakan AudioClip.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("AudioClip kosong/null!");
        }
    }

    /// <summary>
    /// Memutar SFX dengan variasi Pitch acak (sangat bagus untuk 2D agar suara tidak monoton).
    /// </summary>
    public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.85f, float maxPitch = 1.15f, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            sfxSource.PlayOneShot(clip, volume);
            sfxSource.pitch = 1f; // Reset pitch
        }
    }
}