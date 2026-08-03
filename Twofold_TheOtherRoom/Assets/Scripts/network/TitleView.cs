using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면. 시작하기 / 옵션 / 종료.
/// 옵션은 기획 미정이라 버튼만 두고 비활성으로 둔다.
/// </summary>
public class TitleView : ScreenView
{
    [Header("위젯")]
    [SerializeField] Button btnStart;
    [Tooltip("아직 기능 없음. 비워둬도 됨.")]
    [SerializeField] Button btnOptions;
    [SerializeField] Button btnQuit;

    public override ScreenId Id => ScreenId.Title;

    void Start()
    {
        btnStart.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.Menu));
        btnQuit.onClick.AddListener(() => GameFlow.Instance.QuitGame());

        // 눌러도 아무 일 없는 버튼은 눌리지 않는 게 낫다
        if (btnOptions != null)
            btnOptions.interactable = false;
    }
}
