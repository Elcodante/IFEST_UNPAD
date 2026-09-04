using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // DIBUTUHKAN untuk mendeteksi kapan mouse/jari dilepas

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // --- TAMBAHAN BARU ---
    [Header("Audio Feedback")]
    [Tooltip("ID SFX yang akan berbunyi saat slider dilepas untuk ngetes volume (misal: SFX_Klik)")]
    public string testSoundID = "SFX_Klik";
    // ---------------------

    private void Start()
    {
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        float defaultVolume = 0.5f;

        bgmSlider.value = defaultVolume;
        sfxSlider.value = defaultVolume;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(defaultVolume);
            AudioManager.Instance.SetSFXVolume(defaultVolume);
        }

        bgmSlider.onValueChanged.AddListener(OnBGMInputChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXInputChanged);

        // --- TAMBAHAN BARU: Pasang pendeteksi jari dilepas (PointerUp) ke kedua slider ---
        SetupSliderReleaseEvent(sfxSlider);
        SetupSliderReleaseEvent(bgmSlider);
    }

    // Fungsi otomatis untuk menempelkan EventTrigger ke Slider tanpa repot di Inspector
    private void SetupSliderReleaseEvent(Slider slider)
    {
        if (slider == null) return;

        // Ambil komponen EventTrigger. Jika belum ada, buatkan otomatis
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // Buat perintah: "Saat Jari Dilepas (PointerUp), jalankan fungsi PlayTestSound"
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { PlayTestSound(); });

        trigger.triggers.Add(entry);
    }

    private void PlayTestSound()
    {
        // Putar suara dengan nada acak ringan agar tidak membosankan saat slider digeser berkali-kali
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(testSoundID))
        {
            AudioManager.Instance.PlaySFXRandomPitch(testSoundID, 0.95f, 1.05f);
        }
    }

    private void OnBGMInputChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
    }

    private void OnSFXInputChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnDestroy()
    {
        bgmSlider.onValueChanged.RemoveListener(OnBGMInputChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXInputChanged);
    }
}