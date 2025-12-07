using System.Diagnostics;
using UnityEngine;

public class SlashFX : MonoBehaviour
{
    public Collider2D SlashCollider;
    SpriteRenderer sr;
    Animator anim;
    bool wasVisible = false;

    public StatsComponent playerStats;

    public bool isSkill = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerStats = GetComponentInParent<StatsComponent>();

        if (sr != null)
            sr.enabled = false;
        if (SlashCollider != null)
            SlashCollider.enabled = false;
    }

    void Update()
    {
        if (!wasVisible && sr.enabled)
        {
            if (SlashCollider != null)
                SlashCollider.enabled = true;
            // Animator
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                anim.Rebind();
                anim.Update(0f);
            }
        }

        wasVisible = sr.enabled;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            UnityEngine.Debug.Log("SlashFX hit enemy: " + enemy.name);
            CardManager.instance.DetermineWinner(enemy.gameObject);
            enemy.TakeDamage(playerStats.Attack);
            
        }
        if (SlashCollider != null)
            SlashCollider.enabled = false;
    }
}