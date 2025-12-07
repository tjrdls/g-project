using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용

public class Player : MonoBehaviour
{
    public Weapon weapon;       // Weapon 오브젝트 연결
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveInput; // 입력 값 저장

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {
        // moveInput.x 값으로 좌우 이동
        float h = moveInput.x;


        // 좌우 반전
        if (h != 0)
            sr.flipX = h < 0;

        // Weapon 방향 반전
        if (weapon != null)
            weapon.SetFacingRight(h >= 0);
    }

    // Animation Event에서 호출
    public void TriggerWeaponAttack()
    {
        weapon?.Attack();
    }

    public void TriggerWeaponSkill()
    {
        weapon?.Skill();
    }

    // Input System 콜백
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
