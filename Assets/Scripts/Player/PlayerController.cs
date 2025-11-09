using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponSO equippedWeapon;

    private Rigidbody2D rb;

    private bool canPickUp ;
    private WeaponPickup weaponOnGround;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (canPickUp && Input.GetKeyDown(KeyCode.E))
        {
            EquipWeapon(weaponOnGround.weaponData);
        }
        
        // if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        // {
        //     Shoot();
        // }


    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v);

        rb.velocity =
            move.magnitude > 0.1f ? move.normalized * playerStats.moveSpeed : Vector2.zero;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        equippedWeapon = weapon;
        Debug.Log("Arma equipada: " + weapon.weaponName + " Daño: " + weapon.damage + " Frecuencia de disparo: " + weapon.fireRate + " Alcance: " + weapon.range + " Velocidad: " + weapon.velocity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            weaponOnGround = collision.GetComponent<WeaponPickup>();
            if (weaponOnGround != null)
            {
                canPickUp = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            canPickUp = false;
            weaponOnGround = null;
        }
    }
}
