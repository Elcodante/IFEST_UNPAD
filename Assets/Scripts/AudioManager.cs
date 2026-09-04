using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System; // Dibutuhkan untuk fungsi Array.Find

// 1. Buat struktur data khusus agar rapi di Inspector Unity
[Serializable]
public struct SoundData
{
    public string soundID;  // Contoh: "SFX_Benar", "BGM_Kabel"
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Routing")]
    [SerializeField] private AudioMixer mainMixer; // Referensi ke file Audio Mixer utama
    public AudioMixerGroup bgmMixer;
    public AudioMixerGroup sfxMixer;

    // 2. Array untuk menyimpan seluruh Audio di game
    [Header("Audio Database")]
    public SoundData[] bgmList;
    public SoundData[] sfxList;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource sfxLoopSource;
    private Coroutine crossfadeRoutine;

    // Nama parameter yang kita expose di Mixer editor
    private const string BGM_PARAMS = "BGMVolume";
    private const string SFX_PARAMS = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.outputAudioMixerGroup = bgmMixer;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.outputAudioMixerGroup = sfxMixer;

        sfxLoopSource = gameObject.AddComponent<AudioSource>();
        sfxLoopSource.loop = true; // SANGAT PENTING: Aktifkan mode pengulangan
        sfxLoopSource.spatialBlend = 0f;
        sfxLoopSource.outputAudioMixerGroup = sfxMixer;
    }

    // ==========================================
    // PENGATURAN VOLUME (LOGARITMIK)
    // ==========================================

    // Slider UI biasanya 0 hingga 1 (Linier). Mixer menggunakan Desibel (Logaritmik).
    // Kita butuh fungsi konversi. Middel slider (0.5) akan menjadi sekitar -6dB, bukan -40dB.
    // 0 linier akan menjadi -80dB (senyap).
    private float LinearToDecibel(float linear)
    {
        // Beri batasan agar tidak log(0) yang menghasilkan error infinity
        if (linear <= 0) return -80f;

        // Rumus standar konversi linier ke dB
        return Mathf.Log10(linear) * 20f;
    }

    // Fungsi ini dipanggil oleh Slider UI
    public void SetBGMVolume(float sliderValue)
    {
        if (mainMixer == null)
        {
            Debug.LogError("Main Mixer belum dipasang di AudioManager!");
            return;
        }
        mainMixer.SetFloat(BGM_PARAMS, LinearToDecibel(sliderValue));
    }

    // Fungsi ini dipanggil oleh Slider UI
    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer == null)
        {
            Debug.LogError("Main Mixer belum dipasang di AudioManager!");
            return;
        }
        mainMixer.SetFloat(SFX_PARAMS, LinearToDecibel(sliderValue));
    }


    // ==========================================
    // FUNGSI PENCARI AUDIO BERDASARKAN ID
    // ==========================================
    private AudioClip GetSFXClip(string id)
    {
        SoundData data = Array.Find(sfxList, sound => sound.soundID == id);
        if (data.clip != null) return data.clip;

        Debug.LogWarning($"[AudioManager] SFX dengan ID '{id}' tidak ditemukan!");
        return null;
    }

    private AudioClip GetBGMClip(string id)
    {
        SoundData data = Array.Find(bgmList, sound => sound.soundID == id);
        if (data.clip != null) return data.clip;

        Debug.LogWarning($"[AudioManager] BGM dengan ID '{id}' tidak ditemukan!");
        return null;
    }

    // ==========================================
    // PEMUTAR SFX
    // ==========================================
    public void PlaySFX(string id, float volume = 1f)
    {
        AudioClip clipToPlay = GetSFXClip(id);
        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay, volume);
        }
    }

    public void PlaySFXRandomPitch(string id, float minPitch = 0.85f, float maxPitch = 1.15f, float volume = 1f)
    {
        AudioClip clipToPlay = GetSFXClip(id);
        if (clipToPlay != null)
        {
            sfxSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            sfxSource.PlayOneShot(clipToPlay, volume);
            sfxSource.pitch = 1f;
        }
    }

    // ==========================================
    // PEMUTAR BGM (DENGAN CROSSFADE)
    // ==========================================
    // Catatan: Crossfade mengubah volume pada AudioSource, 
    // sedangkan Slider mengubah volume pada Mixer Group. Keduanya tumpuk menumpuk (stack).
    public void PlayBGM(string id, float fadeDuration = 1.5f)
    {
        AudioClip clipToPlay = GetBGMClip(id);
        if (clipToPlay == null || bgmSource.clip == clipToPlay) return;

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeBGM(clipToPlay, fadeDuration));
    }

    public void PlayLoopingSFX(string id, float volume = 1f)
    {
        // Jangan putar ulang jika suara gosokan yang sama sudah sedang dimainkan
        AudioClip clipToPlay = GetSFXClip(id);
        if (clipToPlay != null && sfxLoopSource.clip != clipToPlay || !sfxLoopSource.isPlaying)
        {
            sfxLoopSource.clip = clipToPlay;
            sfxLoopSource.volume = volume;
            sfxLoopSource.Play();
        }
    }

    // 4. TAMBAHAN: Fungsi untuk mematikan suara gosokan saat kursor dilepas
    public void StopLoopingSFX()
    {
        if (sfxLoopSource.isPlaying)
        {
            sfxLoopSource.Stop();
        }
    }

    private IEnumerator CrossfadeBGM(AudioClip newClip, float duration)
    {
        float currentTime = 0;
        float startVolume = bgmSource.volume;

        if (bgmSource.clip != null)
        {
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
                yield return null;
            }
        }

        bgmSource.clip = newClip;
        bgmSource.Play();
        currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, 1f, currentTime / duration);
            yield return null;
        }
        // Pastikan kembali ke volume maksimal AudioSource (Mixer volume tetap diatur slider)
        bgmSource.volume = 1f;
    }

    public float GetBGMDuration(string id)
    {
        AudioClip clip = GetBGMClip(id);
        if (clip != null) return clip.length;
        return 0f;
    }
}