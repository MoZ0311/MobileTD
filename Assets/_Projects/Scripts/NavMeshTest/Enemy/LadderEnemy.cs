using UnityEngine;

/// <summary>
/// 梯子を制作する敵の動作を管理するクラス。
/// </summary>
public class LadderEnemy : EnemyMove
{
    [Header("梯子移動")]
    [SerializeField] private float ladderMoveSpeed = 1f;

    [SerializeField] private float attackRange = 1f;

    // 梯子ポイントの管理クラス
    private LadderPointManager ladderPointManager;

    // 梯子敵が向かう目的地
    private Vector3 ladderTarget;

    // 梯子を制作中か
    private bool isBuildingLadder;

    // 初期化が完了しているか
    private bool isInitialized;

    /// <summary>
    /// 梯子敵を初期化する。
    /// </summary>
    public void Init(LadderPointManager manager, Vector3 targetPoint)
    {
        ladderPointManager = manager;
        ladderTarget = targetPoint;

        // 親クラスのAwakeで取得済みのagentを使用する
        if (agent == null)
        {
            Debug.LogError("LadderEnemyMove: NavMeshAgentがありません。", this);
            return;
        }

        agent.speed = speed;

        // 梯子ポイントへ移動する
        agent.SetDestination(ladderTarget);

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 梯子の制作中なら進捗を更新する
        if (isBuildingLadder)
        {
            BuildLadder();
            return;
        }

        if (agent == null) return;

        float distance = Vector3.Distance(transform.position, ladderTarget);

        // 目的地に到着したら梯子の制作を開始する
        if (distance <= attackRange)
        {
            StartBuildingLadder();
        }

        // 梯子の上を移動する場合の速度を変更する
        if (agent.isOnOffMeshLink)
        {
            agent.speed = ladderMoveSpeed;
        }
        else
        {
            agent.speed = speed;
        }
    }

    /// <summary>
    /// 梯子の制作を開始する。
    /// </summary>
    private void StartBuildingLadder()
    {
        if (agent == null) return;

        isBuildingLadder = true;

        // 制作中は移動しない
        agent.isStopped = true;
    }

    /// <summary>
    /// 梯子の制作を進める。
    /// </summary>
    private void BuildLadder()
    {
        if (ladderPointManager == null) return;

        bool isCompleted = ladderPointManager.BuildLadder(this, Time.deltaTime);

        if (isCompleted)
        {
            isBuildingLadder = false;
        }
    }

    /// <summary>
    /// 死亡時に担当している梯子ポイントを解放する。
    /// </summary>
    public override void Die()
    {
        if (ladderPointManager != null)
        {
            ladderPointManager.ReleaseLadderPoint(this);
        }

        base.Die();
    }
}
