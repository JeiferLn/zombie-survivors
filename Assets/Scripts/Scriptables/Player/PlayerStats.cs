using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptables/Player/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Vida")]
    public float maxHealth = 100f;

    [Header("Movimiento")]
    public float moveSpeed = 2f;

    [Header("Ataque")]
    public float damage = 0f;
    public float fireRate = 0f;
    public float bulletSpeed = 0f;
}
