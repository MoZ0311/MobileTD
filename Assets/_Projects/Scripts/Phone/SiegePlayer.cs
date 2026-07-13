using UnityEngine;
using UnityEngine.InputSystem;

public class SiegePlayer : MonoBehaviour
{
    [SerializeField] PhoneUIController phoneUIController;

    [Header("クラッシュ加速度")]
    [SerializeField] public float crashAcceleration = 0.5f;

    [Header("攻撃可能加速度")]
    [SerializeField] private float attackAcceleration = 0.1f;

    [Header("下フリック判定距離")]
    [SerializeField] private float attackFlickDistance = -500f;

    private float currentAcceleration;  // 現在の加速度
    private float maxAcceleration;      // 最大加速度
    private float currentHeading;       // 現在の方角

    private bool isDead;    // 死亡状態(true = 操作不可 / false = 操作可能)

    private Vector2 pressStartPos;      // タッチ開始時の座標

    public float CurrentAcceleration => currentAcceleration;
    public float MaxAcceleration => maxAcceleration;
    public float CurrentHeading => currentHeading;

    private void Start()
    {
        // 線形加速度センサーを有効化
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.EnableDevice(LinearAccelerationSensor.current);
        }

        // 姿勢センサーを有効化
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }
    }

    void Update()
    {
        // 死亡していなければクラッシュ加速度チェック
        if (!isDead)
        {
            CheckCrash();
        }

        // 現在の方角を更新
        UpdateHeading();

        // 現在の加速度を更新
        UpdateAcceleration();
    }

    // タッチ開始 / 終了取得
    private void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            // タッチ開始位置を記録
            pressStartPos = Pointer.current.position.ReadValue();
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

        // 移動中は攻撃できない
        if (currentAcceleration > attackAcceleration)
        {
            phoneUIController.AddLog("スマホを動かしているため攻撃不可");
            return;
        }

        // タッチ開始位置から終了位置までの移動量
        Vector2 delta = Pointer.current.position.ReadValue() - pressStartPos;

        phoneUIController.AddLog("下フリック : " + delta.y);

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

        phoneUIController.AddLog("攻撃");
    }

    // 死亡処理
    private void Die()
    {
        if (isDead) return;

        isDead = true;

        phoneUIController.AddLog("死亡");
    }

    // 復活処理
    public void Respawn()
    {
        if (!isDead) return;

        isDead = false;

        phoneUIController.AddLog("復活");
    }

    // クラッシュ加速度チェック処理
    private void CheckCrash()
    {
        if (currentAcceleration > crashAcceleration)
        {
            phoneUIController.AddLog("クラッシュ加速度を超えた");
            Die();
        }
    }

    // 現在の方角を取得
    private void UpdateHeading()
    {
        if (AttitudeSensor.current == null) return;

        // 現在の姿勢を取得
        Quaternion quaternion = AttitudeSensor.current.attitude.ReadValue();

        // Z軸回転から方角を取得
        currentHeading = quaternion.eulerAngles.z;
    }

    // スマホの加速度を取得
    private void UpdateAcceleration()
    {
        if (LinearAccelerationSensor.current == null) return;

        // センサーから線形加速度を取得 (m/s²)
        Vector3 acceleration = LinearAccelerationSensor.current.acceleration.ReadValue();

        // X・Y方向の加速度から加速度の大きさを取得
        currentAcceleration = new Vector2(acceleration.x, acceleration.y).magnitude;

        // 最大加速度を更新
        if (maxAcceleration < currentAcceleration)
        {
            maxAcceleration = currentAcceleration;
        }
    }
}
