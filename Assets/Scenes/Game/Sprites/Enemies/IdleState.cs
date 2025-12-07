using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private Enemy enemy;

    public EnemyIdleState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        // 이동, 공격 등 초기화
        enemy.EndSkill();
        enemy.EndAttack();
    }

    public void Update()
    {
        if (enemy.CanSeePlayer())
        {
            enemy.stateMachine.ChangeState(new EnemyChaseState(enemy));
        }
    }

    public void Exit() { }
}
