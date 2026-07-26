using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 화면 흐름 담당. 메뉴 ↔ 대기방 전환, 시작 시 게임플레이 씬 로드.
/// 게임 시작 이후 흐름(로드 완료 통보, 타이머 시작, 일시정지)도 여기 붙이면 됨.
/// </summary>
public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    [Header("View")]
    [SerializeField] MenuView menuView;
    [SerializeField] LobbyView lobbyView;

    [Header("시작 씬 (모드)")]
    [Tooltip("모드1-방장 / 모드2-일반 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneA = "ceb-network-2d-1";
    [Tooltip("모드1-일반 / 모드2-방장 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneB = "ceb-network-3d-1";

    // StartRequested가 여러 번 감지돼도 씬은 한 번만 로드
    bool _gameplayStarted;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RoomService.Instance.RoomJoined += ShowLobby;
        RoomService.Instance.RoomLeft   += ShowMenu;

        ShowMenu();
    }

    void OnDestroy()
    {
        if (RoomService.Instance == null) return;
        RoomService.Instance.RoomJoined -= ShowLobby;
        RoomService.Instance.RoomLeft   -= ShowMenu;
    }

    void ShowMenu()
    {
        _gameplayStarted = false;
        menuView.SetVisible(true);
        lobbyView.SetVisible(false);
    }

    void ShowLobby()
    {
        menuView.SetVisible(false);
        lobbyView.SetVisible(true);
    }

    /// GameSession이 StartRequested 변화를 감지하면 호출.
    /// 이 클라이언트의 역할(방장/게스트) + 모드로 시작 씬을 정해 Additive 로드.
    public void BeginGameplay(int mode)
    {
        var room = RoomService.Instance;
        if (room == null || !room.IsInRoom) return;
        if (_gameplayStarted) return;
        _gameplayStarted = true;

        bool isHost  = room.IsHost;
        bool isMode1 = mode == 0;
        // 모드1-방장 sceneA 일반 sceneB. 모드2는 반대로
        string scene = (isHost == isMode1) ? sceneA : sceneB;

        Debug.Log($"[Flow] 게임 시작 — mode: {mode}, host: {isHost}, scene: {scene}");

        menuView.SetVisible(false);
        lobbyView.SetVisible(false);
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // TODO(타이머): 씬 로드 완료를 기다렸다가 GameSession에 로드 완료를 알릴 것.
        //               둘 다 보고되면 StartedTick 기록 → 타이머 시작. ARCHITECTURE.md 참고
    }
}
