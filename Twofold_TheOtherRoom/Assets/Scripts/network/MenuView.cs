using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 첫 화면(Canvas_Menu) UI. 방 만들기 / 코드 입력 / 상태 문구.
/// 네트워크는 RoomService에 맡기고 이벤트만 받아 그림.
/// </summary>
public class MenuView : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] GameObject canvasMenu;

    [Header("위젯")]
    [SerializeField] Button btnMakeRoom;
    [Tooltip("누르면 FindRoomUI를 엶. 이미 열려 있으면 입력한 코드로 입장 시도.")]
    [SerializeField] Button btnEnterRoom;
    [Tooltip("코드 입력창 묶음. 평소엔 꺼둠.")]
    [SerializeField] GameObject findRoomUI;
    [SerializeField] TMP_InputField inputCode;

    [Header("상태 문구")]
    [Tooltip("평소엔 꺼져 있다가 메시지가 생기면 잠깐 켜짐. 없어도 동작함.")]
    [SerializeField] TMP_Text menuStatus;
    [Tooltip("켜져 있는 시간(초)")]
    [SerializeField] float menuStatusDuration = 4f;

    // 문구를 다시 끄는 타이머. 새 메시지가 오면 취소하고 다시 시작
    Coroutine _statusRoutine;

    RoomService Room => RoomService.Instance;

    void Start()
    {
        btnMakeRoom.onClick.AddListener(OnClickMakeRoom);
        btnEnterRoom.onClick.AddListener(OnClickFindRoom);

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

        SetStatus(string.Empty, false);
        ShowFindRoomUI(false);
    }

    void OnDestroy()
    {
        if (Room != null)
            Room.StatusChanged -= OnStatusChanged;
    }

    void Update()
    {
        // 접속 중엔 버튼 잠금
        bool idle = !Room.IsConnecting;
        btnMakeRoom.interactable = idle;
        btnEnterRoom.interactable = idle;
    }

    /// GameFlow가 화면 전환할 때 호출
    public void SetVisible(bool on)
    {
        canvasMenu.SetActive(on);
        if (!on)
            ShowFindRoomUI(false);   // 돌아올 땐 항상 접힌 상태
    }

    // ---------- 버튼 ----------

    void OnClickMakeRoom()
    {
        ShowFindRoomUI(false);
        Room.CreateRoom();
    }

    // 첫 클릭은 입력창 열기, 열려 있으면 입장 시도
    void OnClickFindRoom()
    {
        if (!findRoomUI.activeSelf)
        {
            ShowFindRoomUI(true);
            return;
        }
        TryEnterRoom();
    }

    void TryEnterRoom()
    {
        string code = inputCode.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("코드를 입력하세요", true);
            return;
        }
        // 자릿수 검증은 JoinRoom이 하고 StatusChanged로 알려줌
        Room.JoinRoom(code);
    }

    void ShowFindRoomUI(bool on)
    {
        findRoomUI.SetActive(on);
        if (on)
        {
            // 열자마자 타이핑되게 포커스
            inputCode.Select();
            inputCode.ActivateInputField();
        }
    }

    // ---------- 상태 문구 ----------

    void OnStatusChanged(string message, bool persistent)
    {
        SetStatus(message, !persistent);
    }

    /// autoHide = false면 다음 메시지가 올 때까지 계속 떠 있음 (접속 중처럼 끝을 모르는 경우)
    void SetStatus(string message, bool autoHide)
    {
        if (menuStatus == null)
            return;

        if (_statusRoutine != null)
        {
            StopCoroutine(_statusRoutine);
            _statusRoutine = null;
        }

        if (string.IsNullOrEmpty(message))
        {
            menuStatus.gameObject.SetActive(false);
            return;
        }

        menuStatus.text = message;
        menuStatus.gameObject.SetActive(true);

        if (autoHide)
            _statusRoutine = StartCoroutine(HideStatusAfterDelay());
    }

    IEnumerator HideStatusAfterDelay()
    {
        yield return new WaitForSeconds(menuStatusDuration);
        menuStatus.gameObject.SetActive(false);
        _statusRoutine = null;
    }
}
