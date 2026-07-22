using UnityEngine;

public class PlaneGenerator : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Vector3[] points = new Vector3[4];
    [SerializeField] bool isDebug = false;

    void Start()
    {
        if (isDebug)
        {
            GenerateFlatPlane(points);
        }
    }

    /// <summary>
    /// 4つの座標から最も高いY座標を取得し、地面と平行なメッシュを作成します。
    /// </summary>
    public void GenerateFlatPlane(Vector3[] points)
    {
        if (points == null || points.Length != 4)
        {
            Debug.LogError("4つの座標が指定されていません。Vector3[4]を渡してください。");
            return;
        }

        // 最も高いY座標（高さ）を見つける
        float maxY = points[0].y;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].y > maxY)
            {
                maxY = points[i].y;
            }
        }

        // Y座標を最大値(maxY)に揃えた新しい頂点配列を作る
        Vector3[] flatVertices = new Vector3[4];
        for (int i = 0; i < 4; ++i)
        {
            flatVertices[i] = new Vector3(points[i].x, maxY, points[i].z);
        }

        // 三角形を形成するインデックス
        int[] triangles = new int[6]
        {
            0, 1, 2, // 三角形1
            0, 2, 3  // 三角形2
        };

        Vector2[] uvs = new Vector2[4]
        {
            new(0, 0),
            new(0, 1),
            new(1, 1),
            new(1, 0)
        };

        // メッシュの作成と割り当て
        Mesh mesh = new()
        {
            name = "HighestFlatPlane",
            vertices = flatVertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// メッシュ全体の面積（平方メートル）を計算して返します。
    /// </summary>
    /// <param name="mesh">計算対象のメッシュ</param>
    /// <returns>総面積</returns>
    public float CalculateMeshArea(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        float totalArea = 0f;

        // triangles配列は3個ずつ（頂点3つで三角形1つ）入っているため、3ずつ進めます
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // 三角形の3つの頂点座標を取得
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];

            // 三角形1つの面積を計算して加算
            totalArea += CalculateTriangleArea(p1, p2, p3);
        }

        return totalArea;
    }

    /// <summary>
    /// 3つの頂点から三角形1つの面積を求める（ベクトルの外積を使用）
    /// </summary>
    float CalculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // p1からp2へ向かうベクトルAと、p1からp3へ向かうベクトルBを作成
        Vector3 sideA = p2 - p1;
        Vector3 sideB = p3 - p1;

        // ベクトルの外積（Vector3.Cross）を計算し、そのベクトルの長さ（magnitude）を半分にすると三角形の面積になる
        Vector3 crossProduct = Vector3.Cross(sideA, sideB);
        float area = crossProduct.magnitude / 2;

        return area;
    }
}
