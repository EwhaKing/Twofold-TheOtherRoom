using UnityEngine;

/// <summary>
/// 타이틀에서 여는 설정 화면. 위젯은 SettingsPanel 담당
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
        GameFlow.Instance.Show(ScreenId.Title);
    }
}
