using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillDemo_InputSystem : MonoBehaviour
{
    public GameObject explosionPrefab;
    public GameObject shieldPrefab;
    public Transform effectPoint;

    // PlayerInput 콜백
    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.performed) // 버튼이 눌렸을 때
        {
            CreateEffects();
        }
    }

    private void CreateEffects()
    {
        // 폭발 생성
        Instantiate(explosionPrefab, effectPoint.position, Quaternion.identity);

        // 보호막 생성
        GameObject shield = Instantiate(shieldPrefab, effectPoint.position, Quaternion.identity);
        shield.GetComponent<ShieldEffect>().target = this.transform;
    }
}
