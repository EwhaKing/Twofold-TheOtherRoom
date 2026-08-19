using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면. 시작하기 / 설정 / 종료
/// </summary>
public class TitleView : ScreenView
{
    [Header("위젯")]
    [SerializeField] Button btnStart;
    [SerializeField] Button btnSetting;
    [SerializeField] Button btnQuit;

    public override ScreenId Id => ScreenId.Title;

    void Start()
    {
        btnStart.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.Menu));
        btnSetting.onClick.AddListener(() => GameFlow.Instance.Show(ScreenId.Setting));
        btnQuit.onClick.AddListener(() => GameFlow.Instance.QuitGame());
    }
}
