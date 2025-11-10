using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BulletPool pool;

    private float damage;
    private float speed;
    private float range;

    private bool homing;
    private Transform target;

    private Vector2 startPos;

    private WeaponSO weapon;

    private float lifeTimer;
    private float maxLifetime = 5f;

    private string poolTag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPoolTag(string tag)
    {
        poolTag = tag;
    }

    public void Init(WeaponSO so, Vector2 direction, BulletPool fromPool)
    {
        weapon = so;
        pool = fromPool;

        damage = so.damage;
        speed = so.speed;
        range = so.range;
        homing = false;
        target = null;

        startPos = transform.position;
        lifeTimer = 0f;

        direction.Normalize();
        rb.linearVelocity = direction * speed;

        RotateToward(rb.linearVelocity);
    }

    public void InitHoming(WeaponSO so, Transform newTarget, BulletPool fromPool)
    {
        pool = fromPool;

        damage = so.damage;
        speed = so.speed;
        range = so.range;
        homing = true;
        target = newTarget;

        startPos = transform.position;
        lifeTimer = 0f;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (homing && target != null)
        {
            Vector2 dir = ((Vector2)target.position - rb.position).normalized;
            float turnRate = 12f;
            Vector2 desired = dir * speed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, turnRate * Time.deltaTime);

            RotateToward(rb.linearVelocity);
        }

        if (Vector2.Distance(startPos, transform.position) >= range)
        {
            Despawn();
            return;
        }

        if (lifeTimer >= maxLifetime)
        {
            Despawn();
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet hit: " + collision.collider.name);
        if (collision.collider.CompareTag("Limit"))
        {
            Despawn();
            return;
        }

        if (collision.collider.TryGetComponent<IDamageable>(out var dmg))
        {
            if (weapon.stickToTarget)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = target.position;
                return;
            }

            dmg.TakeDamage(damage);
            Despawn();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
            Despawn();
            return;
        }
    }

    private void RotateToward(Vector2 dir)
    {
        if (dir.sqrMagnitude == 0f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Despawn()
    {
        rb.linearVelocity = Vector2.zero;

        if (string.IsNullOrEmpty(poolTag))
        {
            gameObject.SetActive(false);
            return;
        }

        pool.Return(poolTag, this);
    }
}
