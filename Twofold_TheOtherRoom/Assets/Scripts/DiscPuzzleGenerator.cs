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
            Debug.LogWarning($"슬롯 충돌 (H:{hSlot} M:{mSlot} S:{sSlot}) — 마커/구멍 겹침, 다른 시각 권장.");
        }

        int hTarget = (6 - hSlot) % 6;
        int mTarget = (6 - mSlot) % 6;
        int sTarget = (6 - sSlot) % 6;
        hourDisc.targetSlot = hTarget;
        minuteDisc.targetSlot = mTarget;
        secondDisc.targetSlot = sTarget;

        // 각 원판 로컬 기준 구멍 슬롯 계산 (구멍은 index 기준이라 원래 slot 값 사용)
        hourDisc.ApplyHolePattern(new[] { RequiredHole(hSlot, mSlot), RequiredHole(hSlot, sSlot) });
        minuteDisc.ApplyHolePattern(new[] { RequiredHole(mSlot, sSlot) });
        secondDisc.ApplyHolePattern(new int[0]);

        // 시작은 랜덤 슬롯 (정답 위치와 다르게)
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