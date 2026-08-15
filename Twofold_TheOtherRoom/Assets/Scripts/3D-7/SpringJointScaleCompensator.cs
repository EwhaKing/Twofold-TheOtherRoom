using UnityEngine;

// SpringJoint의 힘과 거리는 전부 월드 단위 절대값이라 부모 스케일을 따라가지 않음
// 평형 거리 d = maxDistance + mass * gravity / spring 이 스케일과 무관하게 고정되어,
// 축소된 씬에서는 플러그가 그만큼 더 아래로 매달림
// lossyScale을 곱해 스케일 1일 때의 비율을 유지시킴 (매 프레임 덮어씀)
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpringJoint))]
public class SpringJointScaleCompensator : MonoBehaviour
{
    [Tooltip("스케일 1 기준의 SpringJoint.spring")]
    [SerializeField] float baseSpring = 50f;
    [Tooltip("스케일 1 기준의 SpringJoint.minDistance")]
    [SerializeField] float baseMinDistance = 0f;
    [Tooltip("스케일 1 기준의 SpringJoint.maxDistance")]
    [SerializeField] float baseMaxDistance = 0.8f;

    SpringJoint joint;

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
        Apply();
    }

    void Cache()
    {
        if (!joint) joint = GetComponent<SpringJoint>();
    }

    void Apply()
    {
        if (joint == null) return;

        float scale = transform.lossyScale.x; // 비균등 스케일은 미지원
        if (scale <= 0f) return;

        // 거리는 곱하고 spring은 나눠야 mass * gravity / spring 항까지 같은 비율로 줄어듦
        float minDistance = baseMinDistance * scale;
        float maxDistance = baseMaxDistance * scale;
        float spring = baseSpring / scale;

        // 값이 바뀔 때만 써서 씬이 계속 dirty 되는 것을 막음
        if (!Mathf.Approximately(joint.minDistance, minDistance)) joint.minDistance = minDistance;
        if (!Mathf.Approximately(joint.maxDistance, maxDistance)) joint.maxDistance = maxDistance;
        if (!Mathf.Approximately(joint.spring, spring)) joint.spring = spring;
    }
}
