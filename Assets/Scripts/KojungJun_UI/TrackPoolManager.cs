using System.Collections.Generic;
using UnityEngine;

public class TrackPoolManager : MonoBehaviour
{
    public static TrackPoolManager Instance;

    [System.Serializable]
    public class TrackPrefabEntry
    {
        public string trackName;
        public GameObject prefab;
        public int preloadCount = 0;
    }

    public List<TrackPrefabEntry> trackPrefabs;
    private Dictionary<string, Queue<GameObject>> poolDict = new();

    void Awake() {
        Instance = this;
        InitializePools();
    }

    public void InitializePools() {
        foreach (var entry in trackPrefabs) {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < entry.preloadCount; i++) {
                GameObject obj = Instantiate(entry.prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            poolDict[entry.trackName] = pool;
        }
    }

    public GameObject GetFromPool(string trackName, Vector3 position, Quaternion rotation) {
        // 1. 풀에 없으면 새로 생성
        if (!poolDict.ContainsKey(trackName)) {
            Debug.LogWarning($"[TrackPoolManager] trackName '{trackName}' not found in poolDict. Creating new pool.");
            poolDict[trackName] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = poolDict[trackName];

        GameObject obj;

        // 2. 풀에 남은 게 없으면 prefab에서 Instantiate
        if (pool.Count > 0) {
            obj = pool.Dequeue();
        }
        else {
            obj = Instantiate(GetPrefab(trackName));
            if (obj == null) {
                Debug.LogError($"[TrackPoolManager] prefab for '{trackName}' is null! Check spelling or registration.");
                return null;
            }
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }


    public void ReturnToPool(string trackName, GameObject obj) {
        obj.SetActive(false);
        poolDict[trackName].Enqueue(obj);
    }
    public void ClearAll() {
        foreach (var queue in poolDict.Values) {
            while (queue.Count > 0) {
                var obj = queue.Dequeue();
                Destroy(obj);
            }
        }

        poolDict.Clear();
    }

    GameObject GetPrefab(string trackName) {
        return trackPrefabs.Find(x => x.trackName == trackName)?.prefab;
    }
}
