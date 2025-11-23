using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player & Offset")]
    public Transform player;
    public Vector2 offset = new Vector2(0, 1f);

    [Header("Camera Bounds")]
    public float minX, maxX;
    public float minY, maxY;

    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSmoothTime = 0.1f; // 빠르게 반응
    public float lookAheadThreshold = 0.01f;

    [Header("Vertical Follow")]
    public float verticalSmoothTime = 0.2f;
    public float verticalOffset = 0f;

    private Vector2 lastPlayerPosition;
    private float lookAheadOffsetX = 0f;
    private float lookAheadVelocity = 0f;
    private float verticalVelocity = 0f;

    void Start()
    {
        if (player != null)
            lastPlayerPosition = player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 1. 플레이어 이동 속도 기반 Look Ahead
        float playerVelocityX = (player.position.x - lastPlayerPosition.x) / Time.deltaTime;
        float targetLookAheadX = Mathf.Abs(playerVelocityX) > lookAheadThreshold
                                 ? Mathf.Sign(playerVelocityX) * lookAheadDistance
                                 : 0f;

        lookAheadOffsetX = Mathf.SmoothDamp(lookAheadOffsetX, targetLookAheadX, ref lookAheadVelocity, lookAheadSmoothTime);

        // 2. x축 위치 = 플레이어 + offset + Look Ahead (즉시)
        float targetX = player.position.x + offset.x + lookAheadOffsetX;

        // 3. y축 위치 = 플레이어 + offset + verticalOffset (SmoothDamp 적용 가능)
        float targetY = player.position.y + offset.y + verticalOffset;
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, verticalSmoothTime);

        // 4. 화면 경계 제한
        targetX = Mathf.Clamp(targetX, minX, maxX);
        smoothY = Mathf.Clamp(smoothY, minY, maxY);

        // 5. 카메라 위치 적용
        transform.position = new Vector3(targetX, smoothY, transform.position.z);

        // 6. 플레이어 위치 저장
        lastPlayerPosition = player.position;
    }
}
