using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 遠距離敵が使用する狙撃ポイントを管理するクラス。
/// </summary>
public class RangedAttackPointManager : MonoBehaviour
{
    [Header("障害物")]
    [SerializeField] private List<BoxCollider> obstacles = new List<BoxCollider>();

    [Header("ポイント設定")]
    [SerializeField] private float pointSpacing = 2f;

    [Header("NavMesh設定")]
    [SerializeField] private float navMeshSampleDistance = 1f;

    // 狙撃ポイント一覧
    private readonly List<AttackPoint> attackPoints = new List<AttackPoint>();

    private class AttackPoint
    {
        public Vector3 Position;
        public BoxCollider Obstacle;
        public EnemyMove UsingEnemy;

        public AttackPoint(Vector3 position, BoxCollider obstacle)
        {
            Position = position;
            Obstacle = obstacle;
            UsingEnemy = null;
        }
    }

    private void Start()
    {
        GenerateAttackPoints();
    }

    /// <summary>
    /// 障害物の上面に狙撃ポイントを生成する。
    /// </summary>
    [ContextMenu("狙撃ポイントを生成")]
    private void GenerateAttackPoints()
    {
        attackPoints.Clear();

        foreach (BoxCollider obstacle in obstacles)
        {
            if (obstacle == null) continue;

            Bounds bounds = obstacle.bounds;

            float width = bounds.size.x;
            float depth = bounds.size.z;

            // 広い方向にポイントを並べる
            bool alongX = width >= depth;
            float length = alongX ? width : depth;

            int count = Mathf.Max(
                1,
                Mathf.FloorToInt(length / Mathf.Max(pointSpacing, 0.1f)) + 1
            );

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : (float)i / (count - 1);

                Vector3 position = bounds.center;
                position.y = bounds.max.y;

                // 障害物の端から少し内側にポイントを生成する
                float margin = 0.5f;

                if (alongX)
                {
                    float minX = bounds.min.x + margin;
                    float maxX = bounds.max.x - margin;

                    position.x = Mathf.Lerp(minX, maxX, t);
                }
                else
                {
                    float minZ = bounds.min.z + margin;
                    float maxZ = bounds.max.z - margin;

                    position.z = Mathf.Lerp(minZ, maxZ, t);
                }

                // ポイントの位置をSceneビューで確認する
                Debug.DrawLine(
                    position,
                    position + Vector3.up * 0.5f,
                    Color.cyan,
                    10f
                );

                attackPoints.Add(new AttackPoint(position, obstacle));
            }
        }

        //Debug.Log($"狙撃ポイント生成完了: {attackPoints.Count}個", this);
    }

    /// <summary>
    /// 移動可能な空き狙撃ポイントを探して予約する。
    /// 見つからなければfalse
    /// </summary>
    public bool TryReserveAttackPoint(Vector3 spawnPosition, EnemyMove enemy, out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        if (enemy == null) return false;

        AttackPoint nearestPoint = null;
        float nearestDistance = float.MaxValue;

        foreach (AttackPoint point in attackPoints)
        {
            if (point.Obstacle == null)
            {
                //Debug.Log("ポイント除外: 障害物がnull");
                continue;
            }

            if (point.UsingEnemy != null)
            {
                //Debug.Log("ポイント除外: 別の敵が予約済み");
                continue;
            }

            // 敵の位置がNavMesh上にあるか確認
            if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit startHit, 2f, NavMesh.AllAreas))
            {
                //Debug.LogWarning($"経路計算失敗: 敵の位置にNavMeshがありません。位置={spawnPosition}", enemy);

                continue;
            }

            // 狙撃ポイントの位置がNavMesh上にあるか確認
            if (!NavMesh.SamplePosition(
                point.Position,
                out NavMeshHit endHit,
                navMeshSampleDistance,
                NavMesh.AllAreas))
            {
                //Debug.LogWarning($"経路計算失敗: 狙撃ポイントにNavMeshがありません。位置={point.Position}", enemy);

                continue;
            }

            // 敵から狙撃ポイントまでの経路を計算
            NavMeshPath path = new NavMeshPath();

            bool pathCalculated = NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                path
            );

            if (!pathCalculated)
            {
                //Debug.LogWarning($"経路計算失敗: CalculatePathがfalse。始点={startHit.position}, 終点={endHit.position}", enemy);

                continue;
            }

            if (path.status != NavMeshPathStatus.PathComplete)
            {
                //Debug.LogWarning($"ポイント除外: 完全な経路がありません。状態={path.status}", enemy);

                continue;
            }

            //Debug.Log($"ポイント使用可能: {endHit.position}");

            float distance = Vector3.Distance(
                spawnPosition,
                endHit.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
                targetPosition = endHit.position;
            }
        }

        if (nearestPoint == null)
        {
            targetPosition = Vector3.zero;
            return false;
        }

        nearestPoint.UsingEnemy = enemy;

        //Debug.Log($"狙撃ポイント予約: {targetPosition}", enemy);

        return true;
    }

    /// <summary>
    /// 敵が狙撃ポイントを使い終わったときに予約を解除する。
    /// </summary>
    public void ReleaseAttackPoint(EnemyMove enemy)
    {
        if (enemy == null) return;

        foreach (AttackPoint point in attackPoints)
        {
            if (point.UsingEnemy != enemy) continue;

            point.UsingEnemy = null;

            //Debug.Log($"狙撃ポイント解放: {point.Position}", this);

            return;
        }
    }
}
