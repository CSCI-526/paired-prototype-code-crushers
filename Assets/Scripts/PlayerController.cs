using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sanity = 1f;
    private float baseSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float horizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Prevent the player from rotating
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        movement = new Vector2(horizontalInput, 0);
    }

    // Apply movement to the player in FixedUpdate for physics consistency
    void FixedUpdate()
    {
        // rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
        float currentSpeed = sanity * baseSpeed;
        rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);
    }
}
