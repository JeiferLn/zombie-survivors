using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptables/Player/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("General")]
    public float maxHealth = 100f;

    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Ataque")]
    public float damage = 10f;
    public float fireRate = 0.2f;
    public float bulletSpeed = 10f;
}
