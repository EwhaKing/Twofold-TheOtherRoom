#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 거울 획득 · 배치 흐름 디버그용. 에디터와 Development Build에서만 컴파일된다.
/// 씬에 붙인 채 정식 빌드 · 데모를 돌리지 말 것.
///
/// 2D는 MirrorPiece 클릭과 같은 경로(MirrorManager.GetMirrorPiece)로,
/// 3D는 퍼즐 클리어와 같은 경로(PuzzleManager.ReportSolved)로 흘려보낸다.
/// 3D를 PuzzleManager로 보내야 거울 오브젝트가 실제로 활성화된다.
///
/// 대상 ID는 씬의 MirrorManager 인스펙터에 채워진 required 목록을 그대로 쓴다.
/// </summary>
public class MirrorFlowDebugger : MonoBehaviour
{
    [Header("단축키 (플레이 중)")]
    [SerializeField] private bool useHotkeys = true;
    [SerializeField] private KeyCode obtainKey = KeyCode.O;
    [SerializeField] private KeyCode completeKey = KeyCode.P;

    [Header("개별 실행용 ID")]
    [SerializeField] private string singleId = "3D-9";

    private void Update()
    {
        if (!useHotkeys) return;

        if (Input.GetKeyDown(obtainKey)) ObtainAll();
        if (Input.GetKeyDown(completeKey)) CompleteAll();
    }

    [ContextMenu("1. 조각 전부 획득")]
    public void ObtainAll()
    {
        if (!Ready()) return;

        ObtainCore();
        LogState();
    }

    [ContextMenu("2. 조각 전부 배치 (거울 완성)")]
    public void CompleteAll()
    {
        if (!Ready()) return;

        ObtainCore();

        foreach (string id in AllRequiredIds())
            MirrorManager.Instance.MirrorPiecePlaced(id);

        LogState();
    }

    [ContextMenu("3. 지정 ID 획득")]
    public void ObtainSingle()
    {
        if (!Ready()) return;

        Obtain(singleId);
        LogState();
    }

    [ContextMenu("4. 지정 ID 배치")]
    public void PlaceSingle()
    {
        if (!Ready()) return;

        MirrorManager.Instance.MirrorPiecePlaced(singleId);
        LogState();
    }

    [ContextMenu("5. 상태 로그")]
    public void LogState()
    {
        if (!Ready()) return;

        MirrorManager manager = MirrorManager.Instance;

        Debug.Log(
            $"[MirrorDebug] 획득 [{string.Join(", ", manager.GetObtainedMirrorPieceIds())}] / " +
            $"배치 [{string.Join(", ", manager.GetPlacedMirrorPieceIds())}] / " +
            $"2D완성 {manager.All2DMirrorPiecesPlaced} · 3D완성 {manager.All3DMirrorPiecesPlaced}", this);
    }

    private void ObtainCore()
    {
        foreach (string id in AllRequiredIds()) Obtain(id);
    }

    private static void Obtain(string id)
    {
        if (id.StartsWith("3D-", StringComparison.OrdinalIgnoreCase))
        {
            Obtain3D(id);
            return;
        }

        MirrorManager.Instance.GetMirrorPiece(id);
    }

    // 3D는 PuzzleManager를 거쳐야 거울 오브젝트가 켜지고 GetMirror까지 이어짐
    private static void Obtain3D(string id)
    {
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(id, PuzzleDimension.ThreeD);
            return;
        }

        Debug.LogWarning($"[MirrorDebug] PuzzleManager가 없어 거울 오브젝트는 안 켜집니다: {id}");
        MirrorManager.Instance.GetMirrorPiece(id);
    }

    private static IEnumerable<string> AllRequiredIds()
    {
        foreach (string id in Required("required2DPieceIds")) yield return id;
        foreach (string id in Required("required3DPieceIds")) yield return id;
    }

    // required 목록은 private 직렬화 필드라 리플렉션으로 읽음
    private static List<string> Required(string fieldName)
    {
        FieldInfo field = typeof(MirrorManager).GetField(
            fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
        {
            Debug.LogError($"[MirrorDebug] MirrorManager.{fieldName}을 찾지 못했습니다. 필드 이름이 바뀌었는지 확인하세요.");
            return new List<string>();
        }

        return field.GetValue(MirrorManager.Instance) as List<string> ?? new List<string>();
    }

    private static bool Ready()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[MirrorDebug] 플레이 모드에서만 동작합니다.");
            return false;
        }

        if (MirrorManager.Instance != null) return true;

        Debug.LogError("[MirrorDebug] MirrorManager가 씬에 없습니다.");
        return false;
    }
}
#endif
