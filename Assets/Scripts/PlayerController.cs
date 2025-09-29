using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 5f;

    [Header("Jump (velocity-based)")]
    [Tooltip("Target jump height in world units (independent of Rigidbody2D mass).")]
    public float desiredJumpHeight = 2.5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;
    public float coyoteTime = 0.10f;

    // ---- NEW: UI hook
    public event Action<float> OnSanityChanged;

    Rigidbody2D rb;
    Collider2D col;
    float h;
    bool jumpQueued;
    float coyoteCounter;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (groundMask.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) groundMask = 1 << g;
        }
    }

    void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) jumpQueued = true;
        sanity = Mathf.Clamp01(sanity); // keep clamped
    }

    void FixedUpdate()
    {
        float moveMult = Mathf.Lerp(minMoveMultiplier, 1f, sanity);
        rb.velocity = new Vector2(h * (baseMoveSpeed * moveMult), rb.velocity.y);

        bool grounded = IsGrounded();
        if (grounded) coyoteCounter = coyoteTime;
        else          coyoteCounter -= Time.fixedDeltaTime;

        if (jumpQueued && coyoteCounter > 0f)
        {
            jumpQueued = false;
            coyoteCounter = 0f;

            float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            float baseJumpSpeed = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));
            float jumpMult = Mathf.Lerp(minJumpMultiplier, 1f, sanity);

            Vector2 v = rb.velocity;
            v.y = baseJumpSpeed * jumpMult;
            rb.velocity = v;
        }
        else
        {
            jumpQueued = false;
        }
    }

    bool IsGrounded()
    {
        if (groundCheck)
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        Bounds b = col.bounds;
        Vector2 boxSize = new Vector2(b.size.x * 0.95f, 0.1f);
        float castDistance = 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(b.center, boxSize, 0f, Vector2.down, castDistance, groundMask);
        return hit.collider != null;
    }

    // ---- NEW: public API to change sanity
    public void ChangeSanity(float delta)
    {
        float before = sanity;
        sanity = Mathf.Clamp01(sanity + delta);
        if (!Mathf.Approximately(before, sanity))
            OnSanityChanged?.Invoke(sanity);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
