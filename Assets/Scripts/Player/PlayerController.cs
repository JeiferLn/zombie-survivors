using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponSO equippedWeapon;

    private Rigidbody2D rb;

    private bool canPickUp;
    private WeaponPickup weaponOnGround;

    private Vector2? overrideLookDir = null;

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
    }

    private void FixedUpdate()
    {
        // Movimiento
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v);

        rb.velocity =
            move.magnitude > 0.1f ? move.normalized * playerStats.moveSpeed : Vector2.zero;

        // Rotación (según auto-target ó mouse)
        Vector2 lookDir;

        if (overrideLookDir.HasValue)
        {
            // Mirar al enemigo
            lookDir = overrideLookDir.Value;
        }
        else
        {
            // Mirar al mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lookDir = mousePos - transform.position;
        }

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        equippedWeapon = weapon;
    }

    public WeaponSO GetEquippedWeapon() => equippedWeapon;

    public void SetLookDirection(Vector2? dir)
    {
        overrideLookDir = dir;
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
