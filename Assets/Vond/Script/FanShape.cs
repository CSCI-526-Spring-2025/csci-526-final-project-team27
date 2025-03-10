using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SectorMesh : MonoBehaviour
{
    [Header("扇形参数")]
    public float radius = 1f;      // 扇形半径
    public float angle = 90f;      // 扇形角度（单位：度）
    public int segments = 20;      // 分段数，越高越平滑

    private Mesh mesh;

    void Awake()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        // 顶点数量：中心点 + 弧上点（segments+1个）
        Vector3[] vertices = new Vector3[segments + 2];
        // 三角形索引数组，每个三角形3个索引，总共segments个三角形
        int[] triangles = new int[segments * 3];

        // 设置中心顶点
        vertices[0] = Vector3.zero;

        // 计算弧上顶点
        float angleStep = angle / segments;
        // 为使扇形居中显示，这里将扇形从 -angle/2 开始
        float startAngle = -angle / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
        }

        // 构建三角形索引，依次连接中心点与相邻弧形点
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;          // 中心顶点
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // 设置网格数据
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 将生成的网格赋值给MeshFilter
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;
    }
}
