using UnityEngine;

public class TabletShoot : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] TabletBullet bulletPrefab;
    [Range(float.Epsilon, float.MaxValue)][SerializeField] float fireRate;
    [SerializeField] bool canRapidFire;
    [SerializeField] float maxDistance;
    [SerializeField] ParticleSystem hitEffect;

    [Header("Components")]
    [SerializeField] Camera arCamera;
    public bool isShooting;
    float fireRateTimer;

    void Update()
    {
        // 念のため0以上の範囲になるようにタイマーを減算
        fireRateTimer = Mathf.Max(0, fireRateTimer - Time.deltaTime);

        if (isShooting && fireRateTimer <= 0)
        {
            // 弾の発射処理
            ShootLazer();

            // 秒間FireRate発になるよう計算
            fireRateTimer = 1.0f / fireRate;

            // オート連射可能でなければ、都度射撃フラグを折る
            if (!canRapidFire)
            {
                isShooting = false;
            }
        }
    }

    void ShootLazer()
    {
        Vector3 targetPoint;

        Ray ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        targetPoint = ray.origin + ray.direction * maxDistance;

        Vector3 spawnPos = arCamera.transform.position;

        TabletBullet bullet = Instantiate(
            bulletPrefab,
            spawnPos,
            Quaternion.identity
        );

        bullet.Initialize(targetPoint);
    }
}
