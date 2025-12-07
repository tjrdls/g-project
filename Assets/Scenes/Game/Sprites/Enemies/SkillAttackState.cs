using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;


public class EnemySkillAttackState : IEnemyState
{
    private Enemy enemy;
    private float skillCooldown = 3f;
    private float elapsed = 0f;
    private bool jumpStarted = false;

    public EnemySkillAttackState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.IsUsingSkill = true;
        elapsed = 0f;
        jumpStarted = false;
        enemy.isSkillCancelled = false;
        // 1) 스킬 발동 전에 카드 드로우

        Debug.Log("Enemy drew cards and is charging a skill...");
        List<Card> cardList = new List<Card>();
        // spade A 만 넣으면 RoyalFlush를 테스트해 볼 수 있도록 함
        cardList.Add(new Card(Card.Suit.Spade, 10));
        cardList.Add(new Card(Card.Suit.Spade, 11));
        cardList.Add(new Card(Card.Suit.Spade, 12));
        cardList.Add(new Card(Card.Suit.Spade, 13));
        enemy.DrawCards(cardList);

        // 점프 공격 전 준비시간
        Debug.Log("Enemy is charging a skill...");
    }

    public void Update()
    {
        elapsed += Time.deltaTime;

        // 1) 준비 시간 동안 가만히 있음
        if (!jumpStarted && elapsed >= skillCooldown)
        {
            // 대기 시간동안 applyEffect가 호출됬으면 스킬 공격 취소
            if (enemy.isSkillCancelled)
            {
                Debug.Log("Enemy's skill attack was cancelled!");
                enemy.EndSkill();
                enemy.EndAttack();   // 혹시 공격 flag가 true였다면 제거
                enemy.Discard();
                enemy.stateMachine.ChangeState(new EnemyIdleState(enemy));
                return;
            }

            jumpStarted = true;
           
            enemy.JumpAttack(); // 실제 점프 공격 시작
            Debug.Log("Enemy performs a jump attack!");
        }

        // 2) 점프 공격이 시작된 후, 착지하면 종료
        if (jumpStarted && enemy.IsGrounded)
        {
            Debug.Log("Enemy landed from skill attack.");
            enemy.EndSkill();
            enemy.EndAttack();   // 혹시 공격 flag가 true였다면 제거
            enemy.Discard();
            enemy.stateMachine.ChangeState(new EnemyIdleState(enemy));
        }
    }

    
    public void Exit()
    {
        enemy.IsUsingSkill = false;
        Debug.Log("Enemy ends skill attack.");
    }
}
