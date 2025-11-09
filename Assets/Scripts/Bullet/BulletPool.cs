using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;        // Tag del pool (p.e: "PistolBullet")
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<BulletController>> poolDictionary;

    void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<BulletController>>();

        foreach (var pool in pools)
        {
            Queue<BulletController> bulletQueue = new Queue<BulletController>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject go = Instantiate(pool.prefab, transform);
                BulletController bullet = go.GetComponent<BulletController>();

                if (bullet == null)
                {
                    continue;
                }

                go.SetActive(false);
                bullet.SetPoolTag(pool.tag); // ASIGNAR TAG AQUÍ
                bulletQueue.Enqueue(bullet);
            }

            poolDictionary.Add(pool.tag, bulletQueue);
        }
    }

    public BulletController Get(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        BulletController bullet = poolDictionary[tag].Dequeue();

        bullet.gameObject.SetActive(true);

        // RE-ASIGNAR TAG para la vida útil actual
        bullet.SetPoolTag(tag);

        poolDictionary[tag].Enqueue(bullet);

        return bullet;
    }

    public void Return(string tag, BulletController bullet)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return;
        }

        bullet.gameObject.SetActive(false);
        poolDictionary[tag].Enqueue(bullet);
    }
}
