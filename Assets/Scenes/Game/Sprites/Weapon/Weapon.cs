using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Collider2D weaponCollider;
    public SpriteRenderer weaponSprite;
    public float poseDuration = 0.1f;

    private bool facingRight = true;
    private StatsComponent playerStats;

    private void Awake()
    {
        playerStats = GetComponentInParent<StatsComponent>();

        // 시작할 때 무기 보이지 않게
        if (weaponSprite != null)
            weaponSprite.enabled = false;
        if (weaponCollider != null)
            weaponCollider.enabled = false;
    }

    public void SetFacingRight(bool right)
    {
        facingRight = right;
    }

    public void Attack()
    {
        // 공격할 때만 Sprite와 Collider 활성화
        if (weaponSprite != null) weaponSprite.enabled = true;
        if (weaponCollider != null) weaponCollider.enabled = true;

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        // 공격 포즈 좌표 및 회전
        Vector3 pose1Pos = new Vector3(0.42f, -0.33f, 0f);
        float pose1Rot = 130f;
        Vector3 pose2Pos = new Vector3(-0.38f, -0.39f, 0f);
        float pose2Rot = -30f;

        if (!facingRight)
        {
            pose1Pos.x *= -1;
            pose1Rot = -pose1Rot;
            pose2Pos.x *= -1;
            pose2Rot = -pose2Rot;
        }

        // 첫 번째 포즈
        transform.localPosition = pose1Pos;
        transform.localRotation = Quaternion.Euler(0f, 0f, pose1Rot);
        yield return new WaitForSeconds(poseDuration);

        // 두 번째 포즈
        transform.localPosition = pose2Pos;
        transform.localRotation = Quaternion.Euler(0f, 0f, pose2Rot);
        yield return new WaitForSeconds(poseDuration);

        // 공격 종료 후 Sprite와 Collider 끄기
        if (weaponSprite != null) weaponSprite.enabled = false;
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null && playerStats != null)
        {
            Vector2 knockbackDir = (enemy.transform.position - playerStats.transform.position).normalized;
            enemy.TakeDamage(playerStats.Attack);
        }
    }

}
