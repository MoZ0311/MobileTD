using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 梯子ポイントを管理するクラス。
/// </summary>
public class LadderPointManager : MonoBehaviour
{
    [Header("タワー")]
    [SerializeField] private GameObject tower;

    [Header("障害物")]
    [SerializeField] private List<BoxCollider> obstacles = new List<BoxCollider>();

    [Header("梯子ポイント")]
    [SerializeField] private float ladderPointSpacing = 1.5f;

    [Header("梯子")]
    [SerializeField] private GameObject ladderPrefab;
    [SerializeField] private Transform ladderParent;

    [Header("壁の表面から梯子を離す距離")]
    [SerializeField] private float ladderOffset = 0.05f;

    [Header("梯子制作")]
    [SerializeField] private float ladderBuildTime = 5f;

    // 計算した全ての梯子ポイント
    private List<LadderPoint> ladderPoints = new List<LadderPoint>();

    private class LadderPoint
    {
        // 梯子を設置するワールド座標
        public Vector3 Position { get; private set; }

        // このポイントの対象となる障害物
        public BoxCollider Obstacle { get; private set; }

        // 障害物の高さ
        public float ObstacleHeight { get; private set; }

        // 現在、このポイントを使用している敵
        public EnemyMove UsingEnemy { get; set; }

        // 梯子の制作進捗。0～1
        public float Progress { get; set; }

        // 梯子が完成しているか
        public bool IsCompleted { get; set; }

        // このポイントに生成された梯子
        public GameObject LadderObject { get; set; }

        /// <summary>
        /// 梯子ポイントを生成する。
        /// </summary>
        public LadderPoint(Vector3 position, BoxCollider obstacle)
        {
            Position = position;
            Obstacle = obstacle;

            // 障害物のワールド空間での高さを取得する
            ObstacleHeight = obstacle.bounds.size.y;

            // 初期状態では誰も使用していない
            UsingEnemy = null;

            // 制作進捗は0%
            Progress = 0f;

            // 梯子は未完成
            IsCompleted = false;

            // 梯子オブジェクトはまだ存在しない
            LadderObject = null;
        }
    }

    /// ゲーム開始時に梯子ポイントを計算
    private void Start()
    {
        CalculateAllLadderPoints();
    }

    // 全ての障害物から梯子ポイントを計算
    [ContextMenu("梯子ポイントを計算")]
    private void CalculateAllLadderPoints()
    {
        ladderPoints.Clear();

        if (tower == null)
        {
            Debug.LogError("LadderPointManager: Towerが設定されていません", this);

            return;
        }

        foreach (BoxCollider obstacle in obstacles)
        {
            if (obstacle == null) continue;

            // 障害物のポイントを計算
            List<Vector3> positions = CalculateLadderPoints(obstacle);

            // 計算した位置をLadderPointとして登録
            foreach (Vector3 position in positions)
            {
                ladderPoints.Add(new LadderPoint(position, obstacle));
            }
        }

        // Sceneビューでポイントの位置を確認できるようにする
        for (int i = 0; i < ladderPoints.Count; i++)
        {
            Debug.DrawLine(ladderPoints[i].Position, ladderPoints[i].Position + Vector3.up * 0.5f, GetPointColor(i), 10f);
        }
    }

    /// 障害物1つ分の梯子ポイントを計算(タワーと反対側の面だけ)
    private List<Vector3> CalculateLadderPoints(BoxCollider obstacle)
    {
        Transform obstacleTransform = obstacle.transform;

        Vector3 center = obstacle.center;
        Vector3 size = obstacle.size;

        // タワーの位置を障害物のローカル座標に変換する
        Vector3 localTowerPosition = obstacleTransform.InverseTransformPoint(tower.transform.position);

        // タワーから見て、どちら側の面が近いかを判定する
        float xDistance = Mathf.Abs(localTowerPosition.x - center.x);
        float zDistance = Mathf.Abs(localTowerPosition.z - center.z);
        bool useXFace = xDistance > zDistance;

        // 選択した面の幅を取得する
        float width;

        if (useXFace)
        {
            // X方向の面なので、幅はZ方向
            width = obstacleTransform.TransformVector(Vector3.forward * size.z).magnitude;
        }
        else
        {
            // Z方向の面なので、幅はX方向
            width = obstacleTransform.TransformVector(Vector3.right * size.x).magnitude;
        }

        // 障害物の幅に応じてポイント数を決定する
        int pointCount = Mathf.Max(1, Mathf.FloorToInt(width / Mathf.Max(ladderPointSpacing, 0.01f)) + 1);

        return CreatePoints(obstacle, pointCount, useXFace);
    }

