using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Bootstrap 씬 하나로 메뉴 → 대기방 → 시작(모드별 씬 로드)을 처리한다.
/// 씬 전환이 아니라 Canvas 패널 교체 방식이며, NetworkRunner는 파괴하지 않는다.
///
/// 이번 마일스톤: 방 만들기 / 들어가기 / 나가기 / 모드 설정 / 시작 시 역할·모드별 씬 로드.
/// </summary>
public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static RoomManager Instance { get; private set; }

    [Header("Runner / Prefab")]
    [Tooltip("씬에 미리 놓아둔 NetworkRunner. 비워두면 런타임에 생성.")]
    [SerializeField] NetworkRunner sceneRunner;
    [Tooltip("NetworkProjectConfig에 등록된 GameSession 프리팹")]
    [SerializeField] NetworkPrefabRef gameSessionPrefab;

    [Header("Panels")]
    [SerializeField] GameObject canvasMenu;
    [SerializeField] GameObject canvasLobby;

    [Header("Menu UI")]
    [SerializeField] Button btnMakeRoom;
    [Tooltip("누르면 FindRoomUI를 연다. 이미 열려 있으면 입력한 코드로 입장 시도.")]
    [SerializeField] Button btnEnterRoom;
    [Tooltip("코드 입력창을 묶은 오브젝트. 평소엔 꺼둔다.")]
    [SerializeField] GameObject findRoomUI;
    [SerializeField] TMP_InputField inputCode;
    [Tooltip("평소엔 꺼져 있다가 메시지가 생기면 잠깐 켜진다.")]
    [SerializeField] TMP_Text menuStatus;
    [Tooltip("menuStatus가 켜져 있는 시간(초).")]
    [SerializeField] float menuStatusDuration = 4f;

    [Header("Lobby UI")]
    [SerializeField] TMP_Text textRoomId;
    [Tooltip("Player2 슬롯의 사람 아이콘. 상대가 들어오면 켜진다. Player1(방장)은 항상 켜둔다.")]
    [SerializeField] GameObject player2Icon;
    [SerializeField] Button btnMode1;           // 방장만 조작
    [SerializeField] Button btnMode2;           // 방장만 조작
    [SerializeField] Button btnStart;           // 방장만 조작
    [SerializeField] Button btnLeave;
    [Tooltip("입장/퇴장 등 로컬 로그를 누적 표시. 없으면 콘솔에만 남는다.")]
    [SerializeField] TMP_Text lobbyLog;

    [Header("모드 버튼 색")]
    [SerializeField] Color modeSelected = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color modeDeselected = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("테스트")]
    [Tooltip("비워두면 방을 만들 때마다 코드를 새로 뽑는다. 값을 넣으면 항상 그 코드로 방을 만든다. " +
             "로컬 2인 테스트에서 코드를 옮겨 적는 수고를 덜기 위한 것이므로 배포 전엔 비워둘 것.")]
    [SerializeField] string debugFixedCode = "";

    [Header("시작 씬 (모드)")]
    [Tooltip("모드1-방장 / 모드2-일반 이 로드하는 씬")]
    [SerializeField] string sceneA = "2d-1";
    [Tooltip("모드1-일반 / 모드2-방장 이 로드하는 씬")]
    [SerializeField] string sceneB = "3d-1";

    // 방 코드 생성용 문자 (헷갈리는 0/O/1/I 제외)
    const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    const int CodeLength = 4;

    // 2인 전용. 방을 만든 피어의 값이 세션에 적용되고, 초과 입장은 Photon이 거절한다.
    const int MaxPlayers = 2;

    NetworkRunner _runner;
    bool _connecting;

    // 모드 버튼의 Image. 하이라이트 색을 매 프레임 칠하므로 Start에서 한 번만 캐싱.
    Image _modeImage1;
    Image _modeImage2;

    // menuStatus를 다시 끄기 위한 타이머. 메시지가 새로 오면 이전 타이머를 취소한다.
    Coroutine _menuStatusRoutine;

    // 로비 로그. 최대 2인이라 길어질 일이 없지만 무한 누적은 막는다.
    const int MaxLogLines = 6;
    readonly List<string> _logLines = new List<string>();

    // 마지막으로 확인된 방장 PlayerRef. Shared Mode엔 방장 PlayerRef API가 없어
    // GameSession의 StateAuthority(=방장)를 매 프레임 캐싱해 이탈 판별에 쓴다.
    PlayerRef _hostRef;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        btnMakeRoom.onClick.AddListener(OnClickMakeRoom);
        btnEnterRoom.onClick.AddListener(OnClickFindRoom);
        // 입력창에서 Enter를 쳐도 입장되게. (버튼을 또 누르러 갈 필요 없음)
        inputCode.onSubmit.AddListener(_ => TryEnterRoom());
        btnMode1.onClick.AddListener(() => OnClickSelectMode(0));
        btnMode2.onClick.AddListener(() => OnClickSelectMode(1));
        btnStart.onClick.AddListener(OnClickStart);
        btnLeave.onClick.AddListener(OnClickLeave);

        // Button의 기본 ColorTint 전환은 마우스가 닿을 때마다 Image.color를 normalColor로
        // 되돌려버린다. 선택 상태를 색으로 표현해야 하므로 전환을 끄고 우리가 직접 칠한다.
        _modeImage1 = SetupModeButton(btnMode1);
        _modeImage2 = SetupModeButton(btnMode2);

        SetMenuStatus("");   // 시작 시엔 꺼둔 상태
        ShowMenu();
    }

    // ---------- 메뉴 ----------

    void OnClickMakeRoom()
    {
        if (_connecting) return;
        ShowFindRoomUI(false);   // 방을 새로 만드는 흐름이니 코드 입력창은 닫는다

        string code = string.IsNullOrWhiteSpace(debugFixedCode)
            ? GenerateCode()
            : debugFixedCode.Trim().ToUpperInvariant();
        _ = StartRoomAsync(code);
    }

    // 첫 클릭은 입력창 열기, 열려 있는 상태에서의 클릭은 입장 시도.
    void OnClickFindRoom()
    {
        if (_connecting) return;

        if (!findRoomUI.activeSelf)
        {
            ShowFindRoomUI(true);
            return;
        }

        TryEnterRoom();
    }

    void TryEnterRoom()
    {
        if (_connecting) return;

        string code = inputCode.text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
        {
            SetMenuStatus("코드를 입력하세요");
            return;
        }
        _ = StartRoomAsync(code);
    }

    // 열 때는 바로 타이핑할 수 있게 입력창에 포커스를 준다.
    void ShowFindRoomUI(bool on)
    {
        findRoomUI.SetActive(on);
        if (on)
        {
            inputCode.Select();
            inputCode.ActivateInputField();
        }
    }

    async Task StartRoomAsync(string code)
    {
        _connecting = true;
        SetMenuButtons(false);
        // 얼마나 걸릴지 모르므로 자동으로 끄지 않는다. 성공/실패 시 다음 메시지가 덮는다.
        SetMenuStatus($"접속 중... ({code})", autoHide: false);

        // 이 메서드는 `_ = StartRoomAsync(...)` 로 던져놓고 기다리지 않는다.
        // try/catch가 없으면 여기서 난 예외는 아무 데도 안 찍히고 UI만 "접속 중"에 멈춘다.
        try
        {
            _runner = GetOrCreateRunner();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            Debug.Log($"[Room] StartGame 호출: {code}");

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode    = GameMode.Shared,
                SessionName = code,
                PlayerCount = MaxPlayers,   // NetworkProjectConfig 값을 덮어쓴다
                // 씬은 우리가 직접 Additive로 관리하므로 여기서 지정하지 않는다.
            });

            sw.Stop();

            // await 도중 OnShutdown이 돌면 ShowMenu()가 _runner를 null로 만든다.
            // 그 상태로 아래를 진행하면 NullReference가 나고, 던져놓은 Task라 조용히 묻힌다.
            if (_runner == null)
            {
                Debug.LogWarning($"[Room] StartGame 반환 전에 러너가 정리됨 ({sw.ElapsedMilliseconds}ms)");
                SetMenuStatus("접속이 중단되었습니다");
                SetMenuButtons(true);
                return;
            }

            Debug.Log($"[Room] StartGame {(result.Ok ? "성공" : "실패")} — {sw.ElapsedMilliseconds}ms, " +
                      $"region: {(result.Ok ? _runner.SessionInfo.Region : "-")}, " +
                      $"reason: {result.ShutdownReason}");

            if (!result.Ok)
            {
                // 3번째 사람이 코드를 알고 들어오려 한 경우가 가장 흔하다.
                SetMenuStatus(result.ShutdownReason == ShutdownReason.GameIsFull
                    ? "방이 가득 찼습니다"
                    : $"접속 실패: {result.ShutdownReason}");
                SetMenuButtons(true);
                return;
            }

            // Shared Mode에서 세션을 처음 만든 피어가 방장이 된다. 방장만 GameSession을 Spawn.
            // (씬에 미리 두면 각 클라가 중복 생성할 위험이 있어 입장 후 Spawn 방식을 쓴다.)
            if (_runner.IsSharedModeMasterClient)
                _runner.Spawn(gameSessionPrefab);

            // 성공했으니 "접속 중"을 끈다. 안 끄면 나중에 메뉴로 돌아왔을 때 남아 있다.
            SetMenuStatus("");
            ShowLobby();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetMenuStatus("접속 오류 (콘솔 확인)");
            SetMenuButtons(true);
        }
        finally
        {
            _connecting = false;
        }
    }

    // ---------- 대기방 ----------

    // 버튼 두 개 중 하나를 고르는 방식. 토글이 아니라 값을 직접 지정한다.
    // 이미 선택된 걸 다시 눌러도 같은 값이라 네트워크 변화가 없다.
    void OnClickSelectMode(int mode)
    {
        GameSession.Instance?.SetMode(mode);
    }

    void OnClickStart()
    {
        GameSession.Instance?.RequestStart();
    }

    async void OnClickLeave()
    {
        await LeaveRoomAsync();
    }

    async Task LeaveRoomAsync()
    {
        if (_runner != null)
            await _runner.Shutdown();   // 완료되면 OnShutdown 콜백에서 메뉴로 복귀
    }

    /// GameSession이 StartRequested 변화를 감지하면 호출된다.
    /// 이 클라이언트의 역할 + 모드로 시작 씬을 정해 Additive 로드한다.
    public void BeginGameplay(int mode)
    {
        if (_runner == null) return;

        bool isHost  = _runner.IsSharedModeMasterClient;
        bool isMode1 = mode == 0;
        // 모드1-방장 == 모드2-일반 == sceneA / 그 반대는 sceneB
        string scene = (isHost == isMode1) ? sceneA : sceneB;

        canvasMenu.SetActive(false);
        canvasLobby.SetActive(false);
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }

    // ---------- UI 갱신 (프로토타입: 매 프레임 폴링) ----------

    void Update()
    {
        if (_runner == null || !_runner.IsRunning || !canvasLobby.activeSelf)
            return;

        textRoomId.text = _runner.SessionInfo.Name;

        int count = 0;
        foreach (var _ in _runner.ActivePlayers)
            count++;

        // Player1(방장) 슬롯은 씬에서 켜둔 채 고정. 방이 있다는 건 방장이 있다는 뜻이다.
        // 상대가 들어왔을 때 Player2 아이콘만 켠다.
        player2Icon.SetActive(count >= 2);

        bool isHost = _runner.IsSharedModeMasterClient;

        var gs = GameSession.Instance;

        // 방장 판별용: GameSession의 StateAuthority가 방장이다. 매 프레임 갱신해 두면
        // 방장이 나간 순간 OnPlayerLeft에서 직전 값(=나간 방장)과 비교할 수 있다.
        if (gs != null && gs.Object != null)
            _hostRef = gs.Object.StateAuthority;

        // 선택된 모드는 게스트도 봐야 하므로 버튼을 숨기지 않고 조작만 막는다.
        // gs가 아직 Spawn 전이면 기본값 0(모드1)으로 그려둔다.
        ApplyModeVisual(gs != null ? gs.Mode : 0);

        // 모드 변경 / 시작은 방장만. 시작은 2명 다 있고 GameSession 준비됐을 때만.
        btnStart.gameObject.SetActive(isHost);
        btnMode1.interactable = isHost && gs != null;
        btnMode2.interactable = isHost && gs != null;
        btnStart.interactable = isHost && gs != null && count >= 2;
    }

    // ---------- 패널 전환 ----------

    void ShowMenu()
    {
        _runner = null;
        canvasMenu.SetActive(true);
        canvasLobby.SetActive(false);
        SetMenuButtons(true);
        ShowFindRoomUI(false);   // 메뉴로 돌아올 땐 항상 접힌 상태에서 시작
        // 여기서 상태 문구를 지우면 안 된다. "방장이 나갔습니다" 같은 메시지가
        // Shutdown → ShowMenu 순서로 곧바로 덮이기 때문. 다음 접속 시도에서 갱신된다.
    }

    void ShowLobby()
    {
        canvasMenu.SetActive(false);
        canvasLobby.SetActive(true);
        _logLines.Clear();
        if (lobbyLog != null)
            lobbyLog.text = "";
    }

    // 로컬 로그 한 줄 추가. 네트워크로 쏘지 않고 각 클라가 콜백 받아 스스로 만든다.
    void AppendLog(string line)
    {
        Debug.Log($"[Room] {line}");

        _logLines.Add(line);
        if (_logLines.Count > MaxLogLines)
            _logLines.RemoveAt(0);

        if (lobbyLog != null)
            lobbyLog.text = string.Join("\n", _logLines);
    }

    // menuStatus는 평소 꺼져 있다가 메시지가 오면 menuStatusDuration 동안만 켜진다.
    // autoHide: false면 다음 메시지가 올 때까지 계속 떠 있다(접속 중처럼 끝을 모르는 경우).
    // 참조가 비어 있어도(씬에 아직 안 만들었어도) 죽지 않는다.
    void SetMenuStatus(string message, bool autoHide = true)
    {
        if (menuStatus == null)
            return;

        // 4초가 지나기 전에 다음 메시지가 오면 타이머를 다시 시작한다.
        if (_menuStatusRoutine != null)
        {
            StopCoroutine(_menuStatusRoutine);
            _menuStatusRoutine = null;
        }

        if (string.IsNullOrEmpty(message))
        {
            menuStatus.gameObject.SetActive(false);
            return;
        }

        menuStatus.text = message;
        menuStatus.gameObject.SetActive(true);

        if (autoHide)
            _menuStatusRoutine = StartCoroutine(HideMenuStatusAfterDelay());
    }

    IEnumerator HideMenuStatusAfterDelay()
    {
        yield return new WaitForSeconds(menuStatusDuration);
        menuStatus.gameObject.SetActive(false);
        _menuStatusRoutine = null;
    }

    void SetMenuButtons(bool on)
    {
        btnMakeRoom.interactable = on;
        btnEnterRoom.interactable = on;
    }

    // 모드 버튼 하나를 준비한다. ColorTint를 끄고 Image를 돌려준다.
    static Image SetupModeButton(Button button)
    {
        button.transition = Selectable.Transition.None;
        return button.GetComponent<Image>();
    }

    // 선택된 쪽은 밝게, 나머지는 어둡게.
    void ApplyModeVisual(int mode)
    {
        if (_modeImage1 != null)
            _modeImage1.color = mode == 0 ? modeSelected : modeDeselected;
        if (_modeImage2 != null)
            _modeImage2.color = mode == 1 ? modeSelected : modeDeselected;
    }

    // ---------- 헬퍼 ----------

    NetworkRunner GetOrCreateRunner()
    {
        NetworkRunner runner = sceneRunner;
        if (runner == null)
        {
            var go = new GameObject("NetworkRunner");
            runner = go.AddComponent<NetworkRunner>();
        }
        runner.ProvideInput = false;   // 이번 마일스톤은 입력 동기화 없음

        // 씬에 놓인 러너는 방을 나가도 살아남는다. 재입장 때 콜백이 두 번 등록되지 않도록
        // 먼저 떼고 다시 붙인다. (등록된 적 없으면 RemoveCallbacks는 아무 일도 하지 않는다.)
        runner.RemoveCallbacks(this);
        runner.AddCallbacks(this);
        return runner;
    }

    static string GenerateCode()
    {
        var sb = new StringBuilder(CodeLength);
        for (int i = 0; i < CodeLength; i++)
            sb.Append(CodeChars[UnityEngine.Random.Range(0, CodeChars.Length)]);
        return sb.ToString();
    }

    // ================= INetworkRunnerCallbacks =================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // 로그는 네트워크로 쏘지 않는다. 각자 로컬에서 문자열을 만든다.
        if (!canvasLobby.activeSelf)
            return;
        string who = player == runner.LocalPlayer ? "내가" : $"Player {player.PlayerId} 님이";
        AppendLog($"{who} 들어왔습니다.");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // 로그는 각자 로컬 생성. (나간 사람 본인은 OnShutdown으로 이미 메뉴에 있다.)

        // 대기방에 있을 때만 처리. (게임 진행 중 처리는 다음 마일스톤)
        if (!canvasLobby.activeSelf)
            return;

        if (player == _hostRef)
        {
            // 방장이 나감 → 남은 사람도 전원 첫 화면으로.
            AppendLog("방장이 나갔습니다.");
            SetMenuStatus("방장이 나갔습니다");
            _ = LeaveRoomAsync();
        }
        else
        {
            // 일반 플레이어가 나감 → 남은 사람은 대기방 유지, UI만 갱신(Update 폴링이 처리).
            // PlayerObject는 Shared Mode에서 소유자가 나가면 자동 Despawn 된다(다음 마일스톤에서 추가).
            AppendLog($"Player {player.PlayerId} 님이 나갔습니다.");
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Room] Shutdown: {shutdownReason}");
        ShowMenu();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        ShowMenu();
        SetMenuStatus($"연결 실패: {reason}");   // ShowMenu 뒤에 써야 남는다
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[Room] 서버 연결 끊김: {reason}");
        ShowMenu();
    }

    // --- 이번 마일스톤에서 사용하지 않는 콜백들 ---
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
}
