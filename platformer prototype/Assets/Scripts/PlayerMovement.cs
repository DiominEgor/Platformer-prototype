using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool isJumping;
    private bool canDoubleJump;


    [Header("WallSliding")]
    private bool isWallSliding;
    private int wallSlideSpeed = 2;


    [Header("Utilities")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float halfPlayerHight;
    private float halfPlayerWidth;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        halfPlayerHight = spriteRenderer.bounds.extents.y;
        halfPlayerWidth = spriteRenderer.bounds.extents.x;
    }
    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterFlip();
        WallSlide();

        horizontalInput = Input.GetAxisRaw("Horizontal");


        if (Input.GetButtonDown("Jump"))
        {
            JumpType();
        }

        // для дебага
        Debug.DrawRay(transform.position, Vector2.down * (halfPlayerHight + 0.1f), Color.red);
        Debug.DrawRay(transform.position, Vector2.right * (halfPlayerWidth + 0.1f), Color.blue);
    }

    void FixedUpdate()
    {
        Move();

        //проверка земли и обновление всякой шляпы
        bool grounded = IsOnGround();
        if (grounded && playerRb.linearVelocity.y <= 0.1f)
        {
            canDoubleJump = true;
            isWallJumping = false;
            isJumping = false;
        }

        //контроль и апдейт анимаций
        animator.SetFloat("VerticalVelocity", playerRb.linearVelocity.y);
        animator.SetFloat("HorizontalVelocity", Math.Abs(playerRb.linearVelocity.x));
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    //определить какой прыжок
    private void JumpType()
    {
        bool isGrounded = IsOnGround();
        if (isGrounded)
        {
            Jump(jumpForce);
        }
        else
        {
            int direction = GetWallTouch();

            if (direction == 0 && canDoubleJump)
            {
                DoubleJump();
            }
            else if (direction != 0)
            {
                WallJump(direction);
            }

        }
    }

    //регать касание стены через рэйкаст
    private int GetWallTouch()
    {
        if (Physics2D.Raycast(transform.position, Vector2.right, halfPlayerWidth + 0.1f, LayerMask.GetMask("Wall")))
        {
            return -1;
        }
        if (Physics2D.Raycast(transform.position, Vector2.left, halfPlayerWidth + 0.1f, LayerMask.GetMask("Wall")))
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
        playerRb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        isJumping = true;
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
        isJumping = true;
        Debug.Log(isWallJumping);
        playerRb.AddForce(force, ForceMode2D.Impulse);
    }

    //чтобы чел съезжал вниз по стене, а не падал, когда я жму передвижение
    private void WallSlide()
    {
        if (!IsOnGround() && GetWallTouch() != 0 && horizontalInput != 0)
        {
            Debug.Log("touch");
            isWallSliding = true;
            isJumping = false;
            transform.localScale = new Vector3(GetWallTouch(), 1, 1);
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
