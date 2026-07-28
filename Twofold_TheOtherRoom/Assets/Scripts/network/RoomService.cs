using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// 방(세션) 관리 전담. Fusion Shared Mode를 감쌈.
/// UI를 전혀 모름 — 이벤트만 쏘고 화면 처리는 View가 함.
/// **여기에 UI 참조(Button, Text)를 추가하지 말 것.**
/// </summary>
public class RoomService : MonoBehaviour, INetworkRunnerCallbacks
{
    public static RoomService Instance { get; private set; }

    [Header("Runner / Prefab")]
    [Tooltip("씬에 미리 놓아둔 NetworkRunner. 비워두면 런타임에 생성.")]
    [SerializeField] NetworkRunner sceneRunner;
    [Tooltip("NetworkProjectConfig에 등록된 GameSession 프리팹")]
    [SerializeField] NetworkPrefabRef gameSessionPrefab;

    [Header("테스트")]
    [Tooltip("비워두면 방마다 코드를 새로 뽑음. 값을 넣으면 항상 그 코드로 만듦. 배포 전엔 비울 것.")]
    [SerializeField] string debugFixedCode = "";

    /// 방 코드 자릿수. 입력 검증에 쓰라고 공개
    public const int CodeLength = 4;

    // 헷갈리는 0/O/1/I 제외
    const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    // 2인 전용. 초과 입장은 Photon이 거절함
    const int MaxPlayers = 2;

    // 코드가 겹칠 때 다시 뽑는 횟수. 32^4 = 약 100만 가지라 겹칠 일은 사실상 없음
    const int MaxCreateAttempts = 5;

    // ---------- 이벤트 ----------

    /// 보여줄 문구. persistent면 다음 메시지까지 유지. 빈 문자열은 "지우라"는 뜻
    public event Action<string, bool> StatusChanged;

    /// 대기방 로그 한 줄. 네트워크로 안 쏘고 각자 자기 콜백으로 만듦
    public event Action<string> LogLine;

    /// 방 입장 성공
    public event Action RoomJoined;

    /// 방에서 나옴 (퇴장 / 방장 이탈 / 연결 끊김 전부)
    public event Action RoomLeft;

    // ---------- 상태 조회 ----------

    public bool IsConnecting => _connecting;
    public bool IsInRoom     => _runner != null && _runner.IsRunning;
    public bool IsHost       => IsInRoom && _runner.IsSharedModeMasterClient;
    public string RoomCode   => IsInRoom ? _runner.SessionInfo.Name : string.Empty;
    public NetworkRunner Runner => _runner;

    public int PlayerCount
    {
        get
        {
            if (!IsInRoom) return 0;
            int count = 0;
            foreach (var _ in _runner.ActivePlayers)
                count++;
            return count;
        }
    }

    NetworkRunner _runner;
    bool _connecting;

    // 로비에 들어와 있는지. 로그 필터에 씀
    bool _inRoom;

