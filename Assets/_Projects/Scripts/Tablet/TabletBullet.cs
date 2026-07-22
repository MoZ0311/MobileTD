using UnityEngine;

public class TabletBullet : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] ParticleSystem particle;

    Vector3 target;
    Vector3 direction;

    public void Initialize(Vector3 targetPoint)
    {
        target = targetPoint;
        direction = (target - transform.position).normalized;
    }

    void Update()
    {
        float moveDistance = moveSpeed * Time.deltaTime;

        if (Physics.SphereCast(
            transform.position,
            transform.lossyScale.x,
            direction,
            out RaycastHit hit,
            moveDistance))
        {
            Instantiate(particle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveDistance
        );

        if (Vector3.Distance(transform.position, target) < 0.02f)
        {
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
