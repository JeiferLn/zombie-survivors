using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BulletScript bulletPrefab;
    [SerializeField] private PlayerStats playerStats;

    private Rigidbody2D rb;

    private float nextFireTime = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + playerStats.fireRate;
        }
    }

    void FixedUpdate()
    {
        // Movimiento
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical);

        if (movement.magnitude > 0.1f)
        {
            rb.velocity = movement.normalized * playerStats.moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        // Rotación hacia el mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        BulletScript bullet = Instantiate(bulletPrefab, transform.position + transform.right * 1f, transform.rotation);

        bullet.Shoot(transform, playerStats.bulletSpeed);
    }
}
