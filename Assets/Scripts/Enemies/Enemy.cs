using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IDamageable
{
    // Enemy states behavior
    public EnemyState enemyCurrentState;

    // Enemy Template
    public EnemyType enemyType;

    // Enemy Stats
    private float health = 0;
    private float moveSpeed = 0;
    private float expAmount = 0;


    private void Start()
    {
        // always start in idle state
        enemyCurrentState = EnemyState.Idle;
        SetEnemyStats();
    }
    
    private void SetEnemyStats()
    {
        health = enemyType.maxHealth;
        moveSpeed = enemyType.moveSpeed;
        expAmount = enemyType.expAmount;
    }
    

    public void SetInZoneState()
    {
        if (enemyCurrentState == EnemyState.Moving) return;
        
        int initialState = Random.Range(0, 2);
        enemyCurrentState = initialState == 0 ? EnemyState.Idle : EnemyState.Moving;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"aww shiet! {amount} Damage");
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}