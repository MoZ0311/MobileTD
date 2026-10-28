using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 全ての敵に共通する動作を管理する親クラス。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyMove : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] protected float speed = 1f;

    [Header("タワーのレイヤー")]
    [SerializeField] protected LayerMask towerLayer;

    // NavMeshによる移動を管理するコンポーネント
    protected NavMeshAgent agent;

    // 攻撃対象のタワー
    protected GameObject target;

    /// <summary>
    /// 敵の初期化を行う。
    /// </summary>
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = speed;

        target = GameObject.FindGameObjectWithTag("Tower");
    }

    /// <summary>
    /// タワーに向かう方向と距離を取得する。
    /// </summary>
    protected bool TryGetTowerDirection(out Vector3 direction, out float distance)
    {
        direction = Vector3.zero;
        distance = 0f;

        if (target == null)
        {
            Debug.LogError("EnemyMove: Towerタグのオブジェクトが見つかりません。", this);
            return false;
        }

        direction = target.transform.position - transform.position;
        distance = direction.magnitude;

        return true;
    }

    /// <summary>
    /// タワーに向かって移動する。
    /// </summary>
    protected void MoveToTower()
    {
        if (!TryGetTowerDirection(out Vector3 direction, out float distance))
        {
            return;
        }

        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, distance, towerLayer))
        {
            agent.SetDestination(hit.point);
        }
        else
        {
            Debug.Log("タワーに当たっていません。", this);
        }
    }

    /// <summary>
    /// 敵を死亡させる。
    /// </summary>
    [ContextMenu("死亡")]
    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
