using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대기방(Canvas_Room) UI. 방 코드 / 인원 / 모드 선택 / 시작·나가기 / 로그.
/// RoomService와 GameSession을 매 프레임 가져와 그림.
/// 방장이 바꾼 값이 게스트 화면에도 그대로 반영.
/// </summary>
public class LobbyView : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] GameObject canvasLobby;

    [Header("위젯")]
    [SerializeField] TMP_Text textRoomId;
    [Tooltip("Player2 슬롯의 사람 아이콘. Player1(방장)은 씬에서 항상 켜둘 것.")]
    [SerializeField] GameObject player2Icon;
    [SerializeField] Button btnMode1;           // 방장만 조작
    [SerializeField] Button btnMode2;           // 방장만 조작
    [SerializeField] Button btnStart;           // 방장만 조작
    [SerializeField] Button btnLeave;
    [Tooltip("입장/퇴장 로그. 없으면 콘솔에만 남음.")]
    [SerializeField] TMP_Text lobbyLog;

    [Header("모드 버튼 색")]
    [SerializeField] Color modeSelected = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color modeDeselected = new Color(0.35f, 0.35f, 0.35f, 1f);

    // 최대 2인이라 길어질 일은 없지만 무한 누적은 막음
    const int MaxLogLines = 6;
    readonly List<string> _logLines = new List<string>();

    // 매 프레임 색을 칠하므로 한 번만 캐싱
    Image _modeImage1;
    Image _modeImage2;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnMode1.onClick.AddListener(() => OnClickSelectMode(0));
        btnMode2.onClick.AddListener(() => OnClickSelectMode(1));
        btnStart.onClick.AddListener(() => GameSession.Instance?.RequestStart());
        btnLeave.onClick.AddListener(() => Room.Leave());

        _modeImage1 = SetupModeButton(btnMode1);
        _modeImage2 = SetupModeButton(btnMode2);

        Room.LogLine += AppendLog;
    }

    void OnDestroy()
    {
        if (Room != null)
            Room.LogLine -= AppendLog;
    }

    /// GameFlow가 화면 전환할 때 호출
    public void SetVisible(bool on)
    {
        canvasLobby.SetActive(on);
        if (on)
            ClearLog();
    }

    void Update()
    {
        if (!canvasLobby.activeSelf || !Room.IsInRoom)
            return;

        textRoomId.text = Room.RoomCode;

        // 방장=Player1은 고정. 상대가 왔을 때 Player2만 켬
        int count = Room.PlayerCount;
        player2Icon.SetActive(count >= 2);

        bool isHost = Room.IsHost;
        var gs = GameSession.Instance;

        // 게스트는 모드 조작만 막음
        // GameSession이 아직 Spawn 전이면 기본값 0으로 그림
        ApplyModeVisual(gs != null ? gs.Mode : 0);

        btnStart.gameObject.SetActive(isHost);
        btnMode1.interactable = isHost && gs != null;
        btnMode2.interactable = isHost && gs != null;
        btnStart.interactable = isHost && gs != null && count >= 2;
    }

    // 토글이 아니라 값을 직접 지정. 같은 걸 다시 눌러도 변화 없음
    void OnClickSelectMode(int mode)
    {
        GameSession.Instance?.SetMode(mode);
    }

    // Button transition 색 변화 끄고 inspector 색으로 직접 지정.
    static Image SetupModeButton(Button button)
    {
        button.transition = Selectable.Transition.None;
        return button.GetComponent<Image>();
    }

    void ApplyModeVisual(int mode)
    {
        if (_modeImage1 != null)
            _modeImage1.color = mode == 0 ? modeSelected : modeDeselected;
        if (_modeImage2 != null)
            _modeImage2.color = mode == 1 ? modeSelected : modeDeselected;
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
