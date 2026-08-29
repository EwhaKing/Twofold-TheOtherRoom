using System;
using System.Collections;
using UnityEngine;

// 퍼즐 성공 연출 - 기둥을 내려 리셋 버튼을 거울 조각으로 교체
public class PillarMirrorReveal : MonoBehaviour
{
    #region Inspector
    [Header("References")]
    [SerializeField] private Transform pillarInside;    // 오르내릴 기둥
    [SerializeField] private Collider buttonCollider;   // 리셋 버튼 콜라이더
    [SerializeField] private GameObject buttonObject;   // 숨길 버튼
    [SerializeField] private GameObject mirrorObject;   // 드러낼 거울 조각

    [Header("Motion")]
    [SerializeField] private float descendDepth = 1f;      // 하강 깊이
    [SerializeField] private float moveDuration = 0.8f;    // 상하 이동 시간
    [SerializeField] private float holdDuration = 0.2f;    // 교체 후 대기
    [SerializeField] private AnimationCurve moveEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    #endregion

    private string puzzleId = "3D-9";
    private PuzzleDimension dimension = PuzzleDimension.ThreeD;

    private Vector3 startPos;   // 기둥 원위치
    private bool played;        // 중복 실행 방지

    void Awake()
    {
        if (pillarInside != null) startPos = pillarInside.localPosition;
    }

    // 성공 연출 - 컨트롤러가 호출
    public IEnumerator PlayReveal()
    {
        if (pillarInside == null)
        {
            Debug.LogWarning("[PillarMirrorReveal] pillarInside가 비어 있습니다.", this);
            yield break;
        }
        if (played) yield break;
        played = true;

        // 기둥 내려가는 동안 상호작용/E 프롬프트 차단
        if (buttonCollider != null) buttonCollider.enabled = false;

        Vector3 downPos = startPos + new Vector3(0f, -descendDepth, 0f);

        yield return MoveTo(startPos, downPos);   // 내려가기

        // 시야에서 가려진 동안 교체
        if (buttonObject != null) buttonObject.SetActive(false);
        //if (mirrorObject != null) mirrorObject.SetActive(true);
        if (PuzzleManager.Instance != null) // 매니저 보고
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);

        yield return new WaitForSeconds(holdDuration);

        yield return MoveTo(downPos, startPos);   // 올라오기

        pillarInside.localPosition = startPos;    // 오차 보정
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to)
    {
        yield return Animate(moveDuration, moveEase,
            t => pillarInside.localPosition = Vector3.Lerp(from, to, t));
    }

    // 이동 공용 보간
    IEnumerator Animate(float duration, AnimationCurve ease, Action<float> apply)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            apply(ease.Evaluate(elapsed / duration));
            yield return null;
        }

        // 커브 끝값과 무관하게 목표 지점으로 정확히 스냅
        apply(1f);
    }
}
