using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Configuration")] public EnemyDatabase enemyDatabase;
    public Transform[] spawnPoints;
    public float spawnAreaRadius = 20f;

    [Header("Wave Settings")] public int currentWave = 0;
    public bool autoStartWaves = true;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Settings")] public bool spawnOutsideCamera = true;
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 25f;

    [Header("Performance")] public int maxActiveEnemies = 100;
    public int enemiesPerFrame = 2; // Enemigos a procesar por frame

    [Header("Runtime")] private List<Enemy> activeEnemies = new List<Enemy>();
    private int enemiesRemainingToSpawn;
    private int enemiesKilledThisWave;
    private bool waveInProgress;
    private Coroutine currentWaveCoroutine;

    [Header("Events")] public UnityEvent<int> onWaveStart;
    public UnityEvent<int> onWaveComplete;
    public UnityEvent<int> onEnemyKilled;

    private void Awake()
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
    }

    private void Start()
    {
        // Inicializar pool de objetos
        InitializeObjectPools();

        if (autoStartWaves)
        {
            StartNextWave();
        }
    }

    private void InitializeObjectPools()
    {
        // Pre-crear enemigos en el pool
        foreach (var enemyInfo in enemyDatabase.enemyPool)
        {
            GameObject enemyPrefab = new GameObject($"Enemy_{enemyInfo.enemyType.enemyName}");
            Enemy enemy = enemyPrefab.AddComponent<Enemy>();
            SpriteRenderer sr = enemyPrefab.AddComponent<SpriteRenderer>();
            CircleCollider2D col = enemyPrefab.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            ObjectPoolManager.Instance.CreatePool(enemyPrefab, 20);
            Destroy(enemyPrefab);
        }
    }

    public void StartNextWave()
    {
        if (waveInProgress) return;

        currentWave++;
        waveInProgress = true;
        enemiesKilledThisWave = 0;

        // Calcular enemigos para esta oleada
        int enemiesToSpawn = CalculateEnemiesForWave();
        enemiesRemainingToSpawn = enemiesToSpawn;

        onWaveStart?.Invoke(currentWave);

        // Mostrar UI de oleada
        ShowWaveStartUI();

        currentWaveCoroutine = StartCoroutine(SpawnWave(enemiesToSpawn));
    }

    private int CalculateEnemiesForWave()
    {
        if (enemyDatabase.predefinedWaves != null &&
            currentWave <= enemyDatabase.predefinedWaves.Count)
        {
            return enemyDatabase.predefinedWaves[currentWave - 1].enemyCount;
        }
        else if (enemyDatabase.useInfiniteMode)
        {
            return enemyDatabase.baseEnemiesPerWave +
                   (enemyDatabase.enemiesIncreasePerWave * (currentWave - 1));
        }

        return 10; // Default
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        yield return new WaitForSeconds(2f); // Delay inicial

        float spawnDelay = 0.5f;

        for (int i = 0; i < enemyCount; i++)
        {
            // Verificar límite de enemigos activos
            while (activeEnemies.Count >= maxActiveEnemies)
            {
                yield return new WaitForSeconds(0.5f);
                CleanupDeadEnemies();
            }

            SpawnEnemy();
            enemiesRemainingToSpawn--;

            // Spawn gradual para no saturar
            if (i % enemiesPerFrame == 0)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    private void SpawnEnemy()
    {
        EnemyTypeTopDown enemyType = enemyDatabase.GetRandomEnemy(currentWave);
        if (enemyType == null) return;

        Vector2 spawnPosition = GetSpawnPosition();

        // Obtener enemigo del pool
        GameObject enemyObj = ObjectPoolManager.Instance.GetObject($"Enemy_{enemyType.enemyName}");
        if (enemyObj == null)
        {
            // Crear nuevo si no existe en el pool
            enemyObj = new GameObject($"Enemy_{enemyType.enemyName}");
            enemyObj.AddComponent<SpriteRenderer>();
            enemyObj.AddComponent<CircleCollider2D>().isTrigger = true;
            enemyObj.AddComponent<Enemy>();
        }

        enemyObj.transform.position = spawnPosition;

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.Initialize(enemyType, currentWave);

        activeEnemies.Add(enemy);
    }

    private Vector2 GetSpawnPosition()
    {
        if (spawnOutsideCamera)
        {
            return GetSpawnPositionOutsideCamera();
        }
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return (Vector2)point.position + Random.insideUnitCircle * 2f;
        }
        else
        {
            // Spawn aleatorio en círculo
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }
    }

    private Vector2 GetSpawnPositionOutsideCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;

        // Obtener límites de la cámara
        float height = cam.orthographicSize * 2;
        float width = height * cam.aspect;

        // Elegir un lado aleatorio
        int side = Random.Range(0, 4);
        Vector2 spawnPos = cam.transform.position;

        switch (side)
        {
            case 0: // Arriba
                spawnPos.y += height / 2 + 2f;
                spawnPos.x += Random.Range(-width / 2, width / 2);
                break;
            case 1: // Derecha
                spawnPos.x += width / 2 + 2f;
                spawnPos.y += Random.Range(-height / 2, height / 2);
                break;
            case 2: // Abajo
                spawnPos.y -= height / 2 + 2f;
                spawnPos.x += Random.Range(-width / 2, width / 2);
                break;
            case 3: // Izquierda
                spawnPos.x -= width / 2 + 2f;
                spawnPos.y += Random.Range(-height / 2, height / 2);
                break;
        }

        return spawnPos;
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesKilledThisWave++;

        onEnemyKilled?.Invoke(enemiesKilledThisWave);

        // Verificar si la oleada terminó
        if (enemiesRemainingToSpawn == 0 && activeEnemies.Count == 0)
        {
            CompleteWave();
        }
    }

    private void CompleteWave()
    {
        waveInProgress = false;
        onWaveComplete?.Invoke(currentWave);

        ShowWaveCompleteUI();

        if (autoStartWaves)
        {
            Invoke(nameof(StartNextWave), timeBetweenWaves);
        }
    }

    private void CleanupDeadEnemies()
    {
        activeEnemies.RemoveAll(e => e == null || e.currentState == Enemy.EnemyState.Dead);
    }

    private void ShowWaveStartUI()
    {
        // Implementar UI de inicio de oleada
        Debug.Log($"Wave {currentWave} Started!");
    }

    private void ShowWaveCompleteUI()
    {
        // Implementar UI de oleada completada
        Debug.Log($"Wave {currentWave} Complete!");
    }

    public void PauseWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
        }

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.enabled = false;
        }
    }

    public void ResumeWave()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.enabled = true;
        }
    }

    public List<Enemy> GetActiveEnemies()
    {
        CleanupDeadEnemies();
        return activeEnemies;
    }

    public int GetEnemiesRemaining()
    {
        return activeEnemies.Count + enemiesRemainingToSpawn;
    }
}