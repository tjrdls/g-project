using UnityEngine;

public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }

    public void ChangeState(IEnemyState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}

