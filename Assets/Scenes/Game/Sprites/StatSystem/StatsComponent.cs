using UnityEngine;

public class StatsComponent : MonoBehaviour
{
    private ICombatEntity entity;
    private StatsBaseSO baseStats;

    public float CurrentHP { get; private set; }
    public float Attack => baseStats.attackPower;
    public float Defense => baseStats.defense;
    public float MaxHP => baseStats.maxHealth;

    public void Init(ICombatEntity owner, StatsBaseSO stats)
    {
        entity = owner;
        baseStats = stats;
        CurrentHP = MaxHP;
    }

    public void TakeDamage(float rawDamage)
    {
        float finalDamage = Mathf.Max(1, rawDamage - Defense);
        CurrentHP -= finalDamage;

        Debug.Log($"{name} 데미지 {finalDamage} 적용 → HP {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
        {
            entity.Die();
        }
    }
}
