using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    private float damage;
    private Vector2 velocity;
    private GameObject owner;
    private Transform target;
    private bool isHoming;
    private float homingStrength;
    private float lifetime = 5f;
    private float aliveTime;
    
    void Update()
    {
        // Movimiento
        if (isHoming && target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            velocity = Vector2.Lerp(velocity, direction * velocity.magnitude, homingStrength * Time.deltaTime);
        }
        
        transform.Translate(velocity * Time.deltaTime, Space.World);
        
        // Rotación
        if (velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Lifetime
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    public void Initialize(float dmg, Vector2 vel, GameObject own, bool homing = false, Transform tgt = null)
    {
        damage = dmg;
        velocity = vel;
        owner = own;
        isHoming = homing;
        target = tgt;
        homingStrength = 2f;
        aliveTime = 0;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner || other.CompareTag("Enemy"))
            return;
        
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            ReturnToPool();
        }
        else if (other.CompareTag("Obstacle"))
        {
            ReturnToPool();
        }
    }
    
    public void OnGetFromPool()
    {
        aliveTime = 0;
    }
    
    public void OnReturnToPool()
    {
        velocity = Vector2.zero;
        target = null;
    }
    
    void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}