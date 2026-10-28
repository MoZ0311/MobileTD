using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 遠距離攻撃を行う敵。
/// 狙撃ポイントがあれば向かい、到着後はその場から攻撃する。
/// ポイントまでの経路がなければタワーへ向かう。
/// </summary>
public class RangedEnemy : EnemyMove
{
    [Header("攻撃設定")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackInterval = 1f;

    [Header("狙撃ポイント")]
    private RangedAttackPointManager attackPointManager;

    private float nextAttackTime;

    private Vector3 attackPoint;
    private bool hasAttackPoint;
    private bool arrivedAtAttackPoint;

    private void Start()
    {
        // シーン上の狙撃ポイント管理クラスを取得
        attackPointManager = FindAnyObjectByType<RangedAttackPointManager>();

        if (target == null || agent == null)
        {
            return;
        }

        // 狙撃ポイントが設定されていなければタワーへ向かう
        if (attackPointManager == null)
        {
            MoveToTower();
            return;
        }

        // 経路のある狙撃ポイントを探して予約する
        if (attackPointManager.TryReserveAttackPoint(transform.position, this, out attackPoint))
        {
            hasAttackPoint = true;

            agent.isStopped = false;
            agent.SetDestination(attackPoint);
        }
        else
        {
            // ポイントがなければタワーへ向かう
            hasAttackPoint = false;
            MoveToTower();
        }
    }

    private void Update()
    {
        if (target == null || agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        // 狙撃ポイントを予約している場合
        if (hasAttackPoint)
        {
            // まだ到着していない場合
            if (!arrivedAtAttackPoint)
            {
                // 経路が計算中なら待つ
                if (agent.pathPending)
                {
                    return;
                }

                // 移動できなくなった場合はポイントを諦める
                if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    AbandonAttackPoint();
                    return;
                }

                // 狙撃ポイントへの到着を判定
                if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.05f && !agent.pathPending)
                {
                    //Debug.Log($"狙撃ポイント到着判定：現在位置={transform.position}, " + $"目的地={attackPoint}, " + $"残り距離={agent.remainingDistance}");

                    arrivedAtAttackPoint = true;

                    agent.isStopped = true;
                    agent.ResetPath();
                }
                else
                {
                    // まだ目的地に到着していないので移動を続ける
                    return;
                }
            }

            // 狙撃ポイントで停止
            agent.isStopped = true;

            // タワーの方向を向く
            LookAtTarget();
            AttackTower();

            return;
        }

        // 狙撃ポイントがない場合は通常どおりタワーへ向かう
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance <= attackRange && CanAttackTarget())
        {
            agent.isStopped = true;
            AttackTower();
            return;
        }

        agent.isStopped = false;

        // 経路がなければ目的地を再設定する
        if (!agent.pathPending && !agent.hasPath)
        {
            MoveToTower();
        }
    }

    /// <summary>
    /// 狙撃ポイントを諦めてタワーへ向かう。
    /// </summary>
    private void AbandonAttackPoint()
    {
        if (attackPointManager != null)
        {
            attackPointManager.ReleaseAttackPoint(this);
        }

        hasAttackPoint = false;
        arrivedAtAttackPoint = false;

        agent.isStopped = false;
        MoveToTower();
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

        if (distance <= 0f)
        {
            return true;
        }

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance))
        {
            return hit.transform == target.transform || hit.transform.IsChildOf(target.transform);
        }

        return false;
    }

    /// <summary>
    /// タワーを攻撃する。
    /// </summary>
    private void AttackTower()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackInterval;

        // TODO:
        // ここでタワーのHPを減らす処理を呼び出す。
        // attackDamage が攻撃ダメージ。
    }

    /// <summary>
    /// 敵が破棄される際に狙撃ポイントを解放する。
    /// </summary>
    private void OnDestroy()
    {
        if (attackPointManager != null)
        {
            attackPointManager.ReleaseAttackPoint(this);
        }
    }

    private void LookAtTarget()
    {
        if (target == null) return;

        // タワーへの方向を計算
        Vector3 direction = target.transform.position - transform.position;

        // 上下方向の差を無視する
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        // タワーの方向を計算
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Agentの回転速度で徐々に向きを変える
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
    }
}
