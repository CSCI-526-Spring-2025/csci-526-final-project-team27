using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashCooldown = 2f;

    [Header("Combat")]
    [SerializeField] GameObject magicProjectile;
    [SerializeField] float attackCooldown = 0.3f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private float lastAttackTime;
    private float dashTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
        HandleDash();
    } 

    void HandleMovement()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        rb.linearVelocity = input * (Input.GetKey(KeyCode.LeftShift) ? dashSpeed : moveSpeed);
    }

    void HandleAttack()
    {
        if (Input.GetMouseButton(0) && Time.time - lastAttackTime >= attackCooldown)
        {
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shootDirection = (mouseWorldPos - (Vector2)transform.position).normalized;

            Instantiate(
                magicProjectile,
                transform.position,
                Quaternion.FromToRotation(Vector2.up, shootDirection)
            ).GetComponent<Rigidbody2D>().linearVelocity = shootDirection * 10f;

            lastAttackTime = Time.time;
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dashTimer <= 0)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * dashSpeed;
            dashTimer = dashCooldown;
        }
        else
        {
            dashTimer -= Time.deltaTime;
        }
    }
}