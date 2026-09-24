using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>화면 종류. 한 번에 하나만 켜짐.</summary>
public enum ScreenId
{
    Title,
    Setting,
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

    [Header("스테이지 1 씬 (모드)")]
    [Tooltip("모드1-방장 / 모드2-일반 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneA = "2D";
    [Tooltip("모드1-일반 / 모드2-방장 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneB = "3D";

    [Header("스테이지 2 씬")]
    [Tooltip("스테이지 1에서 sceneA(2D)를 받은 사람이 로드.")]
    [SerializeField] string stage2A = "3D_2stage";
    [Tooltip("스테이지 1에서 sceneB(3D)를 받은 사람이 로드.")]
    [SerializeField] string stage2B = "2D_2stage";

    [Header("로비 카메라")]
    [Tooltip("게임플레이 씬 로드 후 끌 카메라. 비우면 시작 시점의 Camera.main")]
    [SerializeField] Camera lobbyCamera;

    public ScreenId Current { get; private set; } = ScreenId.Title;

    // Phase가 여러 번 감지돼도 씬은 한 번만 로드
    bool _gameplayStarted;

    // 지금 올라가 있는 씬의 스테이지. 0이면 아직 없음
    int _loadedStage;

    // View의 ScreenId 넣음
    ScreenView[] _screens;

    string _loadedGamePlayScene;

    // 로비 카메라의 원래 태그. 로비로 돌아올 때 되돌림
    string _lobbyCameraTag;

    void Awake()
    {
        Instance = this;

        // 게임플레이 씬이 아직 없는 지금이라 MainCamera 태그가 하나뿐 — 여기서 잡아야 확실함
        if (lobbyCamera == null) lobbyCamera = Camera.main;
        if (lobbyCamera != null) _lobbyCameraTag = lobbyCamera.gameObject.tag;

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

    #region RoomService Event

    void OnRoomJoined()
    {
        _gameplayStarted = false;
        _loadedStage = 0;
        Show(ScreenId.Lobby);
    }

    void OnRoomLeft()
    {
        bool wasPlaying = _gameplayStarted;
        _gameplayStarted = false;
        _loadedStage = 0;

        if(wasPlaying && _loadedGamePlayScene != null)
        {
            // 전환 도중이면 아직 안 올라온 씬 이름일 수 있음
            if (SceneManager.GetSceneByName(_loadedGamePlayScene).isLoaded)
                SceneManager.UnloadSceneAsync(_loadedGamePlayScene);

            _loadedGamePlayScene = null;
        }
        if (wasPlaying)
        {
            RestoreLobbyCamera();
            SoundManager.Instance?.PlayBGM(BGMType.StartBGM);
        }

        Show(wasPlaying ? ScreenId.Title : ScreenId.Menu);
    }

    #endregion

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
                _loadedStage = 0;
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
        if (_gameplayStarted) return;
        _gameplayStarted = true;

        // Stage가 아직 0인 스냅샷으로 읽힐 수 있음
        int stage = GameSession.Instance != null ? Mathf.Max(1, GameSession.Instance.Stage) : 1;

        Show(ScreenId.None);
        LoadStage(stage, mode, replaceCurrent: false);
    }

    // ---------- Stage ----------

    /// <summary>GameSession의 Stage가 오르는 순간 호출. 지금 씬을 내리고 다음 스테이지 씬 올림.</summary>
    public void BeginStage(int stage)
    {
        if (!_gameplayStarted) return;      // 첫 씬은 Phase 쪽이 올림
        if (_loadedStage == stage) return;  // 같은 값이 다시 감지됨

        int mode = GameSession.Instance != null ? GameSession.Instance.Mode : 0;

        LoadStage(stage, mode, replaceCurrent: true);
    }

    /// 두 진입점이 공유하는 실제 로드. 언제 올릴지는 호출자가 정함
    /// <param name="replaceCurrent">지금 올라가 있는 씬을 내리고 올릴지. 첫 로드면 false</param>
    void LoadStage(int stage, int mode, bool replaceCurrent)
    {
        var room = RoomService.Instance;
        if (room == null || !room.IsInRoom) return;

        string previous = replaceCurrent ? _loadedGamePlayScene : null;

        _loadedStage = stage;
        _loadedGamePlayScene = SceneFor(stage, room.IsHost, mode);

        Debug.Log($"[Flow] {(replaceCurrent ? "스테이지 전환" : "게임 시작")} — " +
                  $"mode: {mode}, host: {room.IsHost}, stage: {stage}, " +
                  $"scene: {(previous != null ? previous + " → " : "")}{_loadedGamePlayScene}");

        StartCoroutine(LoadAndReport(room.IsHost, previous));
    }

    /// 이 클라이언트가 이번 스테이지에 받을 씬 이름.
    string SceneFor(int stage, bool isHost, int mode)
    {
        bool isMode1 = mode == 0;

        // 모드1-방장이 A, 일반이 B. 모드2는 반대로
        bool takesA = isHost == isMode1;

        if (stage >= 2) return takesA ? stage2A : stage2B;
        return takesA ? sceneA : sceneB;
    }

    /// 로드 보고 전에 흘려보낼 프레임 수. 인트로 첫 렌더의 셰이더 · 렌더 그래프 컴파일용
    const int WarmupFrames = 3;

    // 씬 로딩 후 보고하기 위한 코루틴.
    // previousScene 은 반드시 로드보다 먼저 내릴 것 — 나중에 내리면 블링크가 안 보임
    IEnumerator LoadAndReport(bool isHost, string previousScene)
    {
        if (!string.IsNullOrEmpty(previousScene) &&
            SceneManager.GetSceneByName(previousScene).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(previousScene);
        }

        // 로드 전에 떼야 함 — 로드된 씬의 Awake 가 그 시점에 이미 Camera.main 을 캐시함
        ReleaseLobbyCameraTag();

        var op = SceneManager.LoadSceneAsync(_loadedGamePlayScene, LoadSceneMode.Additive);
        yield return op;

        // 로드 도중에 방을 나갔으면 이미 로비로 돌아간 상태
        if (_gameplayStarted) DisableLobbyCamera();

        ReleaseCursor();

        // 첫 프레임 스톨을 보고 전에 흡수. StartedTick 이 양쪽 보고 뒤에 잡히므로
        // 여기서 멈추는 만큼은 인트로 연출에서 안 깎임
        for (int i = 0; i < WarmupFrames; i++)
            yield return null;

        Debug.Log($"[Flow] 로드 완료 보고 | host:{isHost}");
        GameSession.Instance?.RpcReportLoaded(isHost);
    }

    /// 3D 씬은 커서가 잠긴 채로 내려감. 2D 로 넘어가면 풀어줄 주체가 없어 클릭이 죽음.
    /// 새 씬이 3D 면 PlayerController 가 다음 프레임에 다시 잠금
    static void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// 게임플레이 씬은 Additive 로드라 로비 씬이 그대로 남는다.
    /// MainCamera 태그가 둘이 되면 퍼즐들의 Camera.main 폴백이 로비 카메라를 잡아 확대 · 상호작용이 죽음.
    /// 로딩 중 화면이 까매지지 않게 렌더링은 그대로 두고 태그만 뗌
    void ReleaseLobbyCameraTag()
    {
        if (lobbyCamera != null) lobbyCamera.gameObject.tag = "Untagged";
    }

    /// 로드가 끝나면 렌더링도 멈춤. 중복 AudioListener 경고도 같이 사라짐
    void DisableLobbyCamera()
    {
        if (lobbyCamera != null) lobbyCamera.gameObject.SetActive(false);
    }

    /// 로비로 돌아올 때 원상복구
    void RestoreLobbyCamera()
    {
        if (lobbyCamera == null) return;

        lobbyCamera.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(_lobbyCameraTag)) lobbyCamera.gameObject.tag = _lobbyCameraTag;
    }
}
