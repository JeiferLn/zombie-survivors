using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool expandable = true;
    }
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();
    }
    
    public void CreatePool(GameObject prefab, int size)
    {
        string poolKey = prefab.name;
        
        if (poolDictionary.ContainsKey(poolKey))
            return;
        
        Queue<GameObject> objectPool = new Queue<GameObject>();
        prefabDictionary[poolKey] = prefab;
        
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPool.Enqueue(obj);
        }
        
        poolDictionary[poolKey] = objectPool;
    }
    
    public GameObject GetObject(GameObject prefab)
    {
        return GetObject(prefab.name);
    }
    
    public GameObject GetObject(string poolKey)
    {
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Debug.LogWarning($"Pool with key {poolKey} doesn't exist!");
            return null;
        }
        
        GameObject objectToSpawn;
        
        if (poolDictionary[poolKey].Count > 0)
        {
            objectToSpawn = poolDictionary[poolKey].Dequeue();
        }
        else
        {
            // Expandir pool si está vacío
            objectToSpawn = Instantiate(prefabDictionary[poolKey]);
            objectToSpawn.transform.SetParent(transform);
        }
        
        objectToSpawn.SetActive(true);
        
        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        poolable?.OnGetFromPool();
        
        return objectToSpawn;
    }
    
    public void ReturnObject(GameObject objectToReturn)
    {
        string poolKey = objectToReturn.name.Replace("(Clone)", "").Trim();
        
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Destroy(objectToReturn);
            return;
        }
        
        IPoolable poolable = objectToReturn.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();
        
        objectToReturn.SetActive(false);
        poolDictionary[poolKey].Enqueue(objectToReturn);
    }
}

public interface IPoolable
{
    void OnGetFromPool();
    void OnReturnToPool();
}