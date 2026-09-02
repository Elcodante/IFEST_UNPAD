using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class PipeNode : MonoBehaviour, IPointerDownHandler
{
    [Header("Pipe Connections")]
    public bool top;
    public bool right;
    public bool left;
    public bool bottom;

    [Header("Visual State")]
    public Image pipeImage;

    [Header("Juice Effects")]
    [Tooltip("Warna saat pipa kosong (misal: Putih/Abu-abu)")]
    public Color emptyColor = Color.white;
    [Tooltip("Warna saat gas/air mengalir (misal: Hijau/Biru Neon)")]
    public Color filledColor = new Color(0.2f, 1f, 0.2f); // Default Hijau Neon
    public float rotationSpeed = 0.15f; // Semakin kecil, semakin cepat putarannya

    [HideInInspector] public PipePuzzleManager manager;

    private int currentRotationIndex = 0;
    private bool isFilled = false;
    private Coroutine rotateRoutine;

    public void InitPipe(PipePuzzleManager puzzleManager)
    {
        manager = puzzleManager;
        if (pipeImage == null)
        {
            pipeImage = GetComponent<Image>();
        }

        // Pastikan mulai dengan warna kosong
        pipeImage.color = emptyColor;
    }

    public void RandomizeRotation()
    {
        currentRotationIndex = Random.Range(0, 4);

        // Rotasi instan HANYA untuk setup awal agar pemain tidak melihat pipa berputar saat game baru mulai
        transform.localRotation = Quaternion.Euler(0, 0, -currentRotationIndex * 90f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        currentRotationIndex = (currentRotationIndex + 1) % 4;

        // JUICE 1: Hentikan putaran sebelumnya jika pemain mengklik brutal (spam click)
        if (rotateRoutine != null) StopCoroutine(rotateRoutine);
        rotateRoutine = StartCoroutine(SmoothRotateAndSquish());

        // Panggil evaluasi logic (Logika mendahului visual, sehingga game terasa sangat responsif)
        if (manager != null)
        {
            manager.EvaluateFlow();
        }
    }

    // JUICE 2: Efek Putaran Fisik (Tactile Twist)
    private IEnumerator SmoothRotateAndSquish()
    {
        float time = 0;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, -currentRotationIndex * 90f);

        Vector3 originalScale = Vector3.one;

        while (time < rotationSpeed)
        {
            time += Time.deltaTime;
            float t = time / rotationSpeed;

            // Putaran halus yang mencari rute terdekat (Shortest Path)
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            // Efek Squish: Skala mengecil ke 0.8 di tengah putaran, lalu membal ke 1.0 di akhir
            float scale = Mathf.Lerp(0.8f, 1.0f, Mathf.Abs(t - 0.5f) * 2f);
            transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        transform.localRotation = endRot;
        transform.localScale = originalScale;
    }

    public bool HasTop() => GetRotateOpenings()[0];
    public bool HasRight() => GetRotateOpenings()[1];
    public bool HasBottom() => GetRotateOpenings()[2];
    public bool HasLeft() => GetRotateOpenings()[3];

    private bool[] GetRotateOpenings()
    {
        bool[] original = new bool[] { top, right, bottom, left };
        bool[] current = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            current[(i + currentRotationIndex) % 4] = original[i];
        }
        return current;
    }

    // JUICE 3: Indikator Aliran Gas/Air
    public void SetFlowState(bool hasGas)
    {
        // Cegah update warna berulang jika statusnya tidak berubah
        if (isFilled == hasGas) return;

        isFilled = hasGas;

        if (pipeImage != null)
        {
            // Ubah tint gambar pipa menjadi menyala saat dialiri gas, dan pudar saat terputus
            pipeImage.color = hasGas ? filledColor : emptyColor;
        }
    }
}