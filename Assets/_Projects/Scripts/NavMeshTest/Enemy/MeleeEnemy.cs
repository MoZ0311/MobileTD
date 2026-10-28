using UnityEngine;

/// <summary>
/// 近距離攻撃を行う敵の動作を管理するクラス。
/// </summary>
public class MeleeEnemy : EnemyMove
{
    [Header("近距離攻撃")]
    [SerializeField] private float attackRange = 1f;

    private void Start()
    {
        if (target == null) return;

        MoveToTower();
    }

    private void Update()
    {
        if (target == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        // 攻撃範囲内かつ、タワーとの間に障害物がなければ停止する
        if (distance <= attackRange && CanAttackTarget())
        {
            agent.isStopped = true;
            return;
        }

        // 攻撃できない場合は移動する
        agent.isStopped = false;

        // 経路がなければ目的地を再設定する
        if (!agent.pathPending && !agent.hasPath)
        {
            MoveToTower();
        }
    }

    /// <summary>
    /// タワーとの間に障害物がないか判定する。
    /// </summary>
    private bool CanAttackTarget()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 destination = target.transform.position + Vector3.up * 0.5f;

        Vector3 direction = destination - origin;
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance))
        {
            // 最初に当たったオブジェクトがタワーなら攻撃可能
            return hit.transform == target.transform;
        }

        // 何にも当たらなかった場合は攻撃不可
        return false;
    }
}
