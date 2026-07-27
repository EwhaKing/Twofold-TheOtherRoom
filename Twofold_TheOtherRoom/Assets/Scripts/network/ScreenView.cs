using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 화면 하나를 GameFlow가 캔버스 관리
/// </summary>
public abstract class ScreenView : MonoBehaviour
{
    [Header("Canvas")]
    [FormerlySerializedAs("canvasMenu")]
    [FormerlySerializedAs("canvasLobby")]
    [SerializeField] protected GameObject canvas;

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
