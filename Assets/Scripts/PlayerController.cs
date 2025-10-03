using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Sanity (0..1)")]
    [Range(0f, 1f)] public float sanity = 1f;
    [Range(0.2f, 1f)] public float minMoveMultiplier = 0.6f;
    [Range(0.2f, 1f)] public float minJumpMultiplier = 0.6f;

    [Tooltip("How much sanity to lose per tick while idling on the same platform.")]
    [Range(0f, 0.1f)] public float sanityAutoLoss = 0.01f;

    [Header("Idle Drain Rules")]
    [Tooltip("How long the player must be idle on the SAME platform before drain starts.")]
    public float idleDrainDelay = 10f;
    [Tooltip("Sanity drain tick interval once idle drain has started.")]
    public float idleDrainInterval = 1f;
    [Tooltip("Considered 'not moving' if |horizontal velocity| <= this.")]
    public float idleMoveSpeedEpsilon = 0.05f;

    [Header("Movement")]
    public float baseMoveSpeed = 5f;

    [Header("Jump (velocity-based)")]
    [Tooltip("Target jump height in world units (independent of Rigidbody2D mass).")]
    public float desiredJumpHeight = 2.5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;
    public float coyoteTime = 0.10f;

    // ---- UI hook
    public event Action<float> OnSanityChanged;

    Rigidbody2D rb;
    Collider2D col;
    float h;
    bool jumpQueued;
    float coyoteCounter;

    // ---- Idle-on-same-platform tracking
    float idleTimer;
    Collider2D currentGround;     // ground collider we are standing on now
    Collider2D lastGround;        // last ground collider (for "same platform" check)
    bool drainingIdle;            // are we currently draining due to idle?
    Coroutine idleDrainCo;

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
        // --- Move
        float moveMult = Mathf.Lerp(minMoveMultiplier, 1f, sanity);
        rb.velocity = new Vector2(h * (baseMoveSpeed * moveMult), rb.velocity.y);

        // --- Grounded & coyote time
        bool grounded = IsGrounded(out currentGround);
        if (grounded) coyoteCounter = coyoteTime;
        else          coyoteCounter -= Time.fixedDeltaTime;

        // --- Jump
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

        // --- Idle-on-same-platform detector
        bool veryStill = Mathf.Abs(rb.velocity.x) <= idleMoveSpeedEpsilon;
        bool samePlatform = (currentGround != null && currentGround == lastGround);

        if (grounded && veryStill)
        {
            // if we just stepped onto a different platform, reset the timer
            if (!samePlatform) idleTimer = 0f;

            idleTimer += Time.fixedDeltaTime;

            // start draining if we crossed the threshold
            if (idleTimer >= idleDrainDelay && !drainingIdle)
            {
                drainingIdle = true;
                idleDrainCo = StartCoroutine(IdleDrainRoutine());
            }
        }
        else
        {
            // moving or airborne -> stop drain & reset timer
            idleTimer = 0f;
            StopIdleDrainIfAny();
        }

        // remember platform for next frame
        lastGround = grounded ? currentGround : null;
    }

    // returns grounded + the ground collider we stand on
    bool IsGrounded(out Collider2D ground)
    {
        ground = null;

        if (groundCheck)
        {
            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
            ground = hit;
            return hit != null;
        }

        Bounds b = col.bounds;
        Vector2 boxSize = new Vector2(b.size.x * 0.95f, 0.1f);
        float castDistance = 0.05f;
        RaycastHit2D hit2 = Physics2D.BoxCast(b.center, boxSize, 0f, Vector2.down, castDistance, groundMask);
        ground = hit2.collider;
        return hit2.collider != null;
    }

    IEnumerator IdleDrainRoutine()
    {
        while (drainingIdle)
        {
            ChangeSanity(-sanityAutoLoss);
            yield return new WaitForSeconds(idleDrainInterval);
        }
    }

    void StopIdleDrainIfAny()
    {
        if (drainingIdle)
        {
            drainingIdle = false;
            if (idleDrainCo != null) StopCoroutine(idleDrainCo);
            idleDrainCo = null;
        }
    }

    // ---- public API to change sanity
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