    // 指定された面に梯子ポイントを配置する。
    private List<Vector3> CreatePoints(BoxCollider obstacle, int pointCount, bool useXFace)
    {
        Transform obstacleTransform = obstacle.transform;

        Vector3 center = obstacle.center;
        Vector3 size = obstacle.size;

        Vector3 localTowerPosition = obstacleTransform.InverseTransformPoint(tower.transform.position);

        List<Vector3> points = new List<Vector3>();

        // 使用する面の幅の80%にポイントを配置する
        const float widthRatio = 0.8f;

        if (useXFace)
        {
            // X方向の面
            // タワーと反対側のX座標を取得する
            float x;

            if (localTowerPosition.x > center.x)
            {
                x = center.x - size.x * 0.5f;
            }
            else
            {
                x = center.x + size.x * 0.5f;
            }

            // ポイントを配置するZ方向の範囲
            float minZ = center.z - size.z * 0.5f * widthRatio;
            float maxZ = center.z + size.z * 0.5f * widthRatio;

            for (int i = 0; i < pointCount; i++)
            {
                // ポイントが1つなら中央に配置する
                float t = pointCount == 1 ? 0.5f : (float)i / (pointCount - 1);
                float z = Mathf.Lerp(minZ, maxZ, t);

                // ローカル座標からワールド座標に変換する
                Vector3 position = obstacleTransform.TransformPoint(new Vector3(x, center.y, z));

                points.Add(position);
            }
        }
        else
        {
            // Z方向の面
            // タワーと反対側のZ座標を取得する
            float z;

            if (localTowerPosition.z > center.z)
            {
                z = center.z - size.z * 0.5f;
            }
            else
            {
                z = center.z + size.z * 0.5f;
            }

            // ポイントを配置するX方向の範囲
            float minX = center.x - size.x * 0.5f * widthRatio;
            float maxX = center.x + size.x * 0.5f * widthRatio;

            for (int i = 0; i < pointCount; i++)
            {
                float t = pointCount == 1 ? 0.5f : (float)i / (pointCount - 1);

                float x = Mathf.Lerp(minX, maxX, t);

                Vector3 position = obstacleTransform.TransformPoint(new Vector3(x, center.y, z));

                points.Add(position);
            }
        }

        return points;
    }

    /// <summary>
    /// 指定された位置から最も近い空きポイントを予約(制作中のポイントと完成済みのポイントは除外)
    /// </summary>
    public bool TryGetNearestAvailableLadderPoint(Vector3 spawnPosition, EnemyMove enemy, out Vector3 targetPoint)
    {
        targetPoint = Vector3.zero;

        LadderPoint nearestPoint = null;

        float nearestDistance = float.MaxValue;

        foreach (LadderPoint point in ladderPoints)
        {
            // 完成済みのポイントは使用しない
            if (point.IsCompleted) continue;

            // 別の敵が制作中なら使用しない
            if (point.UsingEnemy != null) continue;

            // 敵の出現位置からの距離を計算する
            float distance = Vector3.Distance(spawnPosition, point.Position);

            // 最も近いポイントを記録する
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
            }
        }

        // 空いているポイントが見つからなかった
        if (nearestPoint == null) return false;

        // ポイントを予約する
        nearestPoint.UsingEnemy = enemy;

        targetPoint = nearestPoint.Position;

