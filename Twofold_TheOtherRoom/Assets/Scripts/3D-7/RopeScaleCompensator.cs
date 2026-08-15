using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;

// Rope.ropeLength(처짐)는 월드 단위인데 Start/End 사이 거리는 부모 스케일을 따라가서 둘이 어긋남
// lossyScale을 곱해 스케일 1일 때의 비율을 유지시킴
// 처짐을 조절할 때는 Rope가 아니라 여기 값을 고칠 것 (매 프레임 덮어씀)
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rope))]
public class RopeScaleCompensator : MonoBehaviour
{
    [Tooltip("스케일 1 기준의 Rope.ropeLength. 클수록 축 처짐")]
    [SerializeField] float baseRopeLength = 1.5f;

    Rope rope;

    void OnEnable()
    {
        Cache();
        Apply();
    }

    void OnValidate()
    {
        Cache();
        Apply();
    }

    void Update()
    {
        if (Apply() && !Application.isPlaying) ForceRebuild();
    }

    void Cache()
    {
        if (!rope) rope = GetComponent<Rope>();
    }

    bool Apply()
    {
        if (rope == null) return false;

        float length = baseRopeLength * transform.lossyScale.x; // 비균등 스케일은 미지원
        if (Mathf.Approximately(rope.ropeLength, length)) return false;

        rope.ropeLength = length; // 값이 바뀔 때만 써서 씬이 계속 dirty 되는 것을 막음
        return true;
    }

    // instantAssign = true여야 RecalculateRope까지 돌아서 처짐이 스프링 애니메이션 없이 바로 잡힘
    void ForceRebuild()
    {
        if (rope.EndPoint == null) return;
        rope.SetEndPoint(rope.EndPoint, true);
    }
}
