using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int currentLevel = 1;
    private float currentXP = 0f;
    private float maxXPPerLevel = 100f;

    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        // Asegurar que haya solo un GameManager
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

    public void AddExperience(float amount)
    {
        currentXP += amount;

        // Si alcanzó o superó la XP máxima, subir de nivel
        while (currentXP >= maxXPPerLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP -= maxXPPerLevel;
        maxXPPerLevel *= 1.1f; // aumenta un 10%

        playerStats.maxHealth *= 1.1f;  // +10% vida
        playerStats.moveSpeed *= 1.05f; // +5% velocidad

        Debug.Log($"Nivel {currentLevel} alcanzado! Vida: {playerStats.maxHealth}, Velocidad: {playerStats.moveSpeed}");
    }
}
