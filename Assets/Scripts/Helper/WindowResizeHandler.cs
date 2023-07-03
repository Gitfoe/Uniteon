using UnityEngine;

/// <summary>
/// Ensures the game always stays at the same aspect ratio regardless of window size.
/// </summary>
public class WindowResizeHandler : MonoBehaviour
{
    [SerializeField] float targetAspect = 4f / 3f;
    private Camera _mainCamera;
    
    private void Awake()
    {
        _mainCamera = GetComponentInParent<Camera>();
        UpdateCameraViewport();
    }

    private void Update()
    {
        if (Mathf.Abs(_mainCamera.aspect - targetAspect) > 0.01f)
        {
            UpdateCameraViewport();
        }
    }

    private void UpdateCameraViewport()
    {
        float targetHeight = _mainCamera.orthographicSize * 2f;
        float targetWidth = targetHeight * targetAspect;
        float currentWidth = Screen.width;
        float currentHeight = Screen.height;
        float widthScaleFactor = currentWidth / targetWidth;
        float heightScaleFactor = currentHeight / targetHeight;
        float scale = Mathf.Min(widthScaleFactor, heightScaleFactor);
        Rect rect = _mainCamera.rect;
        rect.width = targetWidth * scale / currentWidth;
        rect.height = targetHeight * scale / currentHeight;
        rect.x = (1f - rect.width) / 2f;
        rect.y = (1f - rect.height) / 2f;
        _mainCamera.rect = rect;
    }
}