using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    GameObject target;
    [SerializeField] private float speed = 1.0f;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Tower");
        if (target == null)
        {
            Debug.Log("era-");
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), speed * Time.deltaTime);
    }
}
