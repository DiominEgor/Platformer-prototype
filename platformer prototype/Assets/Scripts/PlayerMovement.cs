using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("PlayerMovement")]
    [SerializeField] private float speed = 5f;
    private float horizontalInput;
    private Rigidbody2D playerRb;


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
    private float halfPlayerHight;
    private float halfPlayerWidth;

    private bool isGrounded;
    private int wallDirection;


    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        halfPlayerHight = spriteRenderer.bounds.extents.y;
        halfPlayerWidth = spriteRenderer.bounds.extents.x;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        isGrounded = IsOnGround();
        wallDirection = GetWallTouch();

        CharacterFlip();
        WallSlide();

        if (Input.GetButtonDown("Jump"))
        {
            JumpType();
        }

        // для дебага
        Debug.DrawRay(transform.position, Vector2.down * (halfPlayerHight + 0.1f), Color.red);
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

    //регать касание стены через рэйкаст
    private int GetWallTouch()
    {
        int wallLayer = LayerMask.GetMask("Wall");

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
    private void Move()
    {
        if (isWallJumping) return;
        playerRb.linearVelocity = new Vector2(horizontalInput * speed, playerRb.linearVelocity.y);
    }

    //разворот модельки
    private void CharacterFlip()
    {
        if (playerRb.linearVelocity.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (playerRb.linearVelocity.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    //прыжок
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
        if (!isGrounded && wallDirection != 0 && horizontalInput != 0 && playerRb.linearVelocity.y <= 0)
        {
            Debug.Log("touch");
            isWallSliding = true;
            transform.localScale = new Vector3(wallDirection, 1, 1);
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, Mathf.Max(playerRb.linearVelocity.y, -wallSlideSpeed));

        }
        else
        {
            isWallSliding = false;
        }
    }

    //проверка касания с землей через луч
    private bool IsOnGround()
    {
        bool hit = Physics2D.Raycast(transform.position, Vector2.down, halfPlayerHight + 0.1f, LayerMask.GetMask("Ground"));
        return hit;
    }

}
