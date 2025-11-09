using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptables/Player/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Personaje")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
}
