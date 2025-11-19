using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptables/Weapon/New Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon")]
    public string weaponName;
    public float damage;
    public float fireRate;
    public float range;
    public float speed;

    [Header("Others")]
    public bool autoTargetRange = false;

    [Header("Bullet Pool")]
    public string bulletPoolTag;

    [Header("Prefab")]
    public GameObject bulletPrefab;
}
