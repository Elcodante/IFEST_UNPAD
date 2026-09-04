using UnityEngine;
using UnityEngine.UI; // Dibutuhkan untuk mengontrol Slider

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Pastikan Slider memiliki nilai 0 hingga 1
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // --- Atur agar mulai di tengah (0.5) ---
        float defaultVolume = 0.5f;

        // 1. Atur posisi visual slider
        bgmSlider.value = defaultVolume;
        sfxSlider.value = defaultVolume;

        // 2. Terapkan volume ke AudioManager segera saat Start
        // (Ini memastikan volume di game sesuai dengan visual slider saat menu dibuka)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(defaultVolume);
            AudioManager.Instance.SetSFXVolume(defaultVolume);
        }

        // Tambahkan listener (event) agar fungsi dipanggil saat slider digeser
        bgmSlider.onValueChanged.AddListener(OnBGMInputChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXInputChanged);
    }

    // Fungsi yang dipanggil otomatis oleh Event Slider BGM
    private void OnBGMInputChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
    }

    // Fungsi yang dipanggil otomatis oleh Event Slider SFX
    private void OnSFXInputChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnDestroy()
    {
        // Praktik baik untuk menghapus listener saat objek dihancurkan
        bgmSlider.onValueChanged.RemoveListener(OnBGMInputChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXInputChanged);
    }
}