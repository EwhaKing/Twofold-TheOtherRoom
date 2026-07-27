using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메뉴 화면. 방 만들기 / 방 들어가기 / 뒤로가기.
/// 코드 입력은 FindRoomView가 따로 담당한다.
/// </summary>
public class MenuView : ScreenView
{
    [Header("위젯")]
    [SerializeField] Button btnMakeRoom;
    [SerializeField] Button btnFindRoom;
    [SerializeField] Button btnBack;

    [Header("상태 문구")]
    [Tooltip("\"접속 중...\" 등. 없어도 동작함.")]
    [SerializeField] StatusLabel status;

    public override ScreenId Id => ScreenId.Menu;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnMakeRoom.onClick.AddListener(OnClickMakeRoom);
        btnFindRoom.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.FindRoom));
        btnBack.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.Title));

        Room.StatusChanged += OnStatusChanged;
    }

    void OnDestroy()
    {
        if (Room != null)
            Room.StatusChanged -= OnStatusChanged;
    }

    void Update()
    {
        if (!IsVisible) return;

        // 접속 중엔 버튼 잠금
        bool idle = !Room.IsConnecting;
        btnMakeRoom.interactable = idle;
        btnFindRoom.interactable = idle;
        btnBack.interactable     = idle;
    }

    protected override void OnVisibilityChanged(bool on)
    {
        if (!on && status != null)
            status.Clear();
    }

    void OnClickMakeRoom()
    {
        Room.CreateRoom();
    }

    // 같은 이벤트를 방찾기 화면도 들으므로, 지금 보이는 화면만 자기 라벨에 그림
    void OnStatusChanged(string message, bool persistent)
    {
        if (IsVisible && status != null)
            status.Show(message, !persistent);
    }
}
