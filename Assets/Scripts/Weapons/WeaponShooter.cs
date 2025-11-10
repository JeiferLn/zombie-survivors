using UnityEngine;

public class WeaponShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform muzzle;

    [Header("Enemy Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Aim Settings")]
    [SerializeField] private float rotationSmooth = 7f;

    private float nextFireTime = 0f;

    // Dirección interpolada suavemente
    private Vector2 smoothLookDirection;

    void Start()
    {
        // Dirección inicial del jugador (mirando al frente)
        smoothLookDirection = player.transform.right;
    }

    void Update()
    {
        WeaponSO weapon = player?.GetEquippedWeapon();

        if (weapon == null || weapon.bulletPrefab == null)
        {
            player.SetLookDirection(null);
            return;
        }

        // ---------------------------
        //   SUAVIZADO DE AUTO TARGET
        // ---------------------------
        if (weapon.autoTargetRange)
        {
            Transform target = FindNearestEnemy(muzzle.position, weapon.range);

            if (target != null)
            {
                Vector2 targetDir = (Vector2)(target.position - player.transform.position);

                smoothLookDirection = Vector3.Slerp(
                    smoothLookDirection,
                    targetDir.normalized,
                    Time.deltaTime * rotationSmooth
                );

                player.SetLookDirection(smoothLookDirection);
            }
            else
            {
                // Si no hay enemigo, apuntar suavemente al mouse
                Vector2 mouseDir = GetMouseDirectionFromPlayer();

                smoothLookDirection = Vector3.Slerp(
                    smoothLookDirection,
                    mouseDir.normalized,
                    Time.deltaTime * rotationSmooth
                );

                player.SetLookDirection(smoothLookDirection);
            }
        }
        else
        {
            // Sin auto-target → mirar al mouse normalmente
            player.SetLookDirection(null);
        }


        // ---------------------------
        //         DISPARO
        // ---------------------------
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

            if (target == null)
                return;

            dir = (Vector2)(target.position - muzzle.position);
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

    private Vector2 GetMouseDirectionFromPlayer()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;
        return (mouse - player.transform.position);
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
