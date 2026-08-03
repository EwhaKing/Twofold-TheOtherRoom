using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 화면. 방 코드 / 플레이어 2칸 / 인원 / 시작·나가기 / 로그.
/// RoomService와 GameSession을 매 프레임 가져와 그린다.
/// 모드 선택은 여기가 아니라 ModeSelectView가 담당한다.
/// </summary>
public class LobbyView : ScreenView
{
    [Header("위젯")]
    [SerializeField] TMP_Text textRoomId;
    [Tooltip("1번 칸 — 항상 방장. 왕관 아이콘은 씬에 고정해 둘 것.")]
    [SerializeField] TMP_Text textHostName;
    [Tooltip("2번 칸 — 게스트. 아무도 없으면 꺼짐.")]
    [SerializeField] TMP_Text textGuestName;
    [Tooltip("\"다른 플레이어를 기다리는 중...\" 게스트가 들어오면 꺼짐.")]
    [SerializeField] GameObject waitingLabel;
    [Tooltip("\"1/2\" 인원 표시")]
    [SerializeField] TMP_Text textPlayerCount;
    [Tooltip("방장에게만 보임")]
    [SerializeField] Button btnStart;
    [SerializeField] Button btnLeave;
    [Tooltip("입장/퇴장 로그. 없으면 콘솔에만 남음.")]
    [SerializeField] TMP_Text lobbyLog;

    // 최대 2인이라 길어질 일은 없지만 무한 누적은 막음
    const int MaxLogLines = 6;
    readonly List<string> _logLines = new List<string>();

    public override ScreenId Id => ScreenId.Lobby;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        // 이름이 고정이라 한 번만 써두면 됨
        textHostName.text  = PlayerNames.Host;
        textGuestName.text = PlayerNames.Guest;

        btnStart.onClick.AddListener(() => GameSession.Instance?.RequestModeSelect());
        btnLeave.onClick.AddListener(() => Room.Leave());

        Room.LogLine    += AppendLog;
        // 화면이 켜질 때가 아니라 "새 방에 들어올 때" 지운다.
        // 게스트 퇴장 로그는 로비로 돌아오기 직전에 찍히므로 여기서 지우면 안 됨
        Room.RoomJoined += ClearLog;
    }

    void OnDestroy()
    {
        if (Room == null) return;
        Room.LogLine    -= AppendLog;
        Room.RoomJoined -= ClearLog;
    }

    void Update()
    {
        if (!IsVisible || !Room.IsInRoom)
            return;

        textRoomId.text = Room.RoomCode;

        int count = Room.PlayerCount;
        textPlayerCount.text = $"{count}/2";

        var gs = GameSession.Instance;
        bool isHost = Room.IsHost;

        bool guestHere = count >= 2;
        textGuestName.gameObject.SetActive(guestHere);
        if (waitingLabel != null)
            waitingLabel.SetActive(!guestHere);

        // 시작 버튼은 방장에게만, 그것도 둘이 다 모였을 때만
        btnStart.gameObject.SetActive(isHost);
        btnStart.interactable = isHost && gs != null && guestHere;
    }

    // ---------- 로그 ----------

    void AppendLog(string line)
    {
        Debug.Log($"[Room] {line}");

        _logLines.Add(line);
        if (_logLines.Count > MaxLogLines)
            _logLines.RemoveAt(0);

        if (lobbyLog != null)
            lobbyLog.text = string.Join("\n", _logLines);
    }

    void ClearLog()
    {
        _logLines.Clear();
        if (lobbyLog != null)
            lobbyLog.text = string.Empty;
    }
}
