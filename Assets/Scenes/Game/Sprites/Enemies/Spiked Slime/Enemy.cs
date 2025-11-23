using UnityEngine;

public class Enemy : MonoBehaviour, ICombatEntity
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;

    [Header("Stats")]
    public StatsBaseSO baseStats;
    public StatsComponent stats;

    [Header("Movement & Attack Settings")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float attackRange = 0.5f;
    public float attackCooldown = 3f;

    [Header("AI Behaviour")]
    public Transform player;
    public float chaseRange = 5f;
    public float jumpInterval = 3f;
    public LayerMask groundLayer;

    private float lastAttackTime = 0f;
    private float nextJumpTime = 0f;

    private bool isDead = false;
    private bool isHit = false;
    private bool isGrounded = false;
    private bool isAttacking = false;

    private Vector2 knockbackVelocity;
    private float knockbackTime = 0.2f;
    private float knockbackTimer = 0f;

    private PlayerMovementClass playerScript;

    private void Start()
    {
        // Stats 시스템 초기화
        if (!stats) stats = GetComponent<StatsComponent>();
        stats.Init(this, baseStats);   // 스탯 초기화

        if (player != null)
            playerScript = player.GetComponent<PlayerMovementClass>();
    }

    private void Update()
    {
        if (isDead) return;

        CheckGround();

        // 피격 중이면 넉백 처리 우선
        if (isHit)
        {
            ApplyKnockback();
            return;
        }

        if (isAttacking) return;

        float distX = player.position.x - transform.position.x;
        float absX = Mathf.Abs(distX);

        // 공격 범위 안이면 공격
        if (absX <= attackRange)
        {
            TryAttack();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("IsMoving", false);
            return;
        }

        // 추격 범위 검사
        if (absX <= chaseRange)
        {
            if (Time.time >= nextJumpTime && isGrounded &&
                (player.position.y - transform.position.y > 0.5f))
            {
                JumpTowardsPlayer();
                nextJumpTime = Time.time + jumpInterval + Random.Range(-1f, 1f);
            }

            MoveTowardsPlayer(distX);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("IsMoving", false);
        }
    }

    private void MoveTowardsPlayer(float distX)
    {
        float dir = Mathf.Sign(distX);

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        anim.SetBool("IsMoving", isGrounded && !isAttacking);

        if (dir != 0)
            sr.flipX = dir < 0;
    }

    private void JumpTowardsPlayer()
    {
        if (!isGrounded) return;

        anim.SetBool("IsJump", true);
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    // Animator Event에서 호출
    public void DealDamage()
    {
        if (playerScript == null) return;

        float dist = Vector2.Distance(playerScript.transform.position, transform.position);
        if (dist <= attackRange)
        {
            Vector2 knockbackDir = (playerScript.transform.position - transform.position).normalized; //넉백 계산 @수정중@
            playerScript.TakeDamage(stats.Attack);
            Debug.Log("슬라임이 플레이어 공격: " + stats.Attack);
        }
    }

    private void TryAttack()
    {
        if (isHit) return;
        if (Time.time - lastAttackTime < attackCooldown || isAttacking)
            return;

        lastAttackTime = Time.time;
        anim.SetTrigger("IsAttack");
        isAttacking = true;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsJump", false);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, groundLayer);
        isGrounded = hit.collider != null;
        anim.SetBool("IsJump", !isGrounded);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        // Stats 기반 데미지 계산 (방어력 적용)
        stats.TakeDamage(dmg);

        isHit = true;
        knockbackTimer = knockbackTime;

        anim.SetBool("IsMoving", false);
        anim.SetTrigger("Hit");
    }

    private void ApplyKnockback()
    {
        rb.linearVelocity = knockbackVelocity;
        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0f)
            isHit = false;
    }

    public void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.7f);
    }
}