    // Shared Mode엔 방장 PlayerRef API가 없어서, GameSession의 StateAuthority를 캐싱해 씀
    PlayerRef _hostRef;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 방장이 나간 순간 OnPlayerLeft에서 직전 값(=나간 방장)과 비교하려고 미리 저장
        var gs = GameSession.Instance;
        if (gs != null && gs.Object != null)
            _hostRef = gs.Object.StateAuthority;
    }

    // ---------- 공개 API ----------

    /// 새 방 만들기. 코드는 자동 생성 (또는 debugFixedCode)
    public void CreateRoom()
    {
        if (_connecting) return;

        string code = string.IsNullOrWhiteSpace(debugFixedCode)
            ? GenerateCode()
            : debugFixedCode.Trim().ToUpperInvariant();
        _ = StartRoomAsync(code, allowCreate: true);
    }

    /// 기존 방 입장. 없는 코드면 실패함 (새로 만들지 않음)
    public void JoinRoom(string code)
    {
        if (_connecting) return;

        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (!IsValidCode(code))
        {
            StatusChanged?.Invoke($"코드는 {CodeLength}자리입니다", true);
            return;
        }
        _ = StartRoomAsync(code, allowCreate: false);
    }

    /// 방 나가기. 완료되면 OnShutdown → RoomLeft
    public void Leave()
    {
        _ = LeaveAsync();
    }

    /// 길이 + 문자 집합 확인. 대문자로 바꿔서 넘길 것
    public static bool IsValidCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != CodeLength)
            return false;

        foreach (char c in code)
        {
            if (CodeChars.IndexOf(c) < 0)
                return false;
        }
        return true;
    }

    // ---------- 접속 ----------

    /// <param name="allowCreate">
    /// true = 없으면 새로 만듦(방 만들기). false = 없으면 GameNotFound로 실패(방 찾기).
    /// 구분 안 하면 코드 잘못 쳤을 때 새 방 만듦.
    /// </param>
    async Task StartRoomAsync(string code, bool allowCreate)
    {
        _connecting = true;
        StatusChanged?.Invoke($"접속 중... ({code})", true);

        // 던져놓고 기다리지 않는 Task라 try/catch 문 필요
        try
        {
            // 실패하면 await 도중 OnShutdown이 _runner를 비우므로 지역변수로 가지고 있기
            var runner = GetOrCreateRunner();
            _runner = runner;

            // 디버그로 방 코드 고정 시 코드 재시도 안함
            bool canRetryWithNewCode = allowCreate && string.IsNullOrWhiteSpace(debugFixedCode);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            StartGameResult result;

            for (int attempt = 1; ; attempt++)
            {
                Debug.Log($"[Room] StartGame 호출: {code} " +
                          $"(allowCreate: {allowCreate}, 시도 {attempt}/{MaxCreateAttempts})");

                result = await runner.StartGame(new StartGameArgs
                {
                    GameMode    = GameMode.Shared,
                    SessionName = code,
                    PlayerCount = MaxPlayers,   // NetworkProjectConfig 값을 덮어씀
                    EnableClientSessionCreation = allowCreate,
                    // 씬은 GameFlow가 직접 Additive로 관리하므로 여기서 지정하지 않음
                });

                bool codeTaken = result.ShutdownReason == ShutdownReason.GameIdAlreadyExists;
                if (result.Ok || !codeTaken || !canRetryWithNewCode || attempt >= MaxCreateAttempts)
                    break;

                code = GenerateCode();
                Debug.LogWarning($"[Room] 코드 충돌 — 새 코드로 재시도: {code}");
                StatusChanged?.Invoke($"접속 중... ({code})", true);

                // 직전 시도로 러너가 정리됐을 수 있어 다시 확보
                runner = GetOrCreateRunner();
                _runner = runner;
            }

            sw.Stop();
            Debug.Log($"[Room] StartGame {(result.Ok ? "성공" : "실패")} — " +
                      $"{sw.ElapsedMilliseconds}ms, reason: {result.ShutdownReason}");

            if (!result.Ok)
            {
                StatusChanged?.Invoke(DescribeFailure(result.ShutdownReason), false);
                return;
            }

            // 성공 직후 끊긴 경우. SessionInfo를 읽기 전에 걸러냄
            if (!runner.IsRunning)
            {
                Debug.LogWarning("[Room] StartGame은 성공했으나 러너가 이미 정지 상태");
                StatusChanged?.Invoke("접속이 중단되었습니다", false);
                return;
            }

            Debug.Log($"[Room] 입장 완료 — region: {runner.SessionInfo.Region}, " +
                      $"master: {runner.IsSharedModeMasterClient}");

            // OnShutdown이 중간에 비웠을 수 있으니 되돌려놓음
            _runner = runner;
            _inRoom = true;

            // 세션을 처음 만든 피어가 방장. 방장만 GameSession을 Spawn (씬에 두면 중복 생성됨)
            if (runner.IsSharedModeMasterClient)
                runner.Spawn(gameSessionPrefab);

            StatusChanged?.Invoke(string.Empty, false);
            RoomJoined?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            StatusChanged?.Invoke("접속 오류", false);
        }
        finally
        {
            _connecting = false;
        }
    }

    async Task LeaveAsync()
    {
        if (_runner != null)
            await _runner.Shutdown();   // 완료되면 OnShutdown에서 RoomLeft 발생
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
        runner.ProvideInput = false;   // 비연동 게임이라 입력 동기화 안 씀

        // 씬 러너는 방을 나가도 살아남음. 재입장 때 콜백이 두 번 등록되지 않게 먼저 뗌
        runner.RemoveCallbacks(this);
        runner.AddCallbacks(this);
        return runner;
    }

    static string DescribeFailure(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.GameNotFound:        return "방이 존재하지 않습니다";
            case ShutdownReason.GameIsFull:          return "방이 가득 찼습니다";
            case ShutdownReason.GameIdAlreadyExists: return "같은 코드의 방이 이미 있습니다";
            default:                                 return $"접속 실패: {reason}";
        }
    }

    static string GenerateCode()
    {
        var sb = new StringBuilder(CodeLength);
        for (int i = 0; i < CodeLength; i++)
            sb.Append(CodeChars[UnityEngine.Random.Range(0, CodeChars.Length)]);
        return sb.ToString();
    }

    // 여러 콜백에서 부르는 공통 처리
    void HandleLeftRoom()
    {
        bool wasInRoom = _inRoom;

        _runner = null;
        _inRoom = false;

        if (wasInRoom)
            RoomLeft?.Invoke();
    }

    // ================= INetworkRunnerCallbacks =================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!_inRoom) return;
        string who = player == runner.LocalPlayer ? "내가" : $"{PlayerNames.Guest} 님이";
        LogLine?.Invoke($"{who} 들어왔습니다.");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // 대기방에 있을 때만 처리. 게임 중 이탈은 다음 마일스톤
        if (!_inRoom) return;

        if (player == _hostRef)
        {
            // 방장이 나감 → 남은 사람도 첫 화면으로
            LogLine?.Invoke("방장이 나갔습니다.");
            StatusChanged?.Invoke("방장이 나갔습니다", false);
            Leave();
            return;
        }

        // 게스트가 나감 → 방장은 로비로 돌아감
        LogLine?.Invoke($"{PlayerNames.Guest} 님이 나갔습니다.");
        GameSession.Instance?.HandleGuestLeft();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Room] Shutdown: {shutdownReason}");
        HandleLeftRoom();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        HandleLeftRoom();
        StatusChanged?.Invoke($"연결 실패: {reason}", false);   // RoomLeft 뒤에 쏴야 남음
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[Room] 서버 연결 끊김: {reason}");
        HandleLeftRoom();
    }

    // --- 안 쓰는 콜백들. 인터페이스 멤버라 지우면 컴파일 안 됨 ---
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
