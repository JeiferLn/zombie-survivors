using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponSO equippedWeapon;

    private Rigidbody2D rb;

    private bool canPickUp;
    private WeaponPickup weaponOnGround;

    private Vector2? overrideLookDir = null;

    public bool canMove = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!canMove) return;

        if (canPickUp && Input.GetKeyDown(KeyCode.E))
        {
            EquipWeapon(weaponOnGround.weaponData);
        }
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v);

        rb.linearVelocity =
            move.magnitude > 0.1f ? move.normalized * playerStats.moveSpeed : Vector2.zero;

        Vector2 lookDir;

        if (overrideLookDir.HasValue)
        {
            lookDir = overrideLookDir.Value;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lookDir = mousePos - transform.position;
        }

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetBlocked(bool value)
    {
        canMove = !value;
        if (value) rb.linearVelocity = Vector2.zero;
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
