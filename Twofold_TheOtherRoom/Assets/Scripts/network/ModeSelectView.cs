using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모드 선택 화면. 방장만 본다.
/// 큰 버튼 2개 → 확인 알림창 → 예를 누르면 그대로 게임 시작.
/// 게스트는 반대쪽 모드로 자동 배정된다 (GameFlow가 역할로 씬을 정함).
/// </summary>
public class ModeSelectView : ScreenView
{
    [Header("위젯")]
    [SerializeField] TMP_Text textRoomId;
    [SerializeField] Button btnMode1;
    [SerializeField] Button btnMode2;
    [Tooltip("방을 아예 닫음. 게스트도 같이 나가진다.")]
    [SerializeField] Button btnLeave;

    [Header("확인 알림창")]
    [Tooltip("평소엔 꺼둘 것")]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] TMP_Text confirmText;
    [SerializeField] Button btnYes;
    [SerializeField] Button btnNo;

    [Header("모드 이름")]
    [Tooltip("모드1을 고른 사람이 시작하는 곳")]
    [SerializeField] string modeName1 = "2D";
    [Tooltip("모드2를 고른 사람이 시작하는 곳")]
    [SerializeField] string modeName2 = "3D";

    // 알림창에서 "예"를 눌렀을 때 확정할 모드. -1이면 알림창이 닫혀 있음
    int _pendingMode = -1;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnMode1.onClick.AddListener(() => OpenConfirm(0));
        btnMode2.onClick.AddListener(() => OpenConfirm(1));
        // 방장이 나가면 게스트도 자동으로 나감 (RoomService.OnPlayerLeft가 처리)
        btnLeave.onClick.AddListener(() => Room.Leave());

        btnYes.onClick.AddListener(OnClickYes);
        btnNo.onClick.AddListener(CloseConfirm);

        CloseConfirm();
    }

    void Update()
    {
        if (!IsVisible || !Room.IsInRoom)
            return;

        textRoomId.text = Room.RoomCode;
    }

    protected override void OnVisibilityChanged(bool on)
    {
        // 다시 들어왔을 때 알림창이 열린 채로 남아 있으면 안 됨
        if (!on)
            CloseConfirm();
    }

    void OpenConfirm(int mode)
    {
        _pendingMode = mode;

        string mine  = mode == 0 ? modeName1 : modeName2;
        string other = mode == 0 ? modeName2 : modeName1;
        if (confirmText != null)
            confirmText.text = $"정말 \"{mine}\"를 선택합니까?\n다른 플레이어는 자동으로 \"{other}\"가 됩니다.";

        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    void CloseConfirm()
    {
        _pendingMode = -1;
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    void OnClickYes()
    {
        if (_pendingMode < 0) return;

        int mode = _pendingMode;
        // 화면 전환은 Phase 변화를 받은 GameFlow가 함
        CloseConfirm();
        GameSession.Instance?.ConfirmMode(mode);
    }
}
