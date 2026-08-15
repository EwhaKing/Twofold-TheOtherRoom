using System.Collections.Generic;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Rope 곡선을 따라 튜브 메시를 생성 (에셋의 RopeMesh 대체)
// 정점을 로컬 좌표로 만들어서 부모 스케일을 그대로 따라감
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rope))]
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class RopeTubeMesh : MonoBehaviour
{
    [Tooltip("줄 방향 분할 수. 곡선이 각져 보이면 올릴 것")]
    [SerializeField, Range(3, 40)] int lengthDivision = 12;
    [Tooltip("단면 분할 수")]
    [SerializeField, Range(3, 20)] int radialDivision = 8;
    [Tooltip("줄의 반지름. 로컬 단위라 부모 스케일을 따라감")]
    [SerializeField] float ropeWidth = 0.04f;
    [Tooltip("길이 1당 UV 반복 횟수")]
    [SerializeField] float tilingPerMeter = 1f;

    Rope rope;
    MeshFilter meshFilter;
    Mesh mesh;

    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<Vector3> normals = new List<Vector3>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly List<int> triangles = new List<int>();

    Vector3[] points;
    Vector3[] tangents;

    #region Lifecycle
    void OnEnable()
    {
        Cache();
        Subscribe();
        Rebuild();
    }

    void OnDisable()
    {
        Unsubscribe();
#if UNITY_EDITOR
        EditorApplication.delayCall -= Rebuild;
#endif
    }

    void OnValidate()
    {
        Cache();
        Subscribe();
        // OnValidate 안에서 메시를 만들면 Unity가 경고를 내므로 한 프레임 미룸
#if UNITY_EDITOR
        EditorApplication.delayCall += Rebuild;
#endif
    }

    void Update()
    {
        // 에디트 모드에서는 Rope.OnPointsChanged가 대신 호출해줌
        if (Application.isPlaying) Rebuild();
    }

    void OnDestroy()
    {
        Unsubscribe();
#if UNITY_EDITOR
        EditorApplication.delayCall -= Rebuild;
#endif
        if (mesh != null) DestroyImmediate(mesh);
    }

    void Cache()
    {
        if (!rope) rope = GetComponent<Rope>();
        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
    }

    void Subscribe()
    {
        Unsubscribe();
        if (rope != null) rope.OnPointsChanged += Rebuild;
    }

    void Unsubscribe()
    {
        if (rope != null) rope.OnPointsChanged -= Rebuild;
    }
    #endregion

    #region Mesh Generation
    [ContextMenu("메시 다시 생성")]
    void Rebuild()
    {
        if (this == null) return; // delayCall로 예약된 뒤 컴포넌트가 사라진 경우

        Cache();
        if (rope == null || meshFilter == null) return;
        if (rope.IsPrefab) return;
        if (rope.StartPoint == null || rope.EndPoint == null) return;

        int ringCount = lengthDivision + 1;
        if (points == null || points.Length != ringCount)
        {
            points = new Vector3[ringCount];
            tangents = new Vector3[ringCount];
        }

        // GetPointAt은 월드 좌표를 주므로 로컬로 변환해서 담음
        for (int i = 0; i < ringCount; i++)
        {
            points[i] = transform.InverseTransformPoint(rope.GetPointAt(i / (float)lengthDivision));
        }

        BuildTangents(ringCount);
        BuildVertices(ringCount);
        BuildTriangles(ringCount);
        Upload();
    }

    void BuildTangents(int ringCount)
    {
        Vector3 last = Vector3.forward;
        for (int i = 0; i < ringCount; i++)
        {
            Vector3 t = i < ringCount - 1 ? points[i + 1] - points[i] : points[i] - points[i - 1];
            t = t.sqrMagnitude > 1e-10f ? t.normalized : last; // 점이 겹치면 직전 접선 재사용
            tangents[i] = t;
            last = t;
        }
    }

    void BuildVertices(int ringCount)
    {
        vertices.Clear();
        normals.Clear();
        uvs.Clear();

        Vector3 normal = FirstNormal(tangents[0]);
        float traveled = 0f;

        for (int i = 0; i < ringCount; i++)
        {
            if (i > 0)
            {
                // 직전 링의 normal을 현재 접선에 수직인 평면으로 투영해 이어받음
                // 월드 up을 매번 기준으로 삼으면 수직 케이블에서 roll이 튀면서 줄이 꼬임
                normal -= tangents[i] * Vector3.Dot(normal, tangents[i]);
                normal = normal.sqrMagnitude > 1e-10f ? normal.normalized : FirstNormal(tangents[i]);

                traveled += Vector3.Distance(points[i - 1], points[i]);
            }

            AddRing(points[i], normal, Vector3.Cross(tangents[i], normal), traveled * tilingPerMeter);
        }

        AddCap(points[0], -tangents[0], 0f);
        AddCap(points[ringCount - 1], tangents[ringCount - 1], traveled * tilingPerMeter);
    }

    // 접선과 평행하지 않은 아무 수직 벡터. 첫 링의 기준으로만 쓰고 이후로는 이어받음
    Vector3 FirstNormal(Vector3 tangent)
    {
        Vector3 n = Vector3.Cross(tangent, Vector3.up);
        if (n.sqrMagnitude < 1e-10f) n = Vector3.Cross(tangent, Vector3.right);
        return n.normalized;
    }

    // UV 이음매 때문에 첫 정점을 끝에 한 번 더 넣으므로 radialDivision + 1개
    void AddRing(Vector3 center, Vector3 normal, Vector3 binormal, float v)
    {
        for (int j = 0; j <= radialDivision; j++)
        {
            float angle = j * Mathf.PI * 2f / radialDivision;
            Vector3 dir = Mathf.Cos(angle) * normal + Mathf.Sin(angle) * binormal;
            vertices.Add(center + dir * ropeWidth);
            normals.Add(dir);
            uvs.Add(new Vector2((float)j / radialDivision, v));
        }
    }

    // binormal을 facing 기준으로 잡으므로 양 끝 모두 바깥을 향함
    void AddCap(Vector3 center, Vector3 facing, float v)
    {
        Vector3 normal = FirstNormal(facing);
        Vector3 binormal = Vector3.Cross(facing, normal);

        vertices.Add(center);
        normals.Add(facing);
        uvs.Add(new Vector2(0.5f, v));

        for (int j = 0; j <= radialDivision; j++)
        {
            float angle = j * Mathf.PI * 2f / radialDivision;
            Vector3 dir = Mathf.Cos(angle) * normal + Mathf.Sin(angle) * binormal;
            vertices.Add(center + dir * ropeWidth);
            normals.Add(facing);
            uvs.Add(new Vector2((Mathf.Cos(angle) + 1f) * 0.5f, (Mathf.Sin(angle) + 1f) * 0.5f));
        }
    }

    void BuildTriangles(int ringCount)
    {
        triangles.Clear();

        int stride = radialDivision + 1;

        for (int i = 0; i < ringCount - 1; i++)
        {
            for (int j = 0; j < radialDivision; j++)
            {
                int cur = i * stride + j;
                int next = cur + 1;
                int nextRing = cur + stride;

                triangles.Add(cur);
                triangles.Add(next);
                triangles.Add(nextRing);

                triangles.Add(next);
                triangles.Add(nextRing + 1);
                triangles.Add(nextRing);
            }
        }

        int startCap = ringCount * stride;
        AddCapTriangles(startCap);
        AddCapTriangles(startCap + stride + 1);
    }

    void AddCapTriangles(int center)
    {
        for (int j = 0; j < radialDivision; j++)
        {
            triangles.Add(center);
            triangles.Add(center + j + 1);
            triangles.Add(center + j + 2);
        }
    }

    void Upload()
    {
        if (mesh == null)
        {
            // DontSave: 구운 메시가 씬 파일에 직렬화되는 것을 막음
            mesh = new Mesh { name = "RopeTube", hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        // SetVertices는 bounds를 갱신하지 않음. 빼먹으면 프러스텀 컬링으로 사라짐
        mesh.RecalculateBounds();

        if (meshFilter.sharedMesh != mesh) meshFilter.sharedMesh = mesh;
    }
    #endregion
}
