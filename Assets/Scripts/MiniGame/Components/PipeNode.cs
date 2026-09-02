using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PipeNode : MonoBehaviour, IPointerDownHandler
{
    [Header("Pipe Connections")]
    public bool top;
    public bool right;
    public bool left;
    public bool bottom;

    [Header("Visual State")]
    public Image pipeImage;
  
    [HideInInspector] public PipePuzzleManager manager;
    private int currentRotationIndex = 0;

    public void InitPipe(PipePuzzleManager puzzleManager)
    {
        manager = puzzleManager;
        if (pipeImage == null)
        {
            pipeImage = GetComponent<Image>();
        }
    }

    public void RandomizeRotation()
    {
        currentRotationIndex = Random.Range(0, 4);
        ApplyRotation();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        currentRotationIndex = (currentRotationIndex + 1) % 4;
        ApplyRotation();

        if (manager != null)
        {
            manager.EvaluateFlow();
        }
    }

    private void ApplyRotation()
    {
        transform.localEulerAngles = new Vector3(0, 0, -currentRotationIndex * 90f);
    }

    public bool HasTop() => GetRotateOpenings()[0];
    public bool HasRight() => GetRotateOpenings()[1];
    public bool HasBottom() => GetRotateOpenings()[2];
    public bool HasLeft() => GetRotateOpenings()[3];

    private bool[] GetRotateOpenings()
    {
        bool[] original = new bool[] { top,  right, bottom, left };
        bool[] current = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            current[(i + currentRotationIndex) % 4] = original[i];
        } 
        return current;
    }

    public void SetFlowState(bool hasGas)
    {
        
    }
 }
