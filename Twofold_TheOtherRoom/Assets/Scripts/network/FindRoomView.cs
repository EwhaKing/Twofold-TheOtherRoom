using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방 ID 검색 화면. 코드 입력 필드 + 입장 / 뒤로가기.
/// "방이 존재하지 않습니다" 같은 문구도 여기에 뜬다.
/// </summary>
public class FindRoomView : ScreenView
{
    [Header("위젯")]
    [SerializeField] TMP_InputField inputCode;
    [Tooltip("비워두면 Enter로만 입장. 두는 편을 권장.")]
    [SerializeField] Button btnEnter;
    [SerializeField] Button btnBack;

    [Header("상태 문구")]
    [Tooltip("입력 오류 / 접속 실패 문구. 없어도 동작함.")]
    [SerializeField] StatusLabel status;

    public override ScreenId Id => ScreenId.FindRoom;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        if (btnEnter != null)
            btnEnter.onClick.AddListener(TryEnterRoom);
        btnBack.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.Menu));

        inputCode.onSubmit.AddListener(_ => TryEnterRoom());   // Enter로도 입장
        // 잘못 칠 여지를 줄임
        inputCode.characterLimit = RoomService.CodeLength;
        inputCode.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
        inputCode.onValueChanged.AddListener(text =>
        {
            string upper = text.ToUpperInvariant();
            if (upper != text)
                inputCode.text = upper;
        });

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

        bool idle = !Room.IsConnecting;
        if (btnEnter != null)
            btnEnter.interactable = idle;
        btnBack.interactable = idle;
    }

    protected override void OnVisibilityChanged(bool on)
    {
        if (status != null)
            status.Clear();

        if (!on)
        {
            inputCode.text = string.Empty;
            return;
        }

        // 열자마자 타이핑되게 포커스
        inputCode.Select();
        inputCode.ActivateInputField();
    }

    void TryEnterRoom()
    {
        string code = inputCode.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            if (status != null)
                status.Show("코드를 입력하세요");
            return;
        }
        // 자릿수 검증은 JoinRoom이 하고 StatusChanged로 알려줌
        Room.JoinRoom(code);
    }

    void OnStatusChanged(string message, bool persistent)
    {
        if (IsVisible && status != null)
            status.Show(message, !persistent);
    }
}
