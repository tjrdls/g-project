using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("Teleport_value")]
    public Vector3 targetPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        collision.transform.position = targetPosition;
    }
}
