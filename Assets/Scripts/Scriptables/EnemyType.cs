using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy TopDown", menuName = "Enemy System TopDown/Enemy Type")]
public class EnemyTypeTopDown : ScriptableObject
{
    [Header("Basic Stats")]
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f; // Grados por segundo
    public int scoreValue = 10;
    
    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float loseTargetRange = 15f; // Distancia para perder al objetivo
    public bool requireLineOfSight = false;
    
    [Header("Attacks")]
    public EnemyAttackData primaryAttack;
    public EnemyAttackData secondaryAttack;
    [Range(0f, 1f)]
    public float secondaryAttackChance = 0.3f;
    
    [Header("Movement")]
    public MovementBehavior movementType = MovementBehavior.Direct;
    public float strafingDistance = 3f; // Para movimiento circular
    public float zigzagAmplitude = 2f; // Para movimiento zigzag
    public float preferredDistance = 5f; // Distancia preferida del jugador
    
    [Header("Visual")]
    public Sprite sprite;
    public Color tintColor = Color.white;
    public float scale = 1f;
    public RuntimeAnimatorController animator; // Opcional
    
    [Header("Death")]
    public GameObject deathEffectPrefab;
    public GameObject[] lootPrefabs;
    [Range(0f, 1f)]
    public float lootDropChance = 0.1f;
    
    public enum MovementBehavior
    {
        Static,        // No se mueve
        Direct,        // Directo al jugador
        Strafe,        // Movimiento circular alrededor del jugador
        ZigZag,        // Zigzag hacia el jugador
        KeepDistance,  // Mantiene distancia
        Patrol,        // Patrulla puntos
        Random         // Movimiento aleatorio
    }
}