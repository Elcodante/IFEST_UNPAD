using UnityEngine;
using UnityEngine.UI;

public class RoomMiniGameUI : MonoBehaviour
{
    public static RoomMiniGameUI Instance { get; private set; }

    [Header("Main UI")]
    [SerializeField] private GameObject miniGameListPanel;

    [Header("Mini Games")]
    [SerializeField] private MiniGameDefinition[] miniGames;

    [Header("UI")]
    [SerializeField] private Transform miniGameButtonContainer;
    [SerializeField] private Button miniGameButtonPrefab;

    private RoomController selectedRoom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CloseAllPanels();
        GenerateMiniGameButtons();
    }

    public void OpenForRoom(RoomController room)
    {
        if (room == null)
            return;

        // Hanya room yang sedang diserang yang dapat
        // membuka minigame.
        if (room.CurrentStatus != RoomStatus.Diserang)
            return;

        selectedRoom = room;

        CloseAllMiniGames();

        miniGameListPanel.SetActive(true);
    }

    public void CloseMiniGameList()
    {
        selectedRoom = null;

        CloseAllMiniGames();

        miniGameListPanel.SetActive(false);
    }

    private void GenerateMiniGameButtons()
    {
        if (miniGameButtonContainer == null ||
            miniGameButtonPrefab == null)
        {
            return;
        }

        foreach (MiniGameDefinition miniGame in miniGames)
        {
            if (miniGame == null)
                continue;

            Button button = Instantiate(
                miniGameButtonPrefab,
                miniGameButtonContainer
            );

            button.name = $"Button_{miniGame.miniGameName}";

            // Mengubah text button.
            TMPro.TMP_Text text = button.GetComponentInChildren<TMPro.TMP_Text>();

            if (text != null)
            {
                text.text = miniGame.miniGameName;
            }

            MiniGameDefinition capturedGame = miniGame;

            button.onClick.AddListener(() =>
            {
                OpenMiniGame(capturedGame);
            });
        }
    }

    private void OpenMiniGame(MiniGameDefinition miniGame)
    {
        if (miniGame == null)
            return;

        CloseAllMiniGames();

        if (miniGame.miniGamePanel != null)
        {
            miniGame.miniGamePanel.SetActive(true);
        }
    }

    public void MiniGameCompleted()
    {
        if (selectedRoom == null)
            return;

        // Mengubah Diserang -> Aman
        selectedRoom.ResolveAttack();

        CloseMiniGameList();
    }

    private void CloseAllMiniGames()
    {
        foreach (MiniGameDefinition miniGame in miniGames)
        {
            if (miniGame != null &&
                miniGame.miniGamePanel != null)
            {
                miniGame.miniGamePanel.SetActive(false);
            }
        }
    }

    private void CloseAllPanels()
    {
        if (miniGameListPanel != null)
        {
            miniGameListPanel.SetActive(false);
        }

        CloseAllMiniGames();
    }
}