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
    public AudioMixerGroup bgmMixer;
    public AudioMixerGroup sfxMixer;

    // 2. Array untuk menyimpan seluruh Audio di game
    [Header("Audio Database")]
    public SoundData[] bgmList;
    public SoundData[] sfxList;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private Coroutine crossfadeRoutine;

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
    public void PlayBGM(string id, float fadeDuration = 1.5f)
    {
        AudioClip clipToPlay = GetBGMClip(id);
        if (clipToPlay == null || bgmSource.clip == clipToPlay) return;

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeBGM(clipToPlay, fadeDuration));
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
        bgmSource.volume = 1f;
    }
}