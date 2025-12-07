using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private Enemy enemy;
    private float attackDuration = 1f;
    private float timer = 0f;

    public EnemyAttackState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        timer = 0f;
        enemy.StartAttack();  // Animator: IsAttack = true
        enemy.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if (enemy.CanAttackPlayer())
            enemy.DealDamage();

        if (timer >= attackDuration)
        {
            enemy.EndAttack(); // Animator: IsAttack = false
            enemy.stateMachine.ChangeState(new EnemyIdleState(enemy));
        }
    }

    public void Exit()
    {
        enemy.EndAttack();
    }
}
