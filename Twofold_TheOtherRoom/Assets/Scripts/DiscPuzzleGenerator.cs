using System.Collections.Generic;
using UnityEngine;

public class DiscPuzzleGenerator : MonoBehaviour
{
    [Header("입력 시각")]
    public int hour = 12;
    public int minute = 40;
    public int second = 10;

    [Header("원판 참조 (앞→뒤: 시/분/초)")]
    public Disc hourDisc;
    public Disc minuteDisc;
    public Disc secondDisc;

    [Header("구멍 설정")]
    [Tooltip("원판당 전체 구멍 상한 (필수 포함)")]
    public int maxHolesPerDisc = 3;

    void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Puzzle")]
    public void Generate()
    {
        float hourAngle = ((hour % 12) + minute / 60f + second / 3600f) * 30f;
        float minuteAngle = (minute + second / 60f) * 6f;
        float secondAngle = second * 6f;

        int hSlot = SnapToSlot(hourAngle);
        int mSlot = SnapToSlot(minuteAngle);
        int sSlot = SnapToSlot(secondAngle);

        if (hSlot == mSlot || hSlot == sSlot || mSlot == sSlot)
        {
            Debug.LogWarning($"슬롯 충돌 (H:{hSlot} M:{mSlot} S:{sSlot}) — 마커/빈 슬롯 겹침, 다른 시각 권장.");
        }

        int hTarget = (6 - hSlot) % 6;
        int mTarget = (6 - mSlot) % 6;
        int sTarget = (6 - sSlot) % 6;
        hourDisc.targetSlot = hTarget;
        minuteDisc.targetSlot = mTarget;
        secondDisc.targetSlot = sTarget;

        // 각 원판 로컬 기준 구멍 계산 (index 기준이라 원래 slot 값 사용)
        // 필수 + 랜덤 추가 구멍
        hourDisc.ApplyHolePattern(BuildHoles(RequiredHole(hSlot, mSlot), RequiredHole(hSlot, sSlot)));
        minuteDisc.ApplyHolePattern(BuildHoles(RequiredHole(mSlot, sSlot)));
        secondDisc.ApplyHolePattern(BuildHoles());

        // 시작 - 랜덤 슬롯 (정답 위치와 다르게)
        hourDisc.SetSlot(RandomSlotExcept(hTarget));
        minuteDisc.SetSlot(RandomSlotExcept(mTarget));
        secondDisc.SetSlot(RandomSlotExcept(sTarget));

        Debug.Log($"Target Slots - Hour:{hTarget} Minute:{mTarget} Second:{sTarget}");
    }

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle()
    {
        hourDisc.ResetDisc();
        minuteDisc.ResetDisc();
        secondDisc.ResetDisc();
    }

    // 마커 슬롯
    const int MARKER_SLOT = 0;

    // 최종 구멍 목록
    int[] BuildHoles(params int[] required)
    {
        var holes = new List<int>();
        foreach (int h in required)
            if (!holes.Contains(h)) holes.Add(h);

        int cap = Mathf.Clamp(maxHolesPerDisc, holes.Count, 6);
        int targetCount = Random.Range(holes.Count, cap + 1);

        int guard = 0;
        while (holes.Count < targetCount && guard++ < 100)
        {
            int s = Random.Range(0, 6);
            if (s == MARKER_SLOT) continue;         // 마커 슬롯 구멍 제외
            if (!holes.Contains(s)) holes.Add(s);
        }
        return holes.ToArray();
    }

    int SnapToSlot(float angleDeg) => Mathf.RoundToInt(angleDeg / 60f) % 6;

    int RequiredHole(int thisTarget, int behindTarget)
        => ((thisTarget - behindTarget) % 6 + 6) % 6;

    // 정답 슬롯을 피해서 랜덤 시작 슬롯 뽑기
    int RandomSlotExcept(int exclude)
    {
        int slot = Random.Range(0, 5);      // 0~4
        if (slot >= exclude) slot++;        // exclude 건너뛰어 0~5 매핑
        return slot;
    }
}