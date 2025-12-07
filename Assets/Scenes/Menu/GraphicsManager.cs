using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    public static GraphicsManager Instance;

    // 실제 해상도 목록
    private readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(720, 480),
    };

    public int ResolutionCount => resolutions.Length;

    private void Awake()
    {
        Instance = this;
    }

    // Apply 시에 호출됨
    public void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;

        Screen.SetResolution(resolutions[index].x, resolutions[index].y, false);

        Debug.Log($"Resolution Applied: {resolutions[index]}");
    }
}
