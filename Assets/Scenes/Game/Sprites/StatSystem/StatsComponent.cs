using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class StatsComponent : MonoBehaviour
{
    private ICombatEntity entity;
    private StatsBaseSO baseStats;

    public float CurrentHP { get; private set; }
    public float Attack => baseStats.attackPower;
    public float Defense => baseStats.defense;
    public float MaxHP => baseStats.maxHealth;

    // HP
    public System.Action<float, float> OnHealthChanged;

    // 2배 데미지 효과 활성화 여부
    private bool isDoubleDamage = false;

    public void Init(ICombatEntity owner, StatsBaseSO stats)
    {
        entity = owner;
        baseStats = stats;
        CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(float rawDamage)
    {
        // 2배 데미지 효과가 활성화되어 있으면 데미지를 2배로 적용
        if (isDoubleDamage)
        {
            rawDamage *= 2;
        }

        float finalDamage = Mathf.Max(1, rawDamage - Defense);
        CurrentHP -= finalDamage;

        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        Debug.Log($"{name} 이가 {finalDamage} 로 공격함 HP {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
        {
            entity.Die();
        }
    }

    public void applyEffect(HandEvaluator.HandRank handRank)
    {
        Enemy enemy = this.gameObject.GetComponent<Enemy>();
        if (enemy == null) {
            Debug.Log("적이 아닌 대상에게 카드 효과 적용 불가");
            return;
        }
        enemy.isSkillCancelled = true;
        enemy.Discard();

        switch (handRank)
        {
            case HandEvaluator.HandRank.OnePair:
                Debug.Log("카드 효과 적용");
                break;
            case HandEvaluator.HandRank.TwoPair:
                break;
            case HandEvaluator.HandRank.ThreeOfAKind:
                break;
            case HandEvaluator.HandRank.Straight:
                break;
            case HandEvaluator.HandRank.Flush:
                break;
            case HandEvaluator.HandRank.FullHouse:
                break;
            case HandEvaluator.HandRank.FourOfAKind:
                break;
            case HandEvaluator.HandRank.StraightFlush:
                break;
            case HandEvaluator.HandRank.RoyalFlush:
                // 2배 데미지 효과 적용
                StartCoroutine(ApplyDoubleDamageEffect(3f));
                Debug.Log("Royal Flush! 2배 데미지 효과 적용.");

                // ★ 브레이크 상태로 전환
                if (enemy.stateMachine != null)
                {
                    enemy.stateMachine.ChangeState(new EnemyBreakState(enemy));
                    Debug.Log("Royal Flush! Enemy Enter Break State.");
                }
                else
                {
                    Debug.LogWarning("Enemy stateMachine 이 null 입니다!");
                }
                break;
        }
    }

    private IEnumerator ApplyDoubleDamageEffect(float duration)
    {
        // 2배 데미지 활성화
        isDoubleDamage = true;

        // 주어진 시간(duration) 동안 대기
        yield return new WaitForSeconds(duration);

        // 2배 데미지 비활성화
        isDoubleDamage = false;
        Debug.Log("2배 데미지 효과 종료.");
    }
}
