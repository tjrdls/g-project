using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, ICombatEntity
{

    [Header("Sound")]
    public AudioClip HitSFX;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;

    [Header("Stats")]
    public StatsBaseSO baseStats;
    public StatsComponent stats;

    [Header("AI")]
    public Transform player;
    public float chaseRange = 5f;
    public float attackRange = 0.5f;
    public float moveSpeed = 2f;
    public float attackCooldown = 2f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.7f;

    public EnemyStateMachine stateMachine;

    public bool IsDead { get; private set; }
    public bool IsHit { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsInBreakState { get; private set; } = false;



    private Vector2 knockbackVelocity;
    private float knockbackTime = 0.4f;
    private float knockbackTimer = 0f;

    private PlayerMovementClass playerScript;

    private bool isJumpAttacking = false;
    public bool IsUsingSkill { get; set; }
    private float jumpForce = 8f;
    private float jumpHorizontalForce = 2.5f;

    public GameObject cardPanel;

    public bool isSkillCancelled = false;

    private void Awake()
    {
        stateMachine = new EnemyStateMachine();
    }

    private void Start()
    {
        if (!stats) stats = GetComponent<StatsComponent>();
        stats.Init(this, baseStats);

        if (player != null)
            playerScript = player.GetComponent<PlayerMovementClass>();

        stateMachine.ChangeState(new EnemyIdleState(this));

        cardPanel = GameObject.Find("EnemyCardPanel");
    }

    private void Update()
    {
        if (IsDead) return;

        CheckGround();

        if (isJumpAttacking)
        {
                DealDamage();
        }

        if (IsHit)
        {
            ApplyKnockback();
            UpdateAnimator();
            return;
        }

        stateMachine.Update();
        UpdateAnimator();
    }


    public bool CanSeePlayer()
    {
        return Vector2.Distance(player.position, transform.position) <= chaseRange;
    }

    public bool CanAttackPlayer()
    {
        return Vector2.Distance(player.position, transform.position) <= attackRange;
    }


    public void TakeDamage(float dmg)
    {
        if (IsDead) return;

        stats.TakeDamage(dmg);

        SoundManager.Instance.PlaySFX(HitSFX);

        if (!IsInBreakState)  // �극��ũ ���°� �ƴϸ� �˹� ����
        {
            IsHit = true;
            knockbackTimer = knockbackTime;

            Vector2 dir = (transform.position - player.position);
            dir.y = 0;
            dir = dir.normalized;

            knockbackVelocity = dir * 3f;
        }
    }

    private void ApplyKnockback()
    {
        rb.linearVelocity = knockbackVelocity;
        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0)
            IsHit = false;
    }

    public void JumpAttack()
    {
        isJumpAttacking = true;

        // �÷��̾� ���� ���
        float dir = (player.position.x > transform.position.x) ? 1f : -1f;

        // ���� + ������ �̵�
        rb.linearVelocity = new Vector2(dir * jumpHorizontalForce, jumpForce);


        Debug.Log("Enemy Jump Attack Started!");
    }


    public void DealDamage()
    {
        if (playerScript == null) return;

        float dist = Vector2.Distance(playerScript.transform.position, transform.position);
        if (dist <= attackRange)
        {
            playerScript.TakeDamage(stats.Attack);
            Debug.Log("Enemy hit player: " + stats.Attack);
        }
    }


    //setter
    public void StartAttack()
    {
        IsAttacking = true;
    }

    public void EndAttack()
    {
        IsAttacking = false;
    }

    public void EndSkill()
    {
        isJumpAttacking = false;
    }

    public void SetBreakState(bool isInBreak)
    {
        IsInBreakState = isInBreak;
    }

    //��� ó��
    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        stateMachine.ChangeState(new EnemyDeathState(this));
    }


    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        IsGrounded = hit.collider != null;
    }

    public void FaceTowards(float dir)
    {
        if (dir != 0)
            sr.flipX = dir < 0;
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;
        anim.SetBool("IsMoving", rb.linearVelocity.x != 0 && IsGrounded);
        anim.SetBool("IsJumping", !IsGrounded);
        anim.SetBool("IsAttack", IsAttacking);
        anim.SetBool("IsHit", IsHit);
    }

    public void DrawCards(List<Card> cards)
    {
        Debug.Log("Enemy Draw Cards: " + cards.Count);
        CardManager.instance.enemyCardDraw( cardPanel,cards);
    }
    public void Discard()
    {
        Debug.Log("Enemy Discard Cards");
        // �� ī�� �г� �� �ڽ� ����
        foreach (Transform child in cardPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

    }
}
