using Fusion;
using UnityEngine;

/// <summary>
/// 단독 씬에서 혼자 연동 퍼즐을 테스트하기 위한 1인 Shared 세션.
/// 로비의 2인 제한을 거치지 않고 러너를 띄운 뒤 지정한 coop 프리팹을 스폰함.
/// 에디터 · 개발 빌드에서만 동작.
/// </summary>
public class DebugSoloSession : MonoBehaviour
{
    [Tooltip("세션 이름. 비우면 매번 새로 만듦")]
    [SerializeField] private string sessionName;

    [Tooltip("단독 테스트에서 띄울 coop 프리팹. 세션이 준비된 뒤 여기서 스폰함")]
    [SerializeField] private NetworkPrefabRef[] spawnPrefabs;

    private void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        enabled = false;   // 릴리스 빌드. 꺼지면 Start도 안 돎
#endif
    }

    private async void Start()
    {
        // 정상 흐름이거나 이미 러너가 있으면 비켜남
        if (RoomService.Instance != null) return;
        if (FindAnyObjectByType<NetworkRunner>() != null) return;

        // Fusion이 러너가 붙은 오브젝트를 DontDestroyOnLoad로 옮김
        NetworkRunner runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = false;   // 비연동 게임이라 입력 동기화 안 씀

        string code = string.IsNullOrWhiteSpace(sessionName)
            ? "solo-" + System.Guid.NewGuid().ToString("N").Substring(0, 6)
            : sessionName;

        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = code,
            // 씬은 이미 열려 있으므로 SceneManager를 지정하지 않음
        });

        if (!result.Ok)
        {
            Debug.LogWarning(
                $"[{nameof(DebugSoloSession)}] 세션 시작 실패: {result.ShutdownReason}"
            );

            return;
        }

        Debug.Log($"[{nameof(DebugSoloSession)}] 1인 세션 시작: {code}");

        SpawnTestPuzzles(runner);
    }

    /// 세션이 선 뒤에 스폰. 준비 전에 부르면 Fusion 내부에서 NullReference
    private void SpawnTestPuzzles(NetworkRunner runner)
    {
        if (spawnPrefabs == null) return;

        foreach (NetworkPrefabRef prefab in spawnPrefabs)
        {
            if (!prefab.IsValid) continue;

            runner.Spawn(prefab);
        }
    }
}
