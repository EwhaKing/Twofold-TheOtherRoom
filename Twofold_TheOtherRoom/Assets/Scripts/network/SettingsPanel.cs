using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 위젯 묶음. 타이틀 Screen_Setting과 일시정지 설정창이 공유.
/// 확인 전까지는 임시값만 바뀌고, GameSettings의 확정값은 그대로
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("Graphic")]
    [SerializeField] TMP_Dropdown dropdownResolution;
    [SerializeField] TMP_Dropdown dropdownScreenMode;

    [Header("Sound")]
    [SerializeField] Slider sliderMaster;
    [SerializeField] Slider sliderBgm;
    [SerializeField] Slider sliderSfx;

    [Header("Language")]
    [Tooltip("언어 섹션이 없는 설정창은 비워둘 것")]
    [SerializeField] Button btnLanguagePrev;
    [SerializeField] Button btnLanguageNext;
    [SerializeField] TMP_Text languageLabel;

    [Header("Buttons")]
    [SerializeField] Button btnDefault;
    [SerializeField] Button btnConfirm;
    [SerializeField] Button btnBack;

    /// 뒤로가기로 닫힐 때. 화면 전환은 이걸 듣는 쪽이 결정
    public event Action Closed;

    // 확인 전까지의 임시값
    SettingsData _draft;

    void Awake()
    {
        BuildOptions();

        dropdownResolution.onValueChanged.AddListener(OnResolutionChanged);
        dropdownScreenMode.onValueChanged.AddListener(OnScreenModeChanged);

        sliderMaster.onValueChanged.AddListener(value => { _draft.master = value; PreviewSound(); });
        sliderBgm.onValueChanged.AddListener(value => { _draft.bgm = value; PreviewSound(); });
        sliderSfx.onValueChanged.AddListener(value => { _draft.sfx = value; PreviewSound(); });

        if (btnLanguagePrev != null) btnLanguagePrev.onClick.AddListener(() => ShiftLanguage(-1));
        if (btnLanguageNext != null) btnLanguageNext.onClick.AddListener(() => ShiftLanguage(1));

        btnDefault.onClick.AddListener(OnDefault);
        btnConfirm.onClick.AddListener(OnConfirm);
        btnBack.onClick.AddListener(OnBack);
    }

    #region Open / Close

    /// 패널 열 때. 확정값을 임시값으로 복사해 위젯에 반영
    public void Open()
    {
        _draft = GameSettings.Current;
        SyncWidgets();
    }

    /// 패널 닫을 때. 미리듣기로 바꿔둔 볼륨 복귀
    public void Cancel()
    {
        GameSettings.RevertSound();
    }

    #endregion

    #region Make Dropdown Option

    // 해상도 목록은 PC마다 다르므로 인스펙터가 아닌 코드로 채움
    void BuildOptions()
    {
        var resolutionLabels = new List<string>();
        foreach (var size in GameSettings.Resolutions)
            resolutionLabels.Add(GameSettings.LabelOf(size));

        dropdownResolution.ClearOptions();
        dropdownResolution.AddOptions(resolutionLabels);

        var screenModeLabels = new List<string>();
        foreach (var mode in GameSettings.ScreenModes)
            screenModeLabels.Add(GameSettings.LabelOf(mode));

        dropdownScreenMode.ClearOptions();
        dropdownScreenMode.AddOptions(screenModeLabels);
    }

    // SetValueWithoutNotify — 값을 넣는 것만으로 임시값이 바뀌지 않게
    void SyncWidgets()
    {
        dropdownResolution.SetValueWithoutNotify(GameSettings.IndexOfResolution(_draft.Resolution));
        dropdownScreenMode.SetValueWithoutNotify(GameSettings.IndexOfScreenMode(_draft.screenMode));

        sliderMaster.SetValueWithoutNotify(_draft.master);
        sliderBgm.SetValueWithoutNotify(_draft.bgm);
        sliderSfx.SetValueWithoutNotify(_draft.sfx);

        SyncLanguageLabel();
    }

    void SyncLanguageLabel()
    {
        if (languageLabel != null)
            languageLabel.text = GameSettings.LabelOf(_draft.language);
    }

    #endregion

    #region Dropdown Event

    void OnResolutionChanged(int index)
    {
        _draft.Resolution = GameSettings.Resolutions[index];
    }

    void OnScreenModeChanged(int index)
    {
        _draft.screenMode = GameSettings.ScreenModes[index];
    }

    // 사운드만 확인 전에도 들려줌
    void PreviewSound() => GameSettings.PreviewSound(_draft);

    void ShiftLanguage(int step)
    {
        int count = Enum.GetValues(typeof(Language)).Length;

        int index = ((int)_draft.language + step) % count;
        if (index < 0) index += count;

        _draft.language = (Language)index;
        SyncLanguageLabel();
    }

    #endregion

    #region Bottom Buttons

    void OnDefault()
    {
        _draft = GameSettings.Defaults;
        SyncWidgets();
        PreviewSound();
    }

    void OnConfirm()
    {
        GameSettings.Commit(_draft);
    }

    void OnBack()
    {
        Cancel();
        Closed?.Invoke();
    }

    #endregion
}
