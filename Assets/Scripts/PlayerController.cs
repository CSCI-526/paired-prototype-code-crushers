using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sanity = 1f;
    private float baseMoveSpeed = 5f;
    private float baseJumpSpeed = 4f;

    private Rigidbody2D body;
    private float horizontalInput;
    private bool isSpaceKeyDown;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        // Prevent the player from rotating
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Raw is suitable for 2d pixel game; w/o Raw it will be smooth
        horizontalInput = Input.GetAxisRaw("Horizontal");

        isSpaceKeyDown = Input.GetKey(KeyCode.Space);
    }

    // Apply movement to the player in FixedUpdate for physics consistency
    void FixedUpdate()
    {
        float moveSpeed = sanity * baseMoveSpeed;
        body.velocity = new Vector2(horizontalInput * moveSpeed, body.velocity.y);

        if (isSpaceKeyDown)
        {
            float jumpSpeed = sanity * baseJumpSpeed;
            body.velocity = new Vector2(body.velocity.x, jumpSpeed);
        }
    }
}
