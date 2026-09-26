using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class GlobeInteraction : MonoBehaviour, IInteractable, ICloseInspection
{
    [SerializeField] private InspectionCameraRig cameraRig;

    [Header("콜라이더")]
    [Tooltip("E 상호작용 큰 콜라이더. 비우면 이 오브젝트의 Collider")]
    [SerializeField] private Collider interactCollider;

    [Tooltip("클릭 판정 콜라이더. 보기 진입 중에만 활성")]
    [SerializeField] private Collider clickCollider;

    [Tooltip("클릭 레이 최대 거리")]
    [SerializeField, Min(0.1f)] private float clickDistance = 100f;

    [Header("이동할 Key")]
    [SerializeField] private Transform key;

    [Header("Key 이동 위치")]
    [SerializeField] private Transform[] keyTargets;

    [Tooltip("시작 시 key가 놓인 Key Target 인덱스")]
    [SerializeField] private int startTargetIndex = 2;

    [Header("이동 설정")]
    [SerializeField] private float moveDuration = 0.5f;

    [Header("연동 퍼즐")]
    [Tooltip("정답 위도에 해당하는 Key Target 인덱스")]
    [SerializeField] private int answerTargetIndex;

    [Tooltip("스폰할 13_coop 프리팹. 3D-13이 스폰 주체")]
    [SerializeField] private NetworkPrefabRef puzzle13Prefab;

    private int currentTargetIndex = 0;
    private int moveDirection = 1;
    private Coroutine moveCoroutine;

    // key가 실제로 놓인 위치. currentTargetIndex는 다음에 갈 위치
    private int keyIndex;

    private Puzzle13 puzzle;
    private bool spawnTried;
    private bool warnedNoRunner;
    private bool lastReported;
    private bool cleared;


    private void Awake()
    {
        if (interactCollider == null) interactCollider = GetComponent<Collider>();
        if (clickCollider != null) clickCollider.enabled = false;

        InitKeyPosition();
    }


    private void Start()
    {
        TrySpawnPuzzle();
    }


    private void Update()
    {
        TryClickGlobe();

        if (cleared) return;

        Puzzle13 found = ResolvePuzzle();

        // Start 시점에 못 띄웠으면 재시도
        if (found == null)
        {
            TrySpawnPuzzle();
            return;
        }

        ReportLatitude(found);

        if (!found.Solved) return;

        cleared = true;

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(
                Puzzle13.Id3D, PuzzleDimension.ThreeD);
        }

        Debug.Log($"[{nameof(GlobeInteraction)}] 좌표 일치. 3D-13 클리어");
    }


    /// 비활성화 중 MoveKey가 멈추면 핸들이 남아 이후 이동이 막힘
    private void OnDisable()
    {
        if (moveCoroutine == null) return;

        moveCoroutine = null;

        // 중간에 멈춘 key를 keyIndex가 가리키는 자리로 맞춤
        if (key == null || keyTargets == null) return;
        if (keyIndex < 0 || keyIndex >= keyTargets.Length) return;

        Transform target = keyTargets[keyIndex];
        if (target == null) return;

        key.position = target.position;
        key.rotation = target.rotation;
    }


    #region 보기 진입/종료

    public void Interact()
    {
        if (cameraRig == null) return;

        cameraRig.StartView();

        if (!cameraRig.IsViewing) return;

        // 큰 콜라이더가 클릭 레이를 가리므로 교체
        SetCollidersForViewing(true);

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);
    }


    /// CommonCanvas 뒤로가기 버튼
    public void CloseInspection()
    {
        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Hide(this);

        SetCollidersForViewing(false);

        if (cameraRig != null)
            cameraRig.ExitView();
    }


    private void SetCollidersForViewing(bool viewing)
    {
        if (interactCollider != null) interactCollider.enabled = !viewing;
        if (clickCollider != null) clickCollider.enabled = viewing;
    }

    #endregion


    #region Key 이동

    /// 보기 중 클릭 판정. 콜라이더가 자식이어도 레이로 직접 맞힘
    private void TryClickGlobe()
    {
        // 클리어 후 위치 고정
        if (cleared) return;

        if (clickCollider == null || !clickCollider.enabled) return;
        if (!Input.GetMouseButtonDown(0)) return;

        // UI 버튼을 누를 때 뒤쪽 지구본까지 클릭되는 것을 막음
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Camera viewCamera = cameraRig != null ? cameraRig.ViewCamera : null;
        if (viewCamera == null) return;

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, clickDistance)) return;
        if (hit.collider != clickCollider) return;

        OnGlobeClicked();
    }


    private void OnGlobeClicked()
    {
        if (key == null)
        {
            Debug.LogWarning($"[{nameof(GlobeInteraction)}] Key 미연결");
            return;
        }

        if (keyTargets == null || keyTargets.Length == 0)
        {
            Debug.LogWarning($"[{nameof(GlobeInteraction)}] Key Target 없음");
            return;
        }

        if (moveCoroutine != null) return;

        keyIndex = currentTargetIndex;

        moveCoroutine = StartCoroutine(
            MoveKey(keyTargets[keyIndex])
        );

        Debug.Log(
            $"[{nameof(GlobeInteraction)}] 지구본 클릭 → Key Target {keyIndex + 1} 이동"
        );

        AdvanceTargetIndex();
    }


    /// 씬의 key 위치와 인덱스 정합. 어긋나면 첫 클릭이 엉뚱한 자리로 튐
    private void InitKeyPosition()
    {
        if (keyTargets == null || keyTargets.Length == 0) return;

        keyIndex = Mathf.Clamp(startTargetIndex, 0, keyTargets.Length - 1);
        currentTargetIndex = keyIndex;
        AdvanceTargetIndex();

        Transform target = keyTargets[keyIndex];
        if (key == null || target == null) return;

        key.position = target.position;
        key.rotation = target.rotation;
    }


    /// 키가 인덱스 끝에 닿으면 방향 반전
    private void AdvanceTargetIndex()
    {
        currentTargetIndex += moveDirection;

        if (currentTargetIndex >= keyTargets.Length)
        {
            currentTargetIndex = keyTargets.Length - 2;
            moveDirection = -1;
        }
        else if (currentTargetIndex < 0)
        {
            currentTargetIndex = 1;
            moveDirection = 1;
        }
    }


    private IEnumerator MoveKey(Transform target)
    {
        Vector3 startPosition = key.position;
        Quaternion startRotation = key.rotation;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;

            t = Mathf.SmoothStep(0f, 1f, t);


            key.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );


            key.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        key.position = targetPosition;
        key.rotation = targetRotation;
        moveCoroutine = null;
    }

    #endregion


    #region 연동 퍼즐 보고

    /// 위도가 정답인지. 값이 바뀔 때만 보고
    private void ReportLatitude(Puzzle13 found)
    {
        bool match = keyIndex == answerTargetIndex;
        if (match == lastReported) return;

        lastReported = match;
        found.ReportMatch(PuzzleDimension.ThreeD, match);
    }


    /// 찾기만 함. 매 프레임 불려도 되는 딕셔너리 조회
    private Puzzle13 ResolvePuzzle()
    {
        if (puzzle == null)
            puzzle = CoopPuzzle.Find<Puzzle13>(Puzzle13.Key);

        return puzzle;
    }


    /// 13_coop 스폰 주체. RoomService가 있는 실제 플레이에서만
    private void TrySpawnPuzzle()
    {
        if (spawnTried || !puzzle13Prefab.IsValid) return;
        if (ResolvePuzzle() != null) return;

        // 단독 씬은 DebugSoloSession이 세션 준비를 기다렸다 직접 스폰함
        if (RoomService.Instance == null) return;

        NetworkRunner runner = RoomService.Instance.Runner;

        // 접속 전이면 시도 횟수를 쓰지 않고 다음 프레임에 다시
        if (runner == null || !runner.IsRunning)
        {
            if (!warnedNoRunner)
            {
                warnedNoRunner = true;

                Debug.LogWarning(
                    $"[{nameof(GlobeInteraction)}] 러너가 아직 준비되지 않음. 접속 후 재시도"
                );
            }

            return;
        }

        // Spawn이 오브젝트를 만든 뒤 던지면 재시도가 중복 스폰이 됨. 준비 검사는 위에서 끝냄
        spawnTried = true;

        NetworkObject spawned = runner.Spawn(puzzle13Prefab);
        puzzle = spawned != null ? spawned.GetComponent<Puzzle13>() : null;

        if (puzzle == null)
        {
            Debug.LogWarning(
                $"[{nameof(GlobeInteraction)}] 스폰한 프리팹에 {nameof(Puzzle13)}가 없음"
            );
        }
    }

    #endregion
}
