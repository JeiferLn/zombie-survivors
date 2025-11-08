using UnityEngine;
using System.Collections;

public class EnemyTopDown : MonoBehaviour, IPoolable
{
    [Header("Configuration")]
    public EnemyTypeTopDown enemyType;
    
    [Header("Runtime Stats")]
    private float currentHealth;
    private float currentSpeed;
    private Transform target;
    private Vector2 moveDirection;
    private float primaryCooldown;
    private float secondaryCooldown;
    
    [Header("State")]
    private EnemyState currentState = EnemyState.Spawning;
    private float stateTimer;
    private Vector2 targetPosition;
    private float strafeAngle;
    
    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D hitbox;
    private Animator animator;
    
    [Header("Optimization")]
    private float nextUpdateTime;
    private const float UPDATE_INTERVAL = 0.1f; // Actualizar IA cada 100ms
    private bool isVisible = true;
    
    public enum EnemyState
    {
        Spawning,
        Idle,
        Moving,
        Attacking,
        Dying,
        Dead
    }
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        hitbox = GetComponent<CircleCollider2D>();
        if (hitbox == null)
        {
            hitbox = gameObject.AddComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
        }
        
        animator = GetComponent<Animator>();
    }
    
    public void Initialize(EnemyTypeTopDown type, int waveNumber = 1)
    {
        enemyType = type;
        
        // Aplicar stats base
        currentHealth = enemyType.maxHealth * (1f + (waveNumber - 1) * 0.1f);
        currentSpeed = enemyType.moveSpeed * (1f + (waveNumber - 1) * 0.05f);
        
        // Configurar visual
        spriteRenderer.sprite = enemyType.sprite;
        spriteRenderer.color = enemyType.tintColor;
        transform.localScale = Vector3.one * enemyType.scale;
        
        // Configurar animator si existe
        if (animator != null && enemyType.animator != null)
            animator.runtimeAnimatorController = enemyType.animator;
        
        // Configurar collider
        hitbox.radius = enemyType.scale * 0.5f;
        
        // Buscar jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
        
        // Reset cooldowns
        primaryCooldown = 0;
        secondaryCooldown = 0;
        
        // Iniciar
        ChangeState(EnemyState.Spawning);
        StartCoroutine(SpawnAnimation());
    }
    
    IEnumerator SpawnAnimation()
    {
        float spawnTime = 0.5f;
        float timer = 0;
        
        while (timer < spawnTime)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0, enemyType.scale, timer / spawnTime);
            transform.localScale = Vector3.one * scale;
            
            Color color = enemyType.tintColor;
            color.a = timer / spawnTime;
            spriteRenderer.color = color;
            
            yield return null;
        }
        
        ChangeState(EnemyState.Idle);
    }
    
    void Update()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Spawning) 
            return;
        
        // Optimización: Solo actualizar si es visible
        isVisible = IsVisibleToCamera();
        if (!isVisible && currentState != EnemyState.Moving)
            return;
        
        // Actualizar cooldowns
        if (primaryCooldown > 0)
            primaryCooldown -= Time.deltaTime;
        if (secondaryCooldown > 0)
            secondaryCooldown -= Time.deltaTime;
        
        // Actualizar IA con intervalos para optimización
        if (Time.time >= nextUpdateTime)
        {
            UpdateAI();
            nextUpdateTime = Time.time + UPDATE_INTERVAL;
        }
        
        // Movimiento suave siempre se actualiza
        HandleMovement();
        
        // Rotación hacia el objetivo
        if (target != null && enemyType.rotationSpeed > 0)
        {
            RotateTowardsTarget();
        }
    }
    
    void UpdateAI()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }
        
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= enemyType.detectionRange)
                {
                    ChangeState(EnemyState.Moving);
                }
                break;
                
            case EnemyState.Moving:
                if (distanceToTarget <= enemyType.attackRange)
                {
                    ChangeState(EnemyState.Attacking);
                }
                else if (distanceToTarget > enemyType.loseTargetRange)
                {
                    ChangeState(EnemyState.Idle);
                    target = null;
                }
                else
                {
                    UpdateMovementDirection();
                }
                break;
                
            case EnemyState.Attacking:
                if (distanceToTarget > enemyType.attackRange * 1.2f)
                {
                    ChangeState(EnemyState.Moving);
                }
                else
                {
                    TryAttack();
                }
                break;
        }
    }
    
    void UpdateMovementDirection()
    {
        if (target == null) return;
        
        Vector2 directionToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
        
        switch (enemyType.movementType)
        {
            case EnemyTypeTopDown.MovementBehavior.Direct:
                moveDirection = directionToTarget;
                break;
                
            case EnemyTypeTopDown.MovementBehavior.Strafe:
                strafeAngle += Time.deltaTime * 2f;
                Vector2 strafeOffset = new Vector2(
                    Mathf.Cos(strafeAngle) * enemyType.strafingDistance,
                    Mathf.Sin(strafeAngle) * enemyType.strafingDistance
                );
                targetPosition = (Vector2)target.position + strafeOffset;
                moveDirection = (targetPosition - (Vector2)transform.position).normalized;
                break;
                
            case EnemyTypeTopDown.MovementBehavior.ZigZag:
                float zigzag = Mathf.Sin(Time.time * 3f) * enemyType.zigzagAmplitude;
                Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
                moveDirection = directionToTarget + perpendicular * zigzag * 0.3f;
                moveDirection.Normalize();
                break;
                
            case EnemyTypeTopDown.MovementBehavior.KeepDistance:
                float currentDistance = Vector2.Distance(transform.position, target.position);
                if (currentDistance < enemyType.preferredDistance - 1f)
                {
                    moveDirection = -directionToTarget; // Alejarse
                }
                else if (currentDistance > enemyType.preferredDistance + 1f)
                {
                    moveDirection = directionToTarget; // Acercarse
                }
                else
                {
                    // Movimiento lateral
                    strafeAngle += Time.deltaTime;
                    moveDirection = new Vector2(-directionToTarget.y, directionToTarget.x);
                }
                break;
                
            case EnemyTypeTopDown.MovementBehavior.Random:
                if (stateTimer <= 0)
                {
                    moveDirection = Random.insideUnitCircle.normalized;
                    stateTimer = Random.Range(1f, 3f);
                }
                stateTimer -= Time.deltaTime;
                break;
        }
    }
    
    void HandleMovement()
    {
        if (currentState != EnemyState.Moving) 
            return;
        
        // Movimiento suave
        Vector2 movement = moveDirection * currentSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
        
        // Animación de movimiento
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
    }
    
    void RotateTowardsTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.rotation.eulerAngles.z;
        
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, 
            enemyType.rotationSpeed * Time.deltaTime / 360f);
        
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    void TryAttack()
    {
        // Decidir qué ataque usar
        bool useSecondary = secondaryCooldown <= 0 && 
                           enemyType.secondaryAttack != null && 
                           Random.value < enemyType.secondaryAttackChance;
        
        if (useSecondary)
        {
            if (secondaryCooldown <= 0)
            {
                ExecuteAttack(enemyType.secondaryAttack, false);
            }
        }
        else if (primaryCooldown <= 0)
        {
            ExecuteAttack(enemyType.primaryAttack, true);
        }
    }
    
    void ExecuteAttack(EnemyAttackData attack, bool isPrimary)
    {
        if (attack == null || target == null) return;
        
        // Aplicar cooldown
        if (isPrimary)
            primaryCooldown = attack.cooldown;
        else
            secondaryCooldown = attack.cooldown;
        
        // Mostrar advertencia si está configurado
        if (attack.showWarningArea)
        {
            ShowAttackWarning(attack);
        }
        
        // Ejecutar ataque después del charge time
        StartCoroutine(PerformAttackAfterCharge(attack));
    }
    
    IEnumerator PerformAttackAfterCharge(EnemyAttackData attack)
    {
        // Animación de carga
        if (animator != null)
        {
            animator.SetTrigger("ChargeAttack");
        }
        
        yield return new WaitForSeconds(attack.chargeTime);
        
        if (currentState == EnemyState.Dead) yield break;
        
        Vector2 attackDirection = target != null ? 
            ((Vector2)target.position - (Vector2)transform.position).normalized : 
            Vector2.up;
        
        if (attack.isRanged)
        {
            FireProjectiles(attack, attackDirection);
        }
        else
        {
            MeleeAttack(attack);
        }
        
        // Efectos
        if (attack.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(attack.soundEffect, transform.position);
        }
        
        if (attack.effectPrefab != null)
        {
            GameObject effect = ObjectPoolManager.Instance.GetObject(attack.effectPrefab);
            effect.transform.position = transform.position;
        }
    }
    
    void FireProjectiles(EnemyAttackData attack, Vector2 baseDirection)
    {
        switch (attack.pattern)
        {
            case EnemyAttackData.AttackPattern.Single:
                CreateProjectile(attack, baseDirection);
                break;
                
            case EnemyAttackData.AttackPattern.Burst:
                for (int i = 0; i < attack.projectileCount; i++)
                {
                    float angle = (i - attack.projectileCount / 2f) * (attack.spreadAngle / attack.projectileCount);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
                    CreateProjectile(attack, direction);
                }
                break;
                
            case EnemyAttackData.AttackPattern.Circle:
                for (int i = 0; i < attack.projectileCount; i++)
                {
                    float angle = (360f / attack.projectileCount) * i;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                    CreateProjectile(attack, direction);
                }
                break;
                
            case EnemyAttackData.AttackPattern.Cone:
                for (int i = 0; i < attack.projectileCount; i++)
                {
                    float angle = Random.Range(-attack.spreadAngle / 2, attack.spreadAngle / 2);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
                    CreateProjectile(attack, direction);
                }
                break;
        }
    }
    
    void CreateProjectile(EnemyAttackData attack, Vector2 direction)
    {
        GameObject projectile = ObjectPoolManager.Instance.GetObject(attack.projectilePrefab);
        projectile.transform.position = transform.position;
        
        EnemyProjectile proj = projectile.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Initialize(attack.damage, direction * attack.projectileSpeed, 
                          gameObject, attack.homingProjectile, target);
        }
    }
    
    void MeleeAttack(EnemyAttackData attack)
    {
        // Detectar enemigos en el área de ataque
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attack.range);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.SendMessage("TakeDamage", attack.damage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    void ShowAttackWarning(EnemyAttackData attack)
    {
        // Aquí puedes mostrar un indicador visual del área de ataque
        // Por ejemplo, un círculo rojo semi-transparente
    }
    
    public void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;
        
        currentHealth -= damage;
        
        // Efecto de daño
        StartCoroutine(DamageFlash());
        
        // Mostrar daño (opcional)
        // DamageNumberManager.Instance?.ShowDamage(transform.position, damage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = enemyType.tintColor;
    }
    
    void Die()
    {
        ChangeState(EnemyState.Dying);
        
        // Desactivar collider
        hitbox.enabled = false;
        
        // Efecto de muerte
        if (enemyType.deathEffectPrefab != null)
        {
            GameObject effect = ObjectPoolManager.Instance.GetObject(enemyType.deathEffectPrefab);
            effect.transform.position = transform.position;
        }
        
        // Drop loot
        if (Random.value < enemyType.lootDropChance && enemyType.lootPrefabs.Length > 0)
        {
            GameObject loot = enemyType.lootPrefabs[Random.Range(0, enemyType.lootPrefabs.Length)];
            Instantiate(loot, transform.position, Quaternion.identity);
        }
        
        // Dar puntos
        // GameManager.Instance?.AddScore(enemyType.scoreValue);
        
        // Notificar al wave manager
        WaveManager.Instance?.OnEnemyKilled(this);
        
        // Animación de muerte
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        float duration = 0.3f;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(enemyType.scale, 0, timer / duration);
            transform.localScale = Vector3.one * scale;
            
            Color color = spriteRenderer.color;
            color.a = 1f - (timer / duration);
            spriteRenderer.color = color;
            
            yield return null;
        }
        
        ChangeState(EnemyState.Dead);
        ReturnToPool();
    }
    
    void ChangeState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0;
    }
    
    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= enemyType.detectionRange)
            {
                target = player.transform;
            }
        }
    }
    
    bool IsVisibleToCamera()
    {
        return spriteRenderer.isVisible;
    }
    
    // Interfaz IPoolable
    public void OnGetFromPool()
    {
        hitbox.enabled = true;
        currentState = EnemyState.Spawning;
    }
    
    public void OnReturnToPool()
    {
        hitbox.enabled = false;
        target = null;
    }
    
    public void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (enemyType == null) return;
        
        // Rango de detección
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, enemyType.detectionRange);
        
        // Rango de ataque
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, enemyType.attackRange);
    }
}