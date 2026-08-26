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

    [Header("시작 씬 (모드)")]
    [Tooltip("모드1-방장 / 모드2-일반 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneA = "ceb_2D";
    [Tooltip("모드1-일반 / 모드2-방장 이 로드. Build Settings에 등록되어 있어야 함.")]
    [SerializeField] string sceneB = "ceb_3D";

    [Header("로비 카메라")]
    [Tooltip("게임플레이 씬 로드 후 끌 카메라. 비우면 시작 시점의 Camera.main")]
    [SerializeField] Camera lobbyCamera;

    public ScreenId Current { get; private set; } = ScreenId.Title;

    // Phase가 여러 번 감지돼도 씬은 한 번만 로드
    bool _gameplayStarted;

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
        Show(ScreenId.Lobby);
    }

    void OnRoomLeft()
    {
        bool wasPlaying = _gameplayStarted;
        _gameplayStarted = false;

        if(wasPlaying && _loadedGamePlayScene != null)
        {
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
        _loadedGamePlayScene = (isHost == isMode1) ? sceneA : sceneB;

        Debug.Log($"[Flow] 게임 시작 — mode: {mode}, host: {isHost}, scene: {_loadedGamePlayScene}");

        Show(ScreenId.None);
        StartCoroutine(LoadAndReport(isHost));
    }

    /// 로드 보고 전에 흘려보낼 프레임 수. 인트로 첫 렌더의 셰이더 · 렌더 그래프 컴파일용
    const int WarmupFrames = 3;

    // 씬 로딩 후 보고하기 위한 코루틴
    IEnumerator LoadAndReport(bool isHost)
    {
        // 로드 전에 떼야 함 — 로드된 씬의 Awake 가 그 시점에 이미 Camera.main 을 캐시함
        ReleaseLobbyCameraTag();

        var op = SceneManager.LoadSceneAsync(_loadedGamePlayScene, LoadSceneMode.Additive);
        yield return op;

        // 로드 도중에 방을 나갔으면 이미 로비로 돌아간 상태
        if (_gameplayStarted) DisableLobbyCamera();

        // 첫 프레임 스톨을 보고 전에 흡수. StartedTick 이 양쪽 보고 뒤에 잡히므로
        // 여기서 멈추는 만큼은 인트로 연출에서 안 깎임
        for (int i = 0; i < WarmupFrames; i++)
            yield return null;

        Debug.Log($"[Flow] 로드 완료 보고 | host:{isHost}");
        GameSession.Instance?.RpcReportLoaded(isHost);
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
