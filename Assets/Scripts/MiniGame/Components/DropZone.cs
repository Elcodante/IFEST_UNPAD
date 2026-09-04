using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Validation")]
    public string expectedItemID;
    public MinigameDragManager minigameManager;
    public bool hideItemOnDrop = false;

    [Header("Hazard Link")]
    public GearJamHazard myHazard;

    [Header("Juice & Animation")]
    public float rotationSpeed = -150f;

    // --- PENGATURAN AUDIO BARU ---
    [Header("Audio Settings")]
    [Tooltip("ID SFX di AudioManager saat barang ditaruh (misal: SFX_Pasang_Gear atau SFX_Pasang_Kabel)")]
    public string successSoundID = "SFX_Pasang_Gear";

    [Tooltip("ID SFX saat barang terpental/korslet (misal: SFX_Gear_Rusak)")]
    public string errorSoundID = "SFX_Gear_Rusak";

    [Tooltip("AudioClip KHUSUS suara mesin/putaran gerigi ini. (Taruh file mp3/wav nya langsung di sini)")]
    public AudioClip gearSpinClip;
    private AudioSource localSpinSource; // Pemutar suara mandiri untuk gerigi ini

    [HideInInspector]
    public DraggableItem currentItem;

    private void Awake()
    {
        if (myHazard != null)
        {
            myHazard.parentDropZone = this;
        }

        // Bikin pemutar suara lokal secara otomatis khusus untuk putaran gerigi ini
        if (gearSpinClip != null)
        {
            localSpinSource = gameObject.AddComponent<AudioSource>();
            localSpinSource.clip = gearSpinClip;
            localSpinSource.loop = true; // Terus berputar
            localSpinSource.playOnAwake = false;
            localSpinSource.spatialBlend = 0f; // Mode 2D

            // Hubungkan ke Mixer SFX AudioManager agar volumenya tetap bisa diatur Slider!
            if (AudioManager.Instance != null && AudioManager.Instance.sfxMixer != null)
            {
                localSpinSource.outputAudioMixerGroup = AudioManager.Instance.sfxMixer;
            }
        }
    }

    private void Update()
    {
        // Jika gerigi terpasang DAN tidak ada kerikil (mesin normal)
        if (currentItem != null && (myHazard == null || !myHazard.gameObject.activeInHierarchy))
        {
            currentItem.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // 1. AUDIO GEAR BERJALAN: Nyalakan suara putaran lokal jika belum menyala
            if (localSpinSource != null && !localSpinSource.isPlaying)
            {
                localSpinSource.Play();
            }
        }
        else
        {
            // AUDIO GEAR BERJALAN: Matikan suara jika gear dicabut atau sedang macet
            if (localSpinSource != null && localSpinSource.isPlaying)
            {
                localSpinSource.Stop();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null)
            {
                if (draggedItem.itemID == expectedItemID)
                {
                    // 2. AUDIO PASANG GEAR: Panggil SFX sukses dari AudioManager
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFXRandomPitch(successSoundID);

                    draggedItem.transform.SetParent(this.transform, false);
                    draggedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    draggedItem.GetComponent<CanvasGroup>().blocksRaycasts = false;

                    currentItem = draggedItem;

                    if (hideItemOnDrop) draggedItem.gameObject.SetActive(false);

                    if (minigameManager != null)
                    {
                        if (minigameManager is GeneratorGearManager gearManager)
                            gearManager.CheckGeneratorWinCondition();
                        else
                            minigameManager.AddCorrectMatch();
                    }
                }
                else
                {
                    Debug.Log("[DropZone] Barang salah!");
                }
                return;
            }

            // Di dalam OnDrop DropZone.cs...
            WireDragItem wireItem = eventData.pointerDrag.GetComponent<WireDragItem>();
            if (wireItem != null)
            {
                if (wireItem.itemID == expectedItemID)
                {
                    // AUDIO PASANG KABEL SUKSES
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFXRandomPitch(successSoundID);
                        AudioManager.Instance.StopLoopingSFX(); // <--- TAMBAHKAN BARIS INI (Matikan suara setrum)
                    }

                    wireItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    wireItem.enabled = false;

                    // MATIKAN LISTRIK KARENA SUDAH TERSAMBUNG AMAN
                    if (wireItem.sparkEffect != null) wireItem.sparkEffect.SetActive(false);

                    if (minigameManager != null) minigameManager.AddCorrectMatch();
                }
                else
                {
                    // AUDIO ERROR KABEL
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(errorSoundID);
                        AudioManager.Instance.StopLoopingSFX(); // <--- TAMBAHKAN BARIS INI JUGA
                    }

                    if (UIShaker.Instance != null) UIShaker.Instance.Shake(0.3f, 15f);
                }
                return;
            }
        }
    }

    public void EjectItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"[DropZone] Memuntahkan {currentItem.name}!");

            // 3. AUDIO GEAR RUSAK: Panggil SFX error saat terpental
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(errorSoundID);

            if (UIShaker.Instance != null) UIShaker.Instance.Shake(0.4f, 25f);

            currentItem.ReturnToStart();
            currentItem = null;

            if (minigameManager != null)
            {
                if (!(minigameManager is GeneratorGearManager))
                    minigameManager.RemoveCorrectMatch();
                else
                    ((GeneratorGearManager)minigameManager).CheckGeneratorWinCondition();
            }
        }
    }
}