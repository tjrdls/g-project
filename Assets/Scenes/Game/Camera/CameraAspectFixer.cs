using UnityEngine;

public class CameraAspectFixer : MonoBehaviour
{
    private readonly float targetAspect = 16f / 9f;

    void Awake()
    {
        SetupBackgroundCamera();
        FixAspectRatio();
    }

    //카메라 비율 고정
    private void FixAspectRatio()
    {
        Camera cam = GetComponent<Camera>();

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }

        cam.clearFlags = CameraClearFlags.Depth;  // MainCamera는 백그라운드 지우지 않음
        cam.depth = 0;
    }

    //버퍼 여백 제거용
    private void SetupBackgroundCamera()
    {
        // 이미 존재한다면 생성 X
        if (GameObject.Find("__BackgroundCamera__") != null)
            return;

        GameObject bgCamObj = new GameObject("__BackgroundCamera__");
        bgCamObj.transform.position = new Vector3(0, 0, -10);

        Camera bgCam = bgCamObj.AddComponent<Camera>();
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0;  // 아무 오브젝트도 렌더하지 않음
        bgCam.depth = -10;      // MainCamera보다 먼저 렌더
        bgCam.rect = new Rect(0, 0, 1, 1); // 전체 화면 채움
    }
}

