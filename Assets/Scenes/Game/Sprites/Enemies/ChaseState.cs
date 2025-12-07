using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private Enemy enemy;

    public EnemyChaseState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.EndAttack();
    }

    public void Update()
    {
        if (enemy.IsHit) return;  // 맞았다면 추적하지 않음

        // 플레이어 방향 계산하여 적의 방향을 플레이어로 향하게 함
        float dir = enemy.player.position.x - enemy.transform.position.x;
        enemy.FaceTowards(dir);
        enemy.rb.linearVelocity = new UnityEngine.Vector2(Mathf.Sign(dir) * enemy.moveSpeed, enemy.rb.linearVelocity.y);

        // 공격 범위에 들어왔으면 공격 상태로 전환

        if (enemy.CanAttackPlayer())
        {
            // 점프 공격 중이면 중복 스킬 금지
            if (enemy.CanAttackPlayer() && !enemy.IsUsingSkill)
            {
                float rand = Random.value;

                if (rand < 0.5f)
                    enemy.stateMachine.ChangeState(new EnemyAttackState(enemy));
                else
                    enemy.stateMachine.ChangeState(new EnemySkillAttackState(enemy));
            }
        }

        // 플레이어를 볼 수 없으면 Idle 상태로 전환
        else if (!enemy.CanSeePlayer())
        {
            enemy.stateMachine.ChangeState(new EnemyIdleState(enemy));
        }
    }

    public void Exit()
    {
        enemy.rb.linearVelocity = Vector2.zero;
    }
}
