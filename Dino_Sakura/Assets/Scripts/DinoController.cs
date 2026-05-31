using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DinoController : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float holdJumpForce = 18f;
    [SerializeField] private float maxHoldTime = 0.18f;
    [SerializeField] private bool allowDoubleJump = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.45f, 0.12f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Crouch")]
    [SerializeField] private Vector2 normalColliderSize = new Vector2(0.9f, 1.25f);
    [SerializeField] private Vector2 normalColliderOffset = new Vector2(0f, 0.02f);
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(1.2f, 0.72f);
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0.12f, -0.24f);
    [SerializeField] private float swipeDownThreshold = 70f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    public DinoState CurrentState { get; private set; }

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 touchStartPosition;
    private bool isGrounded;
    private bool isHoldingJump;
    private bool hasDoubleJumped;
    private float holdTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        SetState(DinoState.Idle);
        ApplyNormalCollider();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        CheckGround();
        HandleStateByGame();
        HandleInput();
        UpdateAnimator();
    }

    private void HandleStateByGame()
    {
        GameState gameState = GameManager.Instance.CurrentState;

        if (gameState == GameState.Idle)
        {
            SetState(DinoState.Idle);
            return;
        }

        if (gameState == GameState.Dead)
        {
            SetState(DinoState.Dead);
            return;
        }

        if (CurrentState == DinoState.Dead) return;

        if (isGrounded)
        {
            hasDoubleJumped = false;

            if (CurrentState != DinoState.Crouching)
            {
                SetState(DinoState.Running);
            }
        }
        else
        {
            SetState(rb.linearVelocity.y > 0.05f ? DinoState.Jumping : DinoState.Falling);
        }
    }

    private void HandleInput()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleKeyboardForEditor();
        HandleTouchForMobile();

        if (isHoldingJump && holdTimer < maxHoldTime && rb.linearVelocity.y > 0f)
        {
            holdTimer += Time.deltaTime;
            rb.AddForce(Vector2.up * holdJumpForce * Time.deltaTime, ForceMode2D.Impulse);
        }
    }

    private void HandleKeyboardForEditor()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            isHoldingJump = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isHoldingJump = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCrouch();
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            StopCrouch();
        }
    }

    private void HandleTouchForMobile()
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPosition = touch.position;
            TryJump();
            isHoldingJump = true;
        }

        if (touch.phase == TouchPhase.Moved)
        {
            float deltaY = touch.position.y - touchStartPosition.y;
            if (deltaY < -swipeDownThreshold)
            {
                StartCrouch();
                isHoldingJump = false;
            }
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isHoldingJump = false;
            StopCrouch();
        }
    }

    private void TryJump()
    {
        if (CurrentState == DinoState.Crouching)
        {
            StopCrouch();
        }

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJump();
            }
            holdTimer = 0f;
            isHoldingJump = true;
            SetState(DinoState.Jumping);
            return;
        }

        if (allowDoubleJump && !hasDoubleJumped)
        {
            hasDoubleJumped = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce * 0.9f, ForceMode2D.Impulse);
            holdTimer = maxHoldTime;
            SetState(DinoState.Jumping);
        }
    }

    private void StartCrouch()
    {
        if (!isGrounded) return;
        ApplyCrouchCollider();
        SetState(DinoState.Crouching);
    }

    private void StopCrouch()
    {
        ApplyNormalCollider();

        if (isGrounded && GameManager.Instance.CurrentState == GameState.Playing)
        {
            SetState(DinoState.Running);
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * 0.65f, groundCheckSize, 0f, groundLayer);
            return;
        }

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private void ApplyNormalCollider()
    {
        if (boxCollider == null) return;
        boxCollider.size = normalColliderSize;
        boxCollider.offset = normalColliderOffset;
    }

    private void ApplyCrouchCollider()
    {
        if (boxCollider == null) return;
        boxCollider.size = crouchColliderSize;
        boxCollider.offset = crouchColliderOffset;
    }

    private void SetState(DinoState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("IsIdle", CurrentState == DinoState.Idle);
        animator.SetBool("IsRunning", CurrentState == DinoState.Running);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", CurrentState == DinoState.Crouching);
        animator.SetBool("IsDead", CurrentState == DinoState.Dead);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance == null) return;
        ApplyNormalCollider();
        SetState(DinoState.Dead);
        GameManager.Instance.GameOver();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.65f;
        Gizmos.DrawWireCube(checkPos, groundCheckSize);
    }
}