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

    [ContextMenu("Generate Puzzle")]
    public void Generate()
    {
        float hourAngle = ((hour % 12) + minute / 60f + second / 3600f) * 30f;
        float minuteAngle = (minute + second / 60f) * 6f;
        float secondAngle = second * 6f;

        int hSlot = SnapToSlot(hourAngle);
        int mSlot = SnapToSlot(minuteAngle);
        int sSlot = SnapToSlot(secondAngle);

        hourDisc.targetSlot = hSlot;
        minuteDisc.targetSlot = mSlot;
        secondDisc.targetSlot = sSlot;

        int hourShowsMinute = RequiredHole(hSlot, mSlot);
        int hourShowsSecond = RequiredHole(hSlot, sSlot);
        int minuteShowsSecond = RequiredHole(mSlot, sSlot);

        hourDisc.ApplyHolePattern(BuildPattern(hourShowsMinute, hourShowsSecond));
        minuteDisc.ApplyHolePattern(BuildPattern(minuteShowsSecond, -1));
        secondDisc.ApplyHolePattern(BuildPattern(-1, -1));

        // 원판 초기 회전은 랜덤 위치에서 시작
        hourDisc.currentSlot = Random.Range(0, 6);
        minuteDisc.currentSlot = Random.Range(0, 6);
        secondDisc.currentSlot = Random.Range(0, 6);
        hourDisc.transform.localRotation = Quaternion.Euler(0, 0, hourDisc.currentSlot * 60f);
        minuteDisc.transform.localRotation = Quaternion.Euler(0, 0, minuteDisc.currentSlot * 60f);
        secondDisc.transform.localRotation = Quaternion.Euler(0, 0, secondDisc.currentSlot * 60f);

        Debug.Log($"Target Slots - Hour:{hSlot} Minute:{mSlot} Second:{sSlot}");
    }

    int SnapToSlot(float angleDeg) => Mathf.RoundToInt(angleDeg / 60f) % 6;

    int RequiredHole(int thisTarget, int behindTarget)
        => ((behindTarget - thisTarget) % 6 + 6) % 6;

    int[] BuildPattern(int required1, int required2)
    {
        var list = new System.Collections.Generic.List<int>();
        if (required1 >= 0) list.Add(required1);
        if (required2 >= 0 && !list.Contains(required2)) list.Add(required2);

        while (list.Count < 3)
        {
            int c = Random.Range(0, 6);
            if (list.Contains(c)) continue;
            list.Add(c);
        }
        return list.ToArray();
    }
}