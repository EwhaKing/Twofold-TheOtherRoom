using UnityEngine;

/// <summary>
/// 화면 하나를 GameFlow가 캔버스 관리
/// </summary>
public abstract class ScreenView : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] protected GameObject canvas;

    /// 이 View가 담당하는 화면. GameFlow가 이걸 보고 켤 하나를 고름
    public abstract ScreenId Id { get; }

    public bool IsVisible => canvas != null && canvas.activeSelf;

    public void SetVisible(bool on)
    {
        if (canvas != null)
            canvas.SetActive(on);
        OnVisibilityChanged(on);
    }

    /// 화면이 켜지거나 꺼질 때 정리할 게 있으면 override
    protected virtual void OnVisibilityChanged(bool on) { }
}
