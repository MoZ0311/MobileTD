using UnityEngine;
using UnityEngine.InputSystem;

public class SiegePlayer : MonoBehaviour
{
    [Header("クラッシュ速度")]
    [SerializeField] private float crashSpeed;

    [Header("下フリック判定距離")]
    [SerializeField] private float attackFlickDistance = -100f;

    private float currentSpeed;     // 現在の移動速度
    private float currentHeading;   // 現在の方角(0 = 北 / 90 = 東 / 180 = 南 / 270 = 西)

    private Vector3 lastAcceleration;   // 前フレームの加速度
    private bool isDead;    // 死亡状態(true = 操作不可 / false = 操作可能)

    private Vector2 currentPointerPos;  // 現在のポインター座標
    private Vector2 pressStartPos;      // タッチ開始時の座標

    public float CurrentSpeed => currentSpeed;
    public float CurrentHeading => currentHeading;

    private void Start()
    {
        // コンパス（方位センサー）を有効化
        Input.compass.enabled = true;

        // 初期加速度を保存
        lastAcceleration = Input.acceleration;
    }

    void Update()
    {
        // 死亡していなければクラッシュ速度チェック
        if (!isDead)
        {
            CheckCrash();
        }

        UpdateHeading();
        UpdateSpeed();
    }

    // ポインター座標取得
    private void OnPoint(InputValue value)
    {
        currentPointerPos = value.Get<Vector2>();
    }

    // タッチ開始 / 終了取得
    private void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            // タッチ開始位置を記録
            pressStartPos = currentPointerPos;
        }
        else
        {
            // 指を離した時にフリック判定
            CheckAttackFlick();
        }
    }

    // 攻撃用下フリック判定
    private void CheckAttackFlick()
    {
        if (isDead) return;

        // タッチ開始位置から終了位置までの移動量
        Vector2 delta = currentPointerPos - pressStartPos;

        // 下方向へ一定距離以上
        if (delta.y <= attackFlickDistance)
        {
            Attack();
        }
    }

    // 攻撃処理
    private void Attack()
    {
        if (isDead) return;

        Debug.Log("攻撃");
    }

    // 死亡処理
    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("死亡");
    }

    // 復活処理
    private void Respawn()
    {
        if (!isDead) return;

        isDead = false;

        Debug.Log("復活");
    }

    // クラッシュ速度チェック処理
    private void CheckCrash()
    {
        if (currentSpeed > crashSpeed)
        {
            Debug.Log("クラッシュ速度を超えた");
            Die();
        }
    }

    // 現在の方角を取得
    private void UpdateHeading()
    {
        currentHeading = Input.compass.trueHeading;
    }

    // スマホの移動速度を取得
    private void UpdateSpeed()
    {
        // 現在の加速度センサーの値を取得
        Vector3 currentAcceleration = Input.acceleration;

        // 加速度の変化量から移動の勢いを取得
        Vector3 deltaAcceleration = currentAcceleration - lastAcceleration;

        // X・Y方向の勢いから移動速度を計算(G の変化量)
        currentSpeed = new Vector2(deltaAcceleration.x, deltaAcceleration.y).magnitude;

        // 次のフレームで比較するため、現在の加速度を保存
        lastAcceleration = currentAcceleration;
    }
}
