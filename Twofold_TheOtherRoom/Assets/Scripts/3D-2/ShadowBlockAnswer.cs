using System.Collections;
using Photon.Client.StructWrapping;
using UnityEngine;

public class ShadowBlockAnswer : MonoBehaviour
{
    [Header("Blocks From Left To Right")]
    public MoveBlocks[] blocks;

    [Header("Light Gather Effect")]
    [SerializeField] private Light spotLight;
    [Tooltip("빛이 최종적으로 모일 월드 위치입니다.")]
    [SerializeField] private Transform lightGatherTarget;
    [SerializeField, Min(0f)] private float gatherTime = 1.5f;
    [Tooltip("목표 지점보다 빛이 조금 더 뻗어야 할 때 사용하는 여유 거리입니다.")]
    [SerializeField, Min(0f)] private float rangePadding = 0.1f;
    [SerializeField] private float gatheredInnerSpotAngle = 15f;
    [SerializeField, Range(1f, 179f)] private float gatheredSpotAngle = 25f;
    [Tooltip("목표를 바라본 뒤 Spotlight의 로컬 X축에 추가할 각도입니다.")]
    [SerializeField] private float pitchOffset = 10f;

    [Header("Puzzle Report")]
    [SerializeField] private string puzzleId = "3D-2";

    [SerializeField] private GameObject mirror_2;

    private readonly int[] answer = { 1, 3, 4, 2 };
    private bool isCleared;

    void Awake()
    {
        if (mirror_2 != null) mirror_2.SetActive(false);
    }

    public void CheckClear()
    {
        if (isCleared) return;

        if (blocks == null || blocks.Length != answer.Length)
        {
            Debug.LogWarning("블록 개수와 정답 개수가 다름");
            return;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null) return;

            if (blocks[i].GetZone() != answer[i])
            {
                Debug.Log("현재"+i+"의 위치는"+ blocks[i].GetZone() + "but answer is" +answer[i] );
                return;
            }
        }
        // 연출 중에도 다시 판정되지 않도록 먼저 클리어 처리한다.
        isCleared = true;
        SoundManager.Instance.PlaySFX(SFXType.SteppingCorrect);
        StartCoroutine(GatherLightAndReportSolved());
    }

    private IEnumerator GatherLightAndReportSolved()
    {
        if (spotLight != null)
        {
            float startRange = spotLight.range;
            float startSpotAngle = spotLight.spotAngle;
            float startInnerAngle = spotLight.innerSpotAngle;
            Quaternion startRotation = spotLight.transform.rotation;
            Quaternion targetRotation = startRotation;
            float targetRange = startRange;

            if (lightGatherTarget != null)
            {
                Vector3 toTarget = lightGatherTarget.position - spotLight.transform.position;

                if (toTarget.sqrMagnitude > Mathf.Epsilon)
                {
                    // 목표 회전값이랑 목표 범위 계산
                    Quaternion lookRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    targetRotation = lookRotation * Quaternion.Euler(pitchOffset, 0f, 0f);
                    targetRange = toTarget.magnitude + rangePadding;
                }
            }
            else
            {
                Debug.LogWarning("[ShadowBlockAnswer] Light Gather Target이 연결되지 않았습니다.", this);
            }

            if (gatherTime > 0f)
            {
                float elapsed = 0f;

                while (elapsed < gatherTime)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / gatherTime));

                    spotLight.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                    spotLight.range = Mathf.Lerp(startRange, targetRange, t);
                    spotLight.spotAngle = Mathf.Lerp(startSpotAngle, gatheredSpotAngle, t);
                    spotLight.innerSpotAngle = Mathf.Lerp(startInnerAngle,gatheredInnerSpotAngle,t);
                    yield return null;
                }
            }

            spotLight.transform.rotation = targetRotation;
            spotLight.range = targetRange;
            spotLight.innerSpotAngle = gatheredInnerSpotAngle;
            spotLight.spotAngle = gatheredSpotAngle;    
        }

        if (PuzzleManager.Instance == null)
        {
            Debug.LogError("[ShadowBlockAnswer] PuzzleManager를 찾을 수 없습니다.", this);
            yield break;
        }

        yield return new WaitForSeconds(0.1f);

        PuzzleManager.Instance.ReportSolved(puzzleId, PuzzleDimension.ThreeD);

        mirror_2.SetActive(true);
    }
}
