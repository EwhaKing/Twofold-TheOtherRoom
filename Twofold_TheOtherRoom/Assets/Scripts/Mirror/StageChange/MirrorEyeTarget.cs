using UnityEngine;

/// <summary>
/// 거울에 비친 눈의 자리에 이 오브젝트를 둠. 눈 클로즈업 vcam 의 Look At 대상.
/// 실제 눈 위치를 거울 면 기준으로 뒤집어 매 프레임 따라가므로
/// FOV 를 아무리 좁혀도 화면 정중앙에 눈동자가 옴.
///
/// 사용법
/// 1. 플레이어 머리 본 아래에 빈 오브젝트(EyePoint)를 만들어 눈동자 위치에 둠
/// 2. CutSceneRig 아래에 빈 오브젝트를 만들고 이 컴포넌트를 붙여 Mirror · Eye 연결
/// 3. VCam_CloseUp 의 Look At 에 이 오브젝트, Aim 은 Hard Look At
/// </summary>
[DefaultExecutionOrder(-100)]   // CinemachineBrain 보다 먼저 위치를 잡아야 그 프레임 화면에 반영됨
public class MirrorEyeTarget : MonoBehaviour
{
    [SerializeField] private MirrorReflection mirror;

    [Tooltip("실제 플레이어 눈동자 위치. 머리 본의 자식으로 둬야 움직임을 따라감")]
    [SerializeField] private Transform eye;

    private void LateUpdate()
    {
        if (mirror == null || eye == null) return;

        transform.position = mirror.ReflectPoint(eye.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (mirror == null || eye == null) return;

        // 실제 눈 → 거울 속 눈. 선이 거울 면과 수직으로 지나가면 정상
        Vector3 reflected = mirror.ReflectPoint(eye.position);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eye.position, reflected);
        Gizmos.DrawWireSphere(reflected, 0.03f);
    }
}
