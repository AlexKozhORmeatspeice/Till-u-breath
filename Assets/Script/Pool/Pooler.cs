using System.Collections.Generic;
using UnityEngine;

public class Pooler : MonoBehaviour
{
    private Dictionary<PoolObj, Queue<GameObject>> PoolDictionary;
    [System.Serializable]
    public class Pool
    {
        public PoolObj name;
        public GameObject prefab;
        public int sizeOfPool;
    }

    public static Pooler Instance;
    
    public List<Pool> PoolObjects;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        PoolDictionary = new Dictionary<PoolObj, Queue<GameObject>>();

        foreach (Pool pool in PoolObjects)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            GameObject parentObj = Instantiate(new GameObject(), transform.position, Quaternion.identity);
            parentObj.name = pool.name + "s";
            
            for (int i = 0; i < pool.sizeOfPool; i++)
            {
                GameObject gm = Instantiate(pool.prefab);
                gm.SetActive(false);
                gm.transform.parent = parentObj.transform;

                
                objectPool.Enqueue(gm);
            }
            
            PoolDictionary.Add(pool.name, objectPool);
        }
        
    }

    public GameObject SpawnPoolObject(PoolObj objKye, Vector3 position, Quaternion quaternion)
    {
        if (!PoolDictionary.ContainsKey(objKye))
        {
            Debug.LogWarning($"Key {objKye.ToString()} doesn't exist in {PoolDictionary} ");
            return null;
        }
        GameObject spawnGM = PoolDictionary[objKye].Dequeue();
        
        spawnGM.SetActive(true);
        spawnGM.transform.position = position;
        spawnGM.transform.rotation = quaternion;

        IPooledObj pooledObj = spawnGM.GetComponent<IPooledObj>();
        
        
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }
        
        PoolDictionary[objKye].Enqueue(spawnGM);
        
        return spawnGM;
    }

    public void SetAllObjUnactive()
    {
        foreach ( Pool pool in PoolObjects)
        {
            for (int i = 0; i <= PoolDictionary[pool.name].Count; i++)
            {
                GameObject gm = PoolDictionary[pool.name].Dequeue();
                gm.SetActive(false);
                PoolDictionary[pool.name].Enqueue(gm);
            }
        }
    }
    
}
