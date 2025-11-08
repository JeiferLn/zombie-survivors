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
        public int size = 10;
        public bool expandable = true;
    }
    
    [Header("Pool Configuration")]
    public List<Pool> pools = new List<Pool>();
    
    [Header("Auto Create Enemy Pools")]
    public bool autoCreateEnemyPools = true;
    public GameObject enemyPrefabTemplate; // Prefab base para enemigos
    public int enemyPoolSize = 20;
    
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
            return;
        }
        
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();
        
        // Inicializar pools configurados
        InitializePools();
        
        // Auto crear pool de enemigos si está activado
        if (autoCreateEnemyPools && enemyPrefabTemplate == null)
        {
            CreateDefaultEnemyTemplate();
        }
    }
    
    void InitializePools()
    {
        foreach (Pool pool in pools)
        {
            if (pool.prefab == null) continue;
            
            CreatePool(pool.prefab, pool.size);
            Debug.Log($"Created pool: {pool.tag} with {pool.size} objects");
        }
    }
    
    void CreateDefaultEnemyTemplate()
    {
        enemyPrefabTemplate = new GameObject("EnemyTemplate");
        enemyPrefabTemplate.AddComponent<SpriteRenderer>();
        enemyPrefabTemplate.AddComponent<CircleCollider2D>().isTrigger = true;
        enemyPrefabTemplate.AddComponent<Enemy>();
        enemyPrefabTemplate.SetActive(false);
        enemyPrefabTemplate.transform.parent = transform;
        
        // Crear pool para enemigos genéricos
        CreatePool(enemyPrefabTemplate, enemyPoolSize);
        
        Debug.Log("Created default enemy template and pool");
    }
    
    public void CreatePool(GameObject prefab, int size)
    {
        string poolKey = prefab.name;
        
        if (poolDictionary.ContainsKey(poolKey))
        {
            Debug.Log($"Pool {poolKey} already exists");
            return;
        }
        
        Queue<GameObject> objectPool = new Queue<GameObject>();
        prefabDictionary[poolKey] = prefab;
        
        GameObject poolContainer = new GameObject($"Pool_{poolKey}");
        poolContainer.transform.parent = transform;
        
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer.transform);
            obj.name = $"{poolKey}_{i}";
            objectPool.Enqueue(obj);
        }
        
        poolDictionary[poolKey] = objectPool;
        Debug.Log($"Pool created: {poolKey} with {size} objects");
    }
    
    public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null)
        {
            // Usar template por defecto
            if (enemyPrefabTemplate != null)
                return GetObject(enemyPrefabTemplate.name);
            else
            {
                Debug.LogError("No prefab provided and no default template!");
                return null;
            }
        }
        
        return GetObject(prefab.name);
    }
    
    public GameObject GetObject(string poolKey)
    {
        // Si no existe el pool, intentar con el template de enemigo
        if (!poolDictionary.ContainsKey(poolKey))
        {
            if (enemyPrefabTemplate != null && poolDictionary.ContainsKey(enemyPrefabTemplate.name))
            {
                poolKey = enemyPrefabTemplate.name;
            }
            else
            {
                Debug.LogWarning($"Pool with key {poolKey} doesn't exist!");
                
                // Crear un objeto nuevo como fallback
                if (prefabDictionary.ContainsKey(poolKey))
                {
                    return Instantiate(prefabDictionary[poolKey]);
                }
                
                return null;
            }
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
            Debug.Log($"Pool {poolKey} expanded");
        }
        
        objectToSpawn.SetActive(true);
        
        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        poolable?.OnGetFromPool();
        
        return objectToSpawn;
    }
    
    public void ReturnObject(GameObject objectToReturn)
    {
        if (objectToReturn == null) return;
        
        string poolKey = objectToReturn.name.Replace("(Clone)", "").Trim();
        
        // Quitar números del final si los hay
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"_\d+$");
        poolKey = regex.Replace(poolKey, "");
        
        if (!poolDictionary.ContainsKey(poolKey))
        {
            // Intentar con el template de enemigo
            if (enemyPrefabTemplate != null)
            {
                poolKey = enemyPrefabTemplate.name;
            }
        }
        
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
    
    public void ClearPool(string poolKey)
    {
        if (poolDictionary.ContainsKey(poolKey))
        {
            while (poolDictionary[poolKey].Count > 0)
            {
                GameObject obj = poolDictionary[poolKey].Dequeue();
                Destroy(obj);
            }
        }
    }
    
    public void ClearAllPools()
    {
        foreach (var pool in poolDictionary)
        {
            ClearPool(pool.Key);
        }
        poolDictionary.Clear();
        prefabDictionary.Clear();
    }
}