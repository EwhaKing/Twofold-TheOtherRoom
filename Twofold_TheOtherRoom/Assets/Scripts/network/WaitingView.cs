using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대기 화면. 방장이 모드를 고르는 동안 게스트가 본다.
/// 안내 문구는 그냥 텍스트라 코드로 건드리지 않는다.
/// </summary>
public class WaitingView : ScreenView
{
    [Header("위젯")]
    [SerializeField] TMP_Text textRoomId;
    [SerializeField] Button btnLeave;

    public override ScreenId Id => ScreenId.Waiting;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnLeave.onClick.AddListener(() => Room.Leave());
    }

    void Update()
    {
        if (!IsVisible || !Room.IsInRoom)
            return;

        textRoomId.text = Room.RoomCode;
    }
}
