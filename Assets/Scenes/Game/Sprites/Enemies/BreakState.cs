using UnityEngine;

public class EnemyBreakState : IEnemyState
{
    private Enemy enemy;
    private float breakDuration = 5f; // 브레이크 지속 시간
    private float breakTimer = 0f; // 타이머

    public EnemyBreakState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.SetBreakState(true);
        breakTimer = breakDuration; // 타이머 초기화
        enemy.rb.linearVelocity = Vector2.zero; // 움직임 정지
        enemy.sr.material.color = new Color(1f, 1f, 1f, 0.5f); // 투명도 50% 설정
        Debug.Log("Enemy is now in break state.");
    }

    public void Update()
    {
        breakTimer -= Time.deltaTime; // 타이머 감소

        if (breakTimer <= 0)
        {
            enemy.stateMachine.ChangeState(new EnemyIdleState(enemy)); // 시간이 지나면 Idle로 돌아감
        }
    }

    public void Exit()
    {
        enemy.SetBreakState(false);
        enemy.sr.material.color = Color.white; // 원래 색으로 복구
        Debug.Log("Enemy is leaving break state.");
    }
}
