using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    public Transform target;       // 플레이어
    public float duration = 2f;    // 보호막 표시 시간

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void Update()
    {
        if (target != null)
            transform.position = target.position;
    }
}
