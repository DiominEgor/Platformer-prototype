using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("PlayerMovement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float crouchingSpeed = 2f;
    private Rigidbody2D playerRb;
    public PlayerInput playerInput;
    public Vector2 moveInput;
    private bool isCrouching;


    [Header("Animation")]
    [SerializeField] private Animator animator;


    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float doubleJumpForce = 10;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(4f, 10f);
    private bool isWallJumping;
    private bool canDoubleJump;


    [Header("WallSliding")]
    [SerializeField] private int wallSlideSpeed = 2;
    private bool isWallSliding;


    [Header("Utilities")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float halfPlayerHeight;
    private float halfPlayerWidth;

    private bool isGrounded;
    private int wallDirection;
    private BoxCollider2D playerCollider;


    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        halfPlayerHeight = spriteRenderer.bounds.extents.y;
        halfPlayerWidth = spriteRenderer.bounds.extents.x;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = IsOnGround();
        wallDirection = GetWallTouch();

        CharacterFlip();
        WallSlide();

        // для дебага
        // Debug.DrawRay(transform.position, Vector2.down * (halfPlayerHeight + 0.1f), Color.red);
        Debug.DrawRay(transform.position, Vector2.right * (halfPlayerWidth + 0.1f), Color.blue);
        Debug.DrawRay(transform.position, Vector2.left * (halfPlayerWidth + 0.1f), Color.blue);
    }

    void FixedUpdate()
    {
        Move();

        UpdatePlayerState();
        UpdateAnimator();
    }

    //менеджер состояний
    private void UpdatePlayerState()
    {
        if (isGrounded && playerRb.linearVelocity.y <= 0.1f)
        {
            canDoubleJump = true;
            isWallJumping = false;
            // isCrouching = false;
        }
    }

    //менеджер анимаций
    private void UpdateAnimator()
    {
        float vertivalVelocity = playerRb.linearVelocity.y;
        float horizontalVelocity = Math.Abs(playerRb.linearVelocity.x);

        animator.SetFloat("VerticalVelocity", vertivalVelocity);
        animator.SetFloat("HorizontalVelocity", horizontalVelocity);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isWallJumping", isWallJumping);
        animator.SetBool("isWallSliding", isWallSliding);
        animator.SetBool("isCrouching", isCrouching);
    }


    //регать касание стены через рэйкаст
    private int GetWallTouch()
    {
        int wallLayer = LayerMask.GetMask("Ground");

        if (Physics2D.Raycast(transform.position, Vector2.right, halfPlayerWidth + 0.1f, wallLayer))
        {
            return -1;
        }
        if (Physics2D.Raycast(transform.position, Vector2.left, halfPlayerWidth + 0.1f, wallLayer))
        {
            return 1;
        }

        return 0;
    }

    //передвижение
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Move()
    {
        if (isWallJumping) return;
        if (!isCrouching)
        {
            float targetSpeed = moveInput.x * speed;
            playerRb.linearVelocity = new Vector2(targetSpeed, playerRb.linearVelocity.y);
        }
        else
        {
            float targetSpeed = moveInput.x * crouchingSpeed;
            playerRb.linearVelocity = new Vector2(targetSpeed, playerRb.linearVelocity.y);
        }
    }

    private void OnCrouch()
    {
        Crouching();
    }

    private void Crouching()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            playerCollider.size = new Vector2(0.65f, 0.85f);
        }
        else
        {
            isCrouching = false;
            playerCollider.size = new Vector2(0.65f, 1.18f);
        }
    }

    //разворот модельки
    private void CharacterFlip()
    {
        Vector2 lookRight = new Vector3(1, 1, 1);
        Vector2 lookLeft = new Vector3(-1, 1, 1);

        if (isWallJumping && playerRb.linearVelocity.x > 0)
        {
            transform.localScale = lookRight;
        }
        else if (isWallJumping && playerRb.linearVelocity.x < 0)
        {
            transform.localScale = lookLeft;
        }
        else if (moveInput.x > 0)
        {
            transform.localScale = lookRight;
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = lookLeft;
        }
    }

    //прыжок

    private void OnJump(InputValue value)
    {
        JumpType();
    }

    //определить какой прыжок
    private void JumpType()
    {
        if (isGrounded)
        {
            Jump(jumpForce);
            return;
        }

        if (wallDirection != 0)
        {
            WallJump(wallDirection);
            return;
        }

        if (canDoubleJump)
        {
            DoubleJump();
        }
    }
    private void Jump(float force)
    {
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    //двойной прыжок
    private void DoubleJump()
    {
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.angularVelocity = 0f;
        Jump(doubleJumpForce);
        canDoubleJump = false;
    }

    //прыжки от стен
    private void WallJump(int direction)
    {
        Vector2 force = wallJumpForce;
        force.x *= direction;
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.angularVelocity = 0f;
        isWallSliding = false;
        isWallJumping = true;
        Debug.Log(isWallJumping);
        playerRb.AddForce(force, ForceMode2D.Impulse);
    }

    //чтобы чел съезжал вниз по стене, а не падал, когда я жму передвижение
    private void WallSlide()
    {
        if (!isGrounded && wallDirection != 0 && moveInput.x != 0 && playerRb.linearVelocity.y <= 0)
        {
            Debug.Log("touch");
            isWallSliding = true;
            transform.localScale = new Vector3(wallDirection, 1, 1);
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, Mathf.Max(playerRb.linearVelocity.y, -wallSlideSpeed));
            canDoubleJump = true;

        }
        else
        {
            isWallSliding = false;
        }
    }

    //проверка касания с землей через луч
    private bool IsOnGround()
    {
        int groundLayer = LayerMask.GetMask("Ground");

        Vector2 center = transform.position;
        Vector2 left = center + Vector2.left * (halfPlayerWidth * 0.8f);
        Vector2 right = center + Vector2.right * (halfPlayerWidth * 0.8f);

        bool centerHit = Physics2D.Raycast(
            center,
            Vector2.down,
            halfPlayerHeight + 0.1f,
            groundLayer
        );

        bool leftHit = Physics2D.Raycast(
            left,
            Vector2.down,
            halfPlayerHeight + 0.1f,
            groundLayer
        );

        bool rightHit = Physics2D.Raycast(
            right,
            Vector2.down,
            halfPlayerHeight + 0.1f,
            groundLayer
        );

        Debug.DrawRay(center, Vector2.down * (halfPlayerHeight + 0.1f), Color.red);
        Debug.DrawRay(left, Vector2.down * (halfPlayerHeight + 0.1f), Color.red);
        Debug.DrawRay(right, Vector2.down * (halfPlayerHeight + 0.1f), Color.red);

        return centerHit || leftHit || rightHit;
    }

}