        return true;
    }

    /// <summary>
    /// 指定された敵に梯子を制作させる
    /// </summary>
    public bool BuildLadder(EnemyMove enemy, float deltaTime)
    {
        // この敵が担当しているポイントを探す
        foreach (LadderPoint point in ladderPoints)
        {
            if (point.UsingEnemy != enemy) continue;

            // 完成済みなら制作完了とする
            if (point.IsCompleted) return true;

            // 梯子Prefabが設定されていない場合は制作しない
            if (ladderPrefab == null)
            {
                Debug.LogError("LadderPointManager: Ladder Prefabが未設定です。", this);

                return false;
            }

            // 梯子がまだ存在しなければ生成する
            CreateLadderIfNeeded(point);

            if (point.LadderObject == null) return false;

            // 制作進捗を更新する
            float buildTime = Mathf.Max(ladderBuildTime, 0.01f);

            point.Progress += deltaTime / buildTime;

            // 進捗を0～1の範囲に収める
            point.Progress = Mathf.Clamp01(point.Progress);

            // 進捗に合わせて梯子の見た目を更新する
            UpdateLadderVisual(point);

            // 制作が完了したらポイントを解放する
            if (point.Progress >= 1f)
            {
                point.Progress = 1f;
                point.IsCompleted = true;

                // はしご完成時にNavMeshLinkを有効化
                LadderVisual visual = point.LadderObject.GetComponent<LadderVisual>();

                if (visual != null)
                {
                    visual.CompleteLadder();
                }

                point.UsingEnemy = null;

                UpdateLadderVisual(point);

                //Debug.Log($"梯子完成: {point.Position}");

                enemy.Die();

                return true;
            }

            return false;
        }

        // この敵が担当しているポイントがない
        return false;
    }

    // 梯子オブジェクトがなければ生成する
    private void CreateLadderIfNeeded(LadderPoint point)
    {
        // すでに梯子が生成されていれば何もしない
        if (point.LadderObject != null) return;

        // 梯子の生成位置
        Vector3 ladderPosition = point.Position;

        // 障害物の底面に合わせる
        ladderPosition.y = point.Obstacle.bounds.min.y;

        // 障害物のローカル座標でタワーの位置を取得
        Vector3 localTowerPosition = point.Obstacle.transform.InverseTransformPoint(tower.transform.position);

        // 障害物の中心とサイズを取得
        Vector3 center = point.Obstacle.center;

        // タワーと反対側の面を判定する
        bool useXFace = Mathf.Abs(localTowerPosition.x - center.x) > Mathf.Abs(localTowerPosition.z - center.z);

        // 壁の面に対して垂直な方向を取得
        Vector3 direction;

        if (useXFace)
        {
            // X方向の面なら、X軸方向に移動する
            direction = point.Obstacle.transform.right * (localTowerPosition.x > center.x ? -1f : 1f);
        }
        else
        {
            // Z方向の面なら、Z軸方向に移動する
            direction = point.Obstacle.transform.forward * (localTowerPosition.z > center.z ? -1f : 1f);
        }

        // 壁の表面から少し離す
        ladderPosition += direction * ladderOffset;

        // 梯子を生成する
        point.LadderObject = Instantiate(ladderPrefab, ladderPosition, Quaternion.identity, ladderParent);

        // 梯子の見た目を管理するコンポーネントを取得
        LadderVisual visual = point.LadderObject.GetComponent<LadderVisual>();

        if (visual == null)
        {
            Debug.LogError("Ladder PrefabにLadderVisualがありません。", point.LadderObject);

            Destroy(point.LadderObject);
            point.LadderObject = null;

            return;
        }

        // 生成直後はほぼ見えない高さにする
        visual.SetHeight(0.01f);
    }

    // 制作進捗に合わせて梯子の高さを変更する。
    private void UpdateLadderVisual(LadderPoint point)
    {
        if (point.LadderObject == null) return;

        LadderVisual visual = point.LadderObject.GetComponent<LadderVisual>();

        if (visual == null) return;

        // 完成時は障害物と同じ高さになる
        float targetHeight = point.ObstacleHeight * point.Progress;

        visual.SetHeight(targetHeight);
    }

    /// <summary>
    /// 敵が死亡したときに担当ポイントを解放する。
    /// </summary>
    public void ReleaseLadderPoint(EnemyMove enemy)
    {
        foreach (LadderPoint point in ladderPoints)
        {
            if (point.UsingEnemy != enemy) continue;

            // 完成していない場合は担当だけを解除する
            if (!point.IsCompleted)
            {
                point.UsingEnemy = null;

                Debug.Log($"梯子制作中の敵が死亡。 " + $"進捗を保持: {point.Progress * 100f:F1}%");
            }

            // LadderObjectとProgressは変更しない
            return;
        }
    }

    // Sceneビューでポイントを見分けるための色を返す(デバック用)
    private Color GetPointColor(int index)
    {
        switch (index % 3)
        {
            case 0:
                return Color.green;

            case 1:
                return Color.yellow;

            default:
                return Color.blue;
        }
    }
}
