using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>화면 종류. 한 번에 하나만 켜짐.</summary>
public enum ScreenId
{
    Title,
    Menu,
    FindRoom,
    Lobby,
    ModeSelect,   // 방장만 봄
    Waiting,      // 게스트만 봄
    None,         // 게임플레이 중 — UI 전부 꺼짐
}

/// <summary>
/// 화면 흐름 담당. 어떤 화면을 켤지는 전부 여기서 결정한다.
/// View는 자기 위젯만 알고, 화면 전환은 GameFlow.Show()를 부른다.
/// </summary>
public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    [Header("시작 씬 (모드)")]
    [Tooltip("모드1-방장 / 모드2-일반 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneA = "ceb-network-2d-1";
    [Tooltip("모드1-일반 / 모드2-방장 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneB = "ceb-network-3d-1";

    public ScreenId Current { get; private set; } = ScreenId.Title;

    // Phase가 여러 번 감지돼도 씬은 한 번만 로드
    bool _gameplayStarted;

    // View의 ScreenId 넣음
    ScreenView[] _screens;

    void Awake()
    {
        Instance = this;

        _screens = GetComponents<ScreenView>();
        if (_screens.Length == 0)
            Debug.LogError("[Flow] View가 하나도 없음 — GameFlow와 같은 오브젝트에 붙일 것");
    }

    void Start()
    {
        RoomService.Instance.RoomJoined += OnRoomJoined;
        RoomService.Instance.RoomLeft   += OnRoomLeft;

        Show(ScreenId.Title);
    }

    void OnDestroy()
    {
        if (RoomService.Instance == null) return;
        RoomService.Instance.RoomJoined -= OnRoomJoined;
        RoomService.Instance.RoomLeft   -= OnRoomLeft;
    }

    // ---------- 화면 전환 ----------

    public void Show(ScreenId id)
    {
        Current = id;

        foreach (var screen in _screens)
            screen.SetVisible(screen.Id == id);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- RoomService 이벤트 ----------

    void OnRoomJoined()
    {
        _gameplayStarted = false;
        Show(ScreenId.Lobby);
    }

    void OnRoomLeft()
    {
        _gameplayStarted = false;
        Show(ScreenId.Menu);
    }

    // ---------- Phase ----------

    /// <summary>
    /// GameSession의 Phase가 바뀔 때마다 호출. 방장/게스트가 여기서 갈라진다.
    /// </summary>
    public void ApplyPhase(RoomPhase phase)
    {
        var room = RoomService.Instance;
        if (room == null || !room.IsInRoom) return;

        switch (phase)
        {
            case RoomPhase.Lobby:
                _gameplayStarted = false;
                Show(ScreenId.Lobby);
                break;

            case RoomPhase.ModeSelect:
                Show(room.IsHost ? ScreenId.ModeSelect : ScreenId.Waiting);
                break;

            case RoomPhase.Playing:
                BeginGameplay(GameSession.Instance != null ? GameSession.Instance.Mode : 0);
                break;
        }
    }

    /// 이 클라이언트의 역할(방장/게스트) + 모드로 시작 씬을 정해 Additive 로드.
    void BeginGameplay(int mode)
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

        Show(ScreenId.None);
        StartCoroutine(LoadAndReport(scene, isHost));
    }

    // 씬 로딩 후 보고하기 위한 코루틴
    IEnumerator LoadAndReport(string scene, bool isHost)
    {
        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        yield return op;
        Debug.Log($"[Flow] 로드 완료 보고 | host:{isHost}");
        GameSession.Instance?.RpcReportLoaded(isHost);
    }
}
