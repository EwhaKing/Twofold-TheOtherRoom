using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마이크 안내 화면. 방장/게스트 둘 다 본다.
/// 둘 다 "예"를 눌러야 모드 선택으로 넘어가고, 누구든 뒤로가기를 누르면 둘 다 로비로.
/// </summary>
public class NoMicInfoView : ScreenView
{
    [Header("위젯")]
    [Tooltip("누르면 이 버튼(괄호 포함)이 꺼지고 대기 문구가 켜짐")]
    [SerializeField] Button btnYes;
    [Tooltip("방을 나가지 않고 둘 다 로비로 돌아감")]
    [SerializeField] Button btnBack;
    [Tooltip("\"상대의 확인을 기다리는 중...\" 내가 \"예\"를 누른 뒤에만 켜짐")]
    [SerializeField] GameObject waitingLabel;

    public override ScreenId Id => ScreenId.MicInfo;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnYes.onClick.AddListener(() => GameSession.Instance?.RpcConfirmMic(Room.IsHost));
        btnBack.onClick.AddListener(() => GameSession.Instance?.RpcBackToLobby());
    }

    void Update()
    {
        if (!IsVisible || !Room.IsInRoom)
            return;

        var gs = GameSession.Instance;
        bool confirmed = gs != null && gs.HasConfirmedMic(Room.IsHost);

        btnYes.gameObject.SetActive(!confirmed);
        if (waitingLabel != null)
            waitingLabel.SetActive(confirmed);
    }
}
