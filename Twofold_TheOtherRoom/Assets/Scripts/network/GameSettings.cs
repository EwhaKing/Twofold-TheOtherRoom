using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>UI 표기 언어</summary>
public enum Language
{
    Korean,
}

/// <summary>설정 한 세트. 패널이 복사해 쓰는 임시값의 단위</summary>
[Serializable]
public struct SettingsData
{
    public int width;
    public int height;
    public FullScreenMode screenMode;

    public float master;
    public float bgm;
    public float sfx;

    public Language language;

    public Vector2Int Resolution
    {
        get => new Vector2Int(width, height);
        set { width = value.x; height = value.y; }
    }
}

/// <summary>
/// 설정값. 타이틀/일시정지 설정 패널 공용
///
/// 사용 예시)
/// var draft = GameSettings.Current;   // 임시값 복사
/// draft.master = slider.value;
/// GameSettings.PreviewSound(draft);   // 슬라이더 미리듣기
/// GameSettings.Commit(draft);         // 확인
/// GameSettings.RevertSound();         // 뒤로가기
/// </summary>
public static class GameSettings
{
    /// 적용/저장까지 끝난 확정값
    public static SettingsData Current { get; private set; }

    /// 확정값 변경 알림. 구독 측 OnDestroy에서 해제 필수
    public static event Action<SettingsData> Changed;

    #region Entry Point

