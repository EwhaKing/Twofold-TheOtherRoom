using UnityEngine;

/// <summary>
/// 타이틀 버튼 또는 방 안 대기 화면에서 Esc로 여는 설정 화면. 위젯은 SettingsPanel 담당
/// 닫으면 연 화면으로 돌아감 (GameFlow가 기억)
/// </summary>
public class SettingView : ScreenView
{
    [Header("위젯")]
    [SerializeField] SettingsPanel panel;

    public override ScreenId Id => ScreenId.Setting;

    void Start()
    {
        panel.Closed += OnPanelClosed;
    }

    void OnDestroy()
    {
        if (panel != null)
            panel.Closed -= OnPanelClosed;
    }

    protected override void OnVisibilityChanged(bool on)
    {
        if (panel == null) return;

        if (on) panel.Open();
        else    panel.Cancel();
    }

    void OnPanelClosed()
    {
        GameFlow.Instance.CloseSetting();
    }
}
