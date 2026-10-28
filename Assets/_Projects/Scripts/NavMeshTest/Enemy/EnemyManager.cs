using UnityEngine;

/// <summary>
/// 敵の生成を管理するクラス。
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("敵Prefab")]
    [SerializeField] private GameObject ladderEnemyPrefab;
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;

    [Header("梯子ポイント管理")]
    [SerializeField] private LadderPointManager ladderPointManager;

    /// <summary>
    /// 梯子敵を生成する。
    /// </summary>
    [ContextMenu("梯子敵を生成")]
    private void SpawnLadderEnemy()
    {
        if (ladderPointManager == null)
        {
            Debug.LogError("EnemyManager: LadderPointManagerが未設定です。", this);
            return;
        }

        GameObject enemyObject = Instantiate(
            ladderEnemyPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        LadderEnemy enemy = enemyObject.GetComponent<LadderEnemy>();

        if (enemy == null)
        {
            Debug.LogError("梯子敵PrefabにLadderEnemyMoveがありません。", enemyObject);
            Destroy(enemyObject);
            return;
        }

        bool foundPoint = ladderPointManager.TryGetNearestAvailableLadderPoint(
            transform.position,
            enemy,
            out Vector3 targetPoint
        );

        if (!foundPoint)
        {
            Debug.Log("空いている梯子ポイントがありません。");

            Destroy(enemyObject);
            return;
        }

        enemy.Init(ladderPointManager, targetPoint);
    }

    /// <summary>
    /// 近距離敵を生成する。
    /// </summary>
    [ContextMenu("近距離敵を生成")]
    private void SpawnMeleeEnemy()
    {
        SpawnEnemy(meleeEnemyPrefab, typeof(MeleeEnemy));
    }

    /// <summary>
    /// 遠距離敵を生成する。
    /// </summary>
    [ContextMenu("遠距離敵を生成")]
    private void SpawnRangedEnemy()
    {
        SpawnEnemy(rangedEnemyPrefab, typeof(RangedEnemy));
    }

    /// <summary>
    /// 指定されたPrefabから敵を生成する。
    /// </summary>
    private void SpawnEnemy(GameObject prefab, System.Type enemyType)
    {
        if (prefab == null) return;

        GameObject enemyObject = Instantiate(prefab, transform.position, Quaternion.identity, transform);

        if (enemyObject.GetComponent(enemyType) == null)
        {
            Destroy(enemyObject);
        }
    }
}