    // 해상도 적용 — 씬 Awake 이전
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Current = Load();
        ApplyScreen(Current);
    }

    // 사운드 적용 — SoundManager.Instance 준비 이후
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitSound()
    {
        ApplySound(Current);
        Changed?.Invoke(Current);
    }

    #endregion

    #region Default, Resolution List

    /// 기본값. 해상도는 네이티브에 가장 가까운 목록 항목
    public static SettingsData Defaults => new SettingsData
    {
        Resolution = Resolutions[IndexOfResolution(NativeResolution)],
        screenMode = FullScreenMode.FullScreenWindow,
        master     = 1f,
        bgm        = 1f,
        sfx        = 1f,
        language   = Language.Korean,
    };

    /// 모니터 실제 크기. 16:9가 아닐 수 있음
    static Vector2Int NativeResolution =>
        new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);

    // 지원 비율
    const float TargetAspect = 16f / 9f;

    // 허용 오차. 1366x768 포함, 16:10 · 21:9 · 4:3 제외
    const float AspectTolerance = 0.02f;

    static List<Vector2Int> _resolutions;

    /// 지원 16:9 해상도. 중복 제거, 큰 것부터
    public static IReadOnlyList<Vector2Int> Resolutions
    {
        get
        {
            if (_resolutions != null) return _resolutions;

            _resolutions = new List<Vector2Int>();
            var seen = new HashSet<Vector2Int>();
            foreach (var res in Screen.resolutions)
            {
                if (res.height <= 0) continue;
                if (Mathf.Abs((float)res.width / res.height - TargetAspect) > AspectTolerance) continue;

                var size = new Vector2Int(res.width, res.height);
                if (seen.Add(size)) _resolutions.Add(size);
            }

            // 16:9 없는 모니터용 최후 항목
            if (_resolutions.Count == 0)
                _resolutions.Add(NativeResolution);

            _resolutions.Sort((a, b) => a.x != b.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));
            return _resolutions;
        }
    }

    /// 해상도 드롭다운 현재 위치. 목록에 없으면 픽셀 수 최근접
    public static int IndexOfResolution(Vector2Int size)
    {
        var list = Resolutions;

        int nearest = 0;
        long bestGap = long.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == size) return i;

            long gap = Math.Abs((long)list[i].x * list[i].y - (long)size.x * size.y);
            if (gap < bestGap)
            {
                bestGap = gap;
                nearest = i;
            }
        }
        return nearest;
    }

    // 드롭다운 순서. ExclusiveFullScreen 제외
    static readonly FullScreenMode[] _screenModes =
    {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed,
    };

    /// 창모드 드롭다운 옵션
    public static IReadOnlyList<FullScreenMode> ScreenModes => _screenModes;

    /// 창모드 드롭다운 현재 위치. 목록에 없으면 첫 항목
    public static int IndexOfScreenMode(FullScreenMode mode)
    {
        int index = Array.IndexOf(_screenModes, mode);
        return index < 0 ? 0 : index;
    }

    #endregion

    #region Label

    public static string LabelOf(Vector2Int size) => $"{size.x} x {size.y}";

    public static string LabelOf(FullScreenMode mode) =>
        mode == FullScreenMode.Windowed ? "창 모드" : "전체 화면";

    public static string LabelOf(Language language) => language switch
    {
        Language.Korean => "한국어",
        _               => language.ToString(),
    };

    #endregion

    #region Panel API

    /// 확인 — 확정 + 적용 + 저장
    public static void Commit(SettingsData data)
    {
        Sanitize(ref data);

        Current = data;
        ApplyScreen(Current);
        ApplySound(Current);
        Save(Current);

        Changed?.Invoke(Current);
    }

    /// 슬라이더 미리듣기. 확정값은 그대로
    public static void PreviewSound(SettingsData draft)
    {
        Sanitize(ref draft);
        ApplySound(draft);
    }

    /// 뒤로가기 — 확정 볼륨 복귀
    public static void RevertSound() => ApplySound(Current);

    #endregion

    #region Apply

    static void ApplyScreen(in SettingsData data)
    {
        if (data.width <= 0 || data.height <= 0) return;

        // 동일 상태면 생략 — 창 깜빡임 방지
        if (Screen.width == data.width &&
            Screen.height == data.height &&
            Screen.fullScreenMode == data.screenMode) return;

        // 해상도 · 창모드 동시 적용. 에디터에선 무시됨
        Screen.SetResolution(data.width, data.height, data.screenMode);
    }

    static void ApplySound(in SettingsData data)
    {
        var sound = SoundManager.Instance;
        if (sound == null) return;

        sound.SetMasterVolume(data.master);
        sound.SetBGMVolume(data.bgm);
        sound.SetSFXVolume(data.sfx);
    }

    #endregion

    #region PlayerPrefs

    const string KeyWidth      = "settings.width";
    const string KeyHeight     = "settings.height";
    const string KeyScreenMode = "settings.screenMode";
    const string KeyMaster     = "settings.master";
    const string KeyBgm        = "settings.bgm";
    const string KeySfx        = "settings.sfx";
    const string KeyLanguage   = "settings.language";

    static void Save(in SettingsData data)
    {
        PlayerPrefs.SetInt(KeyWidth, data.width);
        PlayerPrefs.SetInt(KeyHeight, data.height);
        PlayerPrefs.SetInt(KeyScreenMode, (int)data.screenMode);
        PlayerPrefs.SetFloat(KeyMaster, data.master);
        PlayerPrefs.SetFloat(KeyBgm, data.bgm);
        PlayerPrefs.SetFloat(KeySfx, data.sfx);
        PlayerPrefs.SetInt(KeyLanguage, (int)data.language);
        PlayerPrefs.Save();
    }

    static SettingsData Load()
    {
        // 미저장 항목은 기본값 유지
        var data = Defaults;

        data.width      = PlayerPrefs.GetInt(KeyWidth, data.width);
        data.height     = PlayerPrefs.GetInt(KeyHeight, data.height);
        data.screenMode = (FullScreenMode)PlayerPrefs.GetInt(KeyScreenMode, (int)data.screenMode);
        data.master     = PlayerPrefs.GetFloat(KeyMaster, data.master);
        data.bgm        = PlayerPrefs.GetFloat(KeyBgm, data.bgm);
        data.sfx        = PlayerPrefs.GetFloat(KeySfx, data.sfx);
        data.language   = (Language)PlayerPrefs.GetInt(KeyLanguage, (int)data.language);

        Sanitize(ref data);
        return data;
    }

    /// 범위 밖/목록 밖 값의 기본값 복귀
    static void Sanitize(ref SettingsData data)
    {
        data.master = Mathf.Clamp01(data.master);
        data.bgm    = Mathf.Clamp01(data.bgm);
        data.sfx    = Mathf.Clamp01(data.sfx);

        if (data.width <= 0 || data.height <= 0)
            data.Resolution = Defaults.Resolution;

        if (Array.IndexOf(_screenModes, data.screenMode) < 0)
            data.screenMode = Defaults.screenMode;

        if (!Enum.IsDefined(typeof(Language), data.language))
            data.language = Defaults.language;
    }

    #endregion
}
