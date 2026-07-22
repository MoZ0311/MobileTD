using UnityEngine;
using System.Collections;

public class TowerManager : MonoBehaviour
{
    [SerializeField] GameObject modelObject;
    [SerializeField] float moveSpeed;

    void Start()
    {
        StartCoroutine(SpawnTower());
    }

    IEnumerator SpawnTower()
    {
        while (modelObject.transform.localPosition.y <= 1.0)
        {
            modelObject.transform.position += moveSpeed * Time.deltaTime * Vector3.up;
            yield return null;
        }
        
    }
}
