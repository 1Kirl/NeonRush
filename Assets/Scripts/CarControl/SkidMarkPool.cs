using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMarkPool : MonoBehaviour
{
    #region SerializeFied private variables
    [SerializeField] private GameObject skidMarkPrefab;
    [SerializeField] private int poolSize = 10;
    #endregion




    #region Private variables
    private Queue<GameObject> pool = new Queue<GameObject>();
    #endregion





    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(skidMarkPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetSkidMark()
    {
        GameObject obj = (pool.Count > 0) ? pool.Dequeue() : Instantiate(skidMarkPrefab);
        obj.SetActive(true);

        // initiate trails.
        var skidMark = obj.GetComponent<SkidMarkObj>();
        skidMark.ResetTrails();

        return obj;
    }

    public void ReturnSkidMark(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
