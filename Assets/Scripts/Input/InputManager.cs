using UnityEngine;
using ControlFreak2;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Control-Freak-2 Joystick (Optional)")]
    [Tooltip("Touch Joystick từ Control-Freak-2. Nếu để trống sẽ tự động tìm trong scene.")]
    public TouchJoystick touchJoystick;
    
    [Header("Input Settings")]
    [Tooltip("Tên axis Horizontal trong Input Manager (mặc định: Horizontal)")]
    public string horizontalAxis = "Horizontal";
    
    [Tooltip("Tên axis Vertical trong Input Manager (mặc định: Vertical)")]
    public string verticalAxis = "Vertical";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Tự động tìm TouchJoystick nếu chưa được assign
            if (touchJoystick == null)
            {
                touchJoystick = FindObjectOfType<TouchJoystick>();
                if (touchJoystick != null)
                {
                    Debug.Log($"[InputManager] Đã tìm thấy TouchJoystick: {touchJoystick.name}");
                }
            }
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Tìm lại TouchJoystick trong scene (dùng khi gameplay panel được kích hoạt)
    /// </summary>
    public void RefreshTouchJoystick()
    {
        if (touchJoystick == null || !touchJoystick.gameObject.activeInHierarchy)
        {
            // Tìm lại TouchJoystick, bao gồm cả những object đang inactive
            TouchJoystick[] allJoysticks = Resources.FindObjectsOfTypeAll<TouchJoystick>();
            if (allJoysticks != null && allJoysticks.Length > 0)
            {
                // Ưu tiên joystick đang active, nếu không có thì lấy joystick đầu tiên
                TouchJoystick activeJoystick = null;
                foreach (var joystick in allJoysticks)
                {
                    if (joystick.gameObject.activeInHierarchy)
                    {
                        activeJoystick = joystick;
                        break;
                    }
                }
                
                touchJoystick = activeJoystick != null ? activeJoystick : allJoysticks[0];
                if (touchJoystick != null)
                {
                    Debug.Log($"[InputManager] Đã tìm thấy TouchJoystick: {touchJoystick.name}");
                }
            }
        }
    }

    /// <summary>
    /// Lấy input vector di chuyển từ joystick hoặc Input Manager cũ
    /// </summary>
    public Vector2 InputMoveVector()
    {
        // Nếu chưa có joystick reference, thử tìm lại
        if (touchJoystick == null)
        {
            RefreshTouchJoystick();
        }
        
        // Ưu tiên lấy input từ Control-Freak-2 TouchJoystick nếu có và đang active
        if (touchJoystick != null && touchJoystick.gameObject.activeInHierarchy)
        {
            Vector2 joystickInput = touchJoystick.GetVector();
            // Nếu joystick đang được sử dụng (có input), trả về input từ joystick
            if (joystickInput.magnitude > 0.01f)
            {
                return joystickInput;
            }
        }
        
        // Fallback: Dùng Input Manager cũ (Input.GetAxis)
        float horizontal = Input.GetAxis(horizontalAxis);
        float vertical = Input.GetAxis(verticalAxis);
        return new Vector2(horizontal, vertical);
    }
    
    /// <summary>
    /// Tạm thời vô hiệu hóa input (không làm gì với Input Manager cũ, chỉ để tương thích với code cũ)
    /// </summary>
    public void DisablePlayerInput()
    {
        // Input Manager cũ không cần disable
        // Method này chỉ để tương thích với code khác đang gọi
    }
}