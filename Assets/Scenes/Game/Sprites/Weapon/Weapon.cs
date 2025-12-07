using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public SpriteRenderer weaponSprite;
    public float poseDuration = 0.1f;
    public SpriteRenderer slashEffect;
    public SpriteRenderer skill1Effect;

    [Header("Sound(SFX) Settings")]
    public AudioClip slashSFX;


    private bool facingRight = true;



    private void Awake()
    {
        // 시작할 때 무기 보이지 않게
        if (weaponSprite != null)
            weaponSprite.enabled = false;
    }

    public void SetFacingRight(bool right)
    {
        facingRight = right;
    }

    public void Attack()
    {
        // 공격할 때만 Sprite와 Collider 활성화
        if (weaponSprite != null) weaponSprite.enabled = true;

        StartCoroutine(DoAttack());
    }

    public void Skill()
    {
        // 공격할 때만 Sprite와 Collider 활성화
        if (weaponSprite != null) weaponSprite.enabled = true;

        StartCoroutine(DoSkill());
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

        //slashsfx 재생
        SoundManager.Instance.PlaySFX(slashSFX);


        // 슬래시 이펙트 켜기 + 방향 맞추기
        if (slashEffect != null)
        {
            slashEffect.enabled = true;

            // SpriteRenderer를 뒤집기
            slashEffect.flipX = !facingRight;
        }

        yield return new WaitForSeconds(poseDuration);

        // 공격 종료 후 Sprite와 Collider 끄기
        if (weaponSprite != null) weaponSprite.enabled = false;
        if (slashEffect != null) slashEffect.enabled = false;
    }

    private IEnumerator DoSkill()
    {
        // 공격 포즈 좌표 및 회전
        Vector3 pose1Pos = new Vector3(0.6f, 0.188f, 0f);
        float pose1Rot = 160f;
        Vector3 pose2Pos = new Vector3(-0.63f, 0.21f, 0f);
        float pose2Rot = -70f;

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
        if (skill1Effect != null)
        {
            skill1Effect.enabled = true;

            // SpriteRenderer를 방향에 맞춰 뒤집기
            skill1Effect.flipX = facingRight;
        }
        yield return new WaitForSeconds(poseDuration);

        // 두 번째 포즈
        transform.localPosition = pose2Pos;
        transform.localRotation = Quaternion.Euler(0f, 0f, pose2Rot);


        yield return new WaitForSeconds(poseDuration);

        // 공격 종료 후 Sprite와 Collider 끄기
        if (weaponSprite != null) weaponSprite.enabled = false;
        if (skill1Effect != null) skill1Effect.enabled = false;
    }

}
