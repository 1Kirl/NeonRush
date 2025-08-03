using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private int initialPoolSize = 4;

    private Queue<GameObject> pool = new Queue<GameObject>();

    public static FloorPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 풀 미리 채워두기
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(floorPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetFloor(Vector3 position)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(floorPrefab);
        }

        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnFloor(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
