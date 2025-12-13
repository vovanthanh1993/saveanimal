using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script để điều khiển joystick UI động - xuất hiện tại vị trí touch
/// </summary>
public class DynamicJoystickUI : MonoBehaviour
{
    [Header("Joystick UI References")]
    [Tooltip("Background của joystick (vòng tròn lớn)")]
    public RectTransform background;
    
    [Tooltip("Handle của joystick (vòng tròn nhỏ di chuyển)")]
    public RectTransform handle;
    
    [Header("Settings")]
    [Tooltip("Bán kính tối đa joystick có thể di chuyển")]
    public float joystickRange = 50f;
    
    [Tooltip("Ẩn joystick khi không sử dụng")]
    public bool hideWhenNotUsed = true;
    
    private Canvas canvas;
    private Camera uiCamera;
    private Vector2 inputVector = Vector2.zero;
    private bool isActive = false;
    
    private void Awake()
    {
        // Đảm bảo GameObject này luôn active để script có thể chạy
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // Tự động tìm canvas nếu chưa được gán
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        // Lấy camera UI
        if (canvas != null)
        {
            uiCamera = canvas.worldCamera;
            if (uiCamera == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = null; // ScreenSpaceOverlay không cần camera
            }
        }
        
        // Ẩn joystick ban đầu nếu cần (chỉ ẩn background và handle, không ẩn GameObject chính)
        if (hideWhenNotUsed)
        {
            SetJoystickVisible(false);
        }
    }
    
    /// <summary>
    /// Đặt vị trí joystick tại vị trí screen (từ InputManager)
    /// </summary>
    public void SetPosition(Vector2 screenPosition)
    {
        // Đảm bảo GameObject active để script có thể chạy
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        if (background == null || canvas == null) return;
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            uiCamera,
            out localPoint);
        
        background.anchoredPosition = localPoint;
        
        // Reset handle về giữa
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
        
        SetJoystickVisible(true);
        isActive = true;
        inputVector = Vector2.zero;
    }
    
    /// <summary>
    /// Cập nhật vị trí handle khi drag (từ InputManager)
    /// </summary>
    public void UpdateHandlePosition(Vector2 screenPosition, Vector2 startScreenPosition)
    {
        if (!isActive || background == null || handle == null || canvas == null) return;
        
        // Chuyển đổi screen position sang local position
        Vector2 currentLocalPoint;
        Vector2 startLocalPoint;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            screenPosition,
            uiCamera,
            out currentLocalPoint);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            startScreenPosition,
            uiCamera,
            out startLocalPoint);
        
        // Tính delta trong local space
        Vector2 delta = currentLocalPoint - startLocalPoint;
        
        // Giới hạn trong phạm vi joystickRange
        inputVector = Vector2.ClampMagnitude(delta, joystickRange);
        handle.anchoredPosition = inputVector;
        
        // Chuẩn hóa input vector về [-1, 1]
        inputVector = inputVector / joystickRange;
    }
    
    /// <summary>
    /// Lấy input vector từ joystick (giá trị từ -1 đến 1)
    /// </summary>
    public Vector2 GetInputVector()
    {
        return inputVector;
    }
    
    /// <summary>
    /// Kiểm tra xem joystick có đang được sử dụng không
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Reset joystick
    /// </summary>
    public void ResetJoystick()
    {
        inputVector = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
        isActive = false;
        
        if (hideWhenNotUsed)
        {
            SetJoystickVisible(false);
        }
    }
    
    /// <summary>
    /// Hiển thị/ẩn joystick (chỉ ẩn background và handle, không ẩn GameObject chính)
    /// </summary>
    private void SetJoystickVisible(bool visible)
    {
        // Đảm bảo GameObject chính luôn active
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // Chỉ ẩn/hiện background và handle
        if (background != null)
        {
            background.gameObject.SetActive(visible);
        }
        if (handle != null)
        {
            handle.gameObject.SetActive(visible);
        }
    }
}

