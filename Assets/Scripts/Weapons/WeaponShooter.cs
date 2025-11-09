using UnityEngine;

public class WeaponShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform muzzle;

    [Header("Enemy Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private float nextFireTime = 0f;

    void Update()
    {
        WeaponSO weapon = player?.GetEquippedWeapon();
        
        if (weapon == null || weapon.bulletPrefab == null)
        {
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot(weapon);
            nextFireTime = Time.time + (1f / Mathf.Max(weapon.fireRate, 0.01f));
        }
    }

    public void Shoot(WeaponSO weapon)
    {
        Vector2 dir;
        Transform target = null;

        if (weapon.autoTargetRange)
        {
            target = FindNearestEnemy(muzzle.position, weapon.range);

            dir = target != null 
                ? (Vector2)(target.position - muzzle.position)
                : GetMouseDirection();
        }
        else
        {
            dir = GetMouseDirection();
        }


        BulletController bullet = bulletPool.Get(weapon.bulletPoolTag);
        bullet.transform.position = muzzle.position;

        if (weapon.autoTargetRange && target != null)
            bullet.InitHoming(weapon, target, bulletPool);
        else
            bullet.Init(weapon, dir, bulletPool);
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;
        return (mouse - muzzle.position);
    }

    private Transform FindNearestEnemy(Vector3 origin, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, enemyLayer);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = (hit.transform.position - origin).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        WeaponSO weapon = player?.GetEquippedWeapon();
        if (weapon == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(muzzle.position, weapon.range);
    }
}
