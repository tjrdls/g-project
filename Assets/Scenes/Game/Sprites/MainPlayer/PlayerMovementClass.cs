using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementClass : MonoBehaviour, ICombatEntity
{
    [Header("Stats")]
    public StatsBaseSO baseStats;
    public StatsComponent stats;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInputActions inputActions;

    [Header("Wall Unstuck Settings")]
    public float wallUnstuckHeight = 0.1f; // 얼마나 위로 올릴지
    public float wallCheckDistance = 0.1f;  // 벽 감지 거리

    [Header("Hit Settings")]
    public float knockbackX = 5f; // 뒤로 밀리는 힘
    public float knockbackY = 3f; // 위로 띄우는 힘
    public float hitDuration = 0.5f;
    public float invincibleDuration = 0.5f;
    public Color hitColor = new Color(1f, 1f, 1f, 0.5f);

    private Vector2 currentKnockback = Vector2.zero;
    private float knockbackTimer = 0f;
    private float invincibleTimer = 0f;
    private bool isKnockbackActive = false;  // 넉백 상태
    private bool isInvincible = false;       // 무적 상태


    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Evade Settings")]
    public float evadeSpeed = 11f;
    public float evadeDuration = 0.3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float moveInput;
    private bool isGrounded;
    private bool isJumping;
    private bool isFalling;
    private bool isAttacking;
    private bool isEvading;
    private bool isHit = false;
    private float evadeTimer = 0f;
    private float evadeDirection;

    public bool facingRight = false; // 플레이어가 오른쪽을 보고 있는지

    private void Start()
    {
        stats.Init(this, baseStats);
    }

    private void Awake()
    {
        InitializeComponents();
        InitializeInput();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void FixedUpdate()
    {
        HandleMovement();
        HandleDirection();
        HandleEvade();
    }

    private void Update()
    {
        if (isKnockbackActive)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isHit = false;
                isKnockbackActive = false;
                currentKnockback = Vector2.zero;
            }
        }

        // 무적 타이머
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                spriteRenderer.color = Color.white;
            }
        }
        CheckGround();
        UpdateJumpFallState();
        UpdateAnimator();
    }

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!stats) stats = GetComponent<StatsComponent>();
    }

    private void InitializeInput()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        inputActions.Player.Move.canceled += ctx => moveInput = 0f;

        inputActions.Player.Jump.performed += ctx => TryJump();
        inputActions.Player.Attack.performed += ctx => TryAttack();
        inputActions.Player.Evade.performed += ctx => TryEvade();
    }
    #endregion

    #region Core Movement
    private void HandleMovement()
    {
        Vector2 velocity = rb.linearVelocity;


        // 공격/회피 중이면 이동 무시
        if (isAttacking || isEvading) return;

        if (isKnockbackActive)
        {
            // 넉백 적용
            velocity = currentKnockback;
        }
        else
        {
            // 일반 이동
            velocity.x = moveInput * moveSpeed;
        }

        rb.linearVelocity = velocity;
    }

    private void HandleDirection()
    {
        if (isAttacking || isEvading || isHit) return;

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = true;
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = false;
        }
    }

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            isFalling = false;
        }
    }

    public void TryJump()
    {
        if (!isGrounded || isAttacking || isEvading || isHit) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        isJumping = true;
        isFalling = false;
    }
    #endregion

    private void WallUnstuckCheck()
    {
        // 점프 중에는 벽 끼임 방지 비활성
        if (isJumping || isEvading || isHit) return;

        Vector2 origin = rb.position;

        // 오른쪽 체크
        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, groundLayer);
        // 왼쪽 체크
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, groundLayer);

        if (hitRight.collider != null || hitLeft.collider != null)
        {
            // 벽에 끼었으면 살짝 위로 이동
            rb.MovePosition(rb.position + new Vector2(0f, wallUnstuckHeight));
        }
    }


    #region Hit
    public void TakeDamage(float rawDamage)
    {
        if (isInvincible) return;

        // 실제 체력 계산은 StatsComponent가 수행(방어력 포함)
        stats.TakeDamage(rawDamage);

        // 넉백 방향: 캐릭터가 보는 반대 방향
        currentKnockback = facingRight ? new Vector2(-knockbackX, knockbackY) : new Vector2(knockbackX, knockbackY);

        // 넉백 시작
        isHit = true;
        isKnockbackActive = true;
        knockbackTimer = hitDuration;

        // 무적 시작
        isInvincible = true;
        invincibleTimer = invincibleDuration;

        // 색상 변경
        spriteRenderer.color = hitColor;
    }

    public void Die()
    {
        Debug.Log("플레이어 사망");
        // 애니메이션 또는 효과 추가 예정
        rb.linearVelocity = Vector2.zero;
        enabled = false;
    }

    #endregion

    #region Attack
    private void TryAttack()
    {
        if (!isGrounded || isAttacking || isEvading || isHit) return;

        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        moveInput = 0f;

        // 공격 애니메이션 시작
        animator.Play("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
        moveInput = inputActions.Player.Move.ReadValue<float>();
    }
    #endregion


    #region Evade
    private void HandleEvade()
    {
        if (!isEvading) return;

        evadeTimer -= Time.fixedDeltaTime;
        if (evadeTimer <= 0f)
        {
            EndEvade();
            return;
        }

        rb.linearVelocity = new Vector2(evadeDirection * evadeSpeed, rb.linearVelocity.y);

    }

    private void TryEvade()
    {
        if (isEvading || isAttacking || !isGrounded || isHit) return;

        isEvading = true;

        evadeTimer = evadeDuration;

        float inputDir = inputActions.Player.Move.ReadValue<float>();
        // 입력 없으면 뒤로 회피
        evadeDirection = Mathf.Abs(inputDir) > 0.1f ? Mathf.Sign(inputDir) : (facingRight ? -1f : 1f);

        animator.Play("Evade");

    }

    public void EndEvade()
    {
        isEvading = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    #endregion

    #region Jump/Fall
    private void UpdateJumpFallState()
    {
        if (isGrounded)
        {
            isJumping = false;
            isFalling = false;
            return;
        }

        if (rb.linearVelocity.y > 0.1f)
        {
            isJumping = true;
            isFalling = false;
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            isJumping = false;
            isFalling = true;
        }
    }
    #endregion

    #region Animator
    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsEvading", isEvading);
    }
    #endregion

}

