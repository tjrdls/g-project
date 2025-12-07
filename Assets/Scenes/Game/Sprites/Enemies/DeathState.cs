using UnityEngine;

public class EnemyDeathState : IEnemyState
{
    private Enemy enemy;

    public EnemyDeathState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.anim.SetTrigger("Death");
        enemy.rb.linearVelocity = Vector2.zero;
        enemy.GetComponent<Collider2D>().enabled = false;

        // ������Ʈ ����
        GameObject.Destroy(enemy.gameObject, 1f);
    }

    public void Update() { }

    public void Exit() { }
}


