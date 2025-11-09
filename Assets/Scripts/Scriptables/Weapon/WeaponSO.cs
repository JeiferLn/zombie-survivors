using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptables/Weapon/New Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon")]
    public string weaponName;
    public float damage;
    public float fireRate;
    public float range;
    public float velocity;

    [Header("Others")]
    public float autoTargetRange;

    [Header("Prefab")]
    public GameObject bulletPrefab;
}
