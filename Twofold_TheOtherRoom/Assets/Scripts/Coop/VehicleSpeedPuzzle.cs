using Fusion;
using UnityEngine;

/// <summary>단계별 난이도 설정</summary>
[System.Serializable]
public struct VehicleSpeedStage
{
    [Tooltip("인정 속도 하한")] public float targetMin;
    [Tooltip("인정 속도 상한")] public float targetMax;
    [Tooltip("액셀 가속도 (초당)")] public float acceleration;
    [Tooltip("브레이크 감속도 (초당)")] public float braking;
    [Tooltip("범위 안일 때 게이지 차는 속도 (초당). 0.2면 5초 걸림")] public float gaugeFillRate;
    [Tooltip("범위 밖일 때 게이지 줄어드는 속도 (초당)")] public float gaugeDrainRate;
}

/// <summary>
/// 차량 속도 퍼즐 공유 상태.
/// 페달을 읽는 쪽이 곧 시뮬을 도는 StateAuthority라 스폰은 반드시 3D.
/// </summary>
public class VehicleSpeedPuzzle : CoopPuzzle
{
    /// CoopPuzzle.Find 로 이 퍼즐을 찾을 때 쓰는 키
    public const string Key = "coop-19";
    /// 2D 쪽 PuzzleManager id
    public const string Id2D = "2D-19";
    /// 3D 쪽 PuzzleManager id
    public const string Id3D = "3D-19";

    public override string PuzzleId => Key;

    [Header("차량 기본 설정")]
    [SerializeField] float maxSpeed = 240f;
    [Tooltip("아무 페달도 안 밟을 때 감속 (초당)")]
    [SerializeField] float friction = 15f;

    [Header("단계별 설정")]
    [SerializeField] VehicleSpeedStage[] stages =
    {
        new VehicleSpeedStage { targetMin = 100f, targetMax = 140f, acceleration = 40f, braking = 60f, gaugeFillRate = 0.2f, gaugeDrainRate = 0.3f },
        new VehicleSpeedStage { targetMin =  50f, targetMax =  90f, acceleration = 40f, braking = 60f, gaugeFillRate = 0.2f, gaugeDrainRate = 0.3f },
        new VehicleSpeedStage { targetMin = 190f, targetMax = 230f, acceleration = 40f, braking = 60f, gaugeFillRate = 0.2f, gaugeDrainRate = 0.3f },
    };

    // ---------- 양쪽이 같이 보는 값 ----------
    /// 현재 속도 0 ~ MaxSpeed
    [Networked] public float Speed { get; private set; }
    /// 현재 단계 게이지 0~1
    [Networked] public float GaugeFill { get; private set; }
    /// 지금 시도 중인 단계 0부터
    [Networked, OnChangedRender(nameof(RaiseChanged))] public int CurrentStage { get; private set; }
    /// 3단계를 모두 통과
    [Networked, OnChangedRender(nameof(RaiseChanged))] public bool Solved { get; private set; }

    /// 단계가 넘어가거나 전부 풀린 순간. 소리/연출용
    public event System.Action Changed;
    void RaiseChanged() => Changed?.Invoke();

    // ---------- 난이도 수치 (읽기 전용) ----------
    public float MaxSpeed => maxSpeed;
    public int StageCount => stages.Length;
    public VehicleSpeedStage GetStage(int i) => stages[Mathf.Clamp(i, 0, stages.Length - 1)];
    public VehicleSpeedStage ActiveStage => GetStage(CurrentStage);

    // ---------- 3D 퍼즐 스크립트가 부르는 API ----------

    bool _accelHeld, _brakeHeld;   // 로컬 전용. 네트워크 미전송
    bool _warnedNoAuthority;

    public void SetAccelerator(bool held) { if (BlockedByAuthority()) return; _accelHeld = held; }
    public void SetBrake(bool held)       { if (BlockedByAuthority()) return; _brakeHeld = held; }

    bool BlockedByAuthority()
    {
        if (Object.HasStateAuthority) return false;

        if (!_warnedNoAuthority)
        {
            _warnedNoAuthority = true;
            Debug.LogWarning($"[{nameof(VehicleSpeedPuzzle)}] 스폰하지 않은 쪽의 페달 입력은 무시 - 이 퍼즐은 3D가 스폰해야 함");
        }
        return true;
    }

    // ---------- 속도 시뮬 ----------

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || Solved || stages.Length == 0) return;

        VehicleSpeedStage s = ActiveStage;
        float dt = Runner.DeltaTime;

        float accel = _accelHeld ?  s.acceleration
                    : _brakeHeld ? -s.braking
                    :              -friction;
        Speed = Mathf.Clamp(Speed + accel * dt, 0f, maxSpeed);

        bool inRange = Speed >= s.targetMin && Speed <= s.targetMax;
        GaugeFill = Mathf.Clamp01(GaugeFill + (inRange ? s.gaugeFillRate : -s.gaugeDrainRate) * dt);

        if (GaugeFill < 1f) return;

        if (CurrentStage >= stages.Length - 1) Solved = true;
        else { CurrentStage++; GaugeFill = 0f; }
    }

    // ---------- 디버그 (2D/3D) ----------

    [ContextMenu("디버그: 액셀 토글")]
    void DebugAccel() { _accelHeld = !_accelHeld; _brakeHeld = false; }

    [ContextMenu("디버그: 브레이크 토글")]
    void DebugBrake() { _brakeHeld = !_brakeHeld; _accelHeld = false; }

    [ContextMenu("디버그: 현재 단계 통과")]
    void DebugPassStage() { if (Object.HasStateAuthority) GaugeFill = 1f; }

    [ContextMenu("디버그: 전부 통과")]
    void DebugSolve()
    {
        if (!Object.HasStateAuthority) return;

        CurrentStage = stages.Length - 1;
        GaugeFill = 1f;
        Solved = true;
    }
}
