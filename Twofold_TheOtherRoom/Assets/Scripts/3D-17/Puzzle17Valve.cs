using Fusion;
using UnityEngine;

public class Puzzle17Valve : MonoBehaviour
{
    [Header("공유 퍼즐 프리팹")]
    [SerializeField] private NetworkObject puzzle17Prefab;

    private NetworkRunner runner;
    private bool clicked;

    private void Start()
    {
        runner = FindAnyObjectByType<NetworkRunner>();
    }

    private void OnMouseDown()
    {
        // 이미 눌렀으면 다시 실행하지 않음
        if (clicked)
            return;

        // NetworkRunner 다시 확인
        if (runner == null)
        {
            runner = FindAnyObjectByType<NetworkRunner>();
        }

        if (runner == null)
        {
            Debug.LogWarning("Puzzle17Valve: NetworkRunner를 찾을 수 없습니다.");
            return;
        }

        // 이미 생성된 Puzzle17이 있는지 확인
        Puzzle17 puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Key);

        // 없다면 공유 프리팹 생성
        if (puzzle == null)
        {
            NetworkObject spawnedObject = runner.Spawn(puzzle17Prefab);
            puzzle = spawnedObject.GetComponent<Puzzle17>();
        }

        if (puzzle == null)
        {
            Debug.LogWarning("Puzzle17Valve: Puzzle17을 찾을 수 없습니다.");
            return;
        }

        // 3D 밸브가 눌렸다는 것을 공유
        puzzle.OpenValve();

        clicked = true;

        Debug.Log("Puzzle17Valve: 밸브 작동");
    }
}