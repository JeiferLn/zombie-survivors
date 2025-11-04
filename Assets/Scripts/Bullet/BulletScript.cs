using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float speed = 10.0f;

    public void Shoot(Transform playerPosition)
    {
        rb = GetComponent<Rigidbody2D>();

        rb.AddForce(playerPosition.right * speed, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
