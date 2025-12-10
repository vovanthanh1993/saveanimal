using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputSystem_Actions InputSystem;
    
    [Header("Look Input Settings")]
    [Tooltip("Chỉ cho phép Look input từ pointer (mouse/touch) khi ở nửa màn hình bên phải")]
    public bool restrictLookToRightHalf = true;
    
    // Multi-touch tracking
    private Vector2 lastRightHalfTouchPosition = Vector2.zero;
    private int rightHalfTouchId = -1;

    private void OnEnable()
    {
        
    }

    /*private void OnDisable()
    {
        InputSystem.Disable();
    }*/

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InputSystem = new InputSystem_Actions();
            InputSystem.Enable();
            
            // Enable Enhanced Touch để hỗ trợ multi-touch tốt hơn
            EnhancedTouchSupport.Enable();
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            EnhancedTouchSupport.Disable();
        }
    }

    public void DisablePlayerInput()
    {
        InputSystem.Player.Disable();
    }

    public Vector2 InputMoveVector()
    {
        return InputSystem.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 InputLookVector()
    {
        // Nếu không restrict, trả về input bình thường
        if (!restrictLookToRightHalf)
        {
            return InputSystem.Player.Look.ReadValue<Vector2>();
        }
        
        // Kiểm tra mouse
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // Nếu mouse đang di chuyển, kiểm tra vị trí
            if (mouseDelta.magnitude > 0.01f)
            {
                float screenX = mousePos.x / Screen.width;
                if (screenX >= 0.5f)
                {
                    // Mouse ở nửa màn hình bên phải - cho phép input
                    return mouseDelta;
                }
                else
                {
                    // Mouse ở nửa màn hình bên trái - chặn input
                    return Vector2.zero;
                }
            }
        }
        
        // Xử lý multi-touch - chỉ tính toán delta từ touch ở nửa phải
        Vector2 touchLookDelta = GetRightHalfTouchLookDelta();
        if (touchLookDelta.magnitude > 0.01f)
        {
            return touchLookDelta;
        }
        
        // Kiểm tra xem có touch ở nửa trái không - nếu có thì chặn look input
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            // Có touch nhưng không phải ở nửa phải - chặn look input
            return Vector2.zero;
        }
        
        // Nếu không có pointer input (gamepad, joystick), trả về input từ Input System
        return InputSystem.Player.Look.ReadValue<Vector2>();
    }
    
    /// <summary>
    /// Tính toán look delta từ touch ở nửa màn hình bên phải (hỗ trợ multi-touch)
    /// </summary>
    private Vector2 GetRightHalfTouchLookDelta()
    {
        if (Touchscreen.current == null)
        {
            rightHalfTouchId = -1;
            lastRightHalfTouchPosition = Vector2.zero;
            return Vector2.zero;
        }
        
        var touches = Touchscreen.current.touches;
        if (touches.Count == 0)
        {
            rightHalfTouchId = -1;
            lastRightHalfTouchPosition = Vector2.zero;
            return Vector2.zero;
        }
        
        // Tìm touch ở nửa phải
        TouchControl rightHalfTouch = null;
        Vector2 rightHalfTouchPosition = Vector2.zero;
        
        // Nếu đã có touch ID được track, tìm touch đó trước
        if (rightHalfTouchId >= 0)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var touchId = touch.touchId.ReadValue();
                if (touchId == rightHalfTouchId)
                {
                    var position = touch.position.ReadValue();
                    float screenX = position.x / Screen.width;
                    
                    // Kiểm tra xem touch có vẫn ở nửa phải không
                    if (screenX >= 0.5f)
                    {
                        rightHalfTouch = touch;
                        rightHalfTouchPosition = position;
                        break;
                    }
                    else
                    {
                        // Touch đã di chuyển ra khỏi nửa phải - reset
                        rightHalfTouchId = -1;
                        lastRightHalfTouchPosition = Vector2.zero;
                    }
                }
            }
        }
        
        // Nếu không tìm thấy touch đã track, tìm touch mới ở nửa phải
        if (rightHalfTouch == null)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                var position = touch.position.ReadValue();
                var phase = touch.phase.ReadValue();
                float screenX = position.x / Screen.width;
                
                // Tìm touch ở nửa phải và đang bắt đầu hoặc di chuyển
                if (screenX >= 0.5f && 
                    (phase == UnityEngine.InputSystem.TouchPhase.Began || 
                     phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                     phase == UnityEngine.InputSystem.TouchPhase.Stationary))
                {
                    rightHalfTouch = touch;
                    rightHalfTouchPosition = position;
                    rightHalfTouchId = touch.touchId.ReadValue();
                    break;
                }
            }
        }
        
        // Tính toán delta
        if (rightHalfTouch != null)
        {
            var phase = rightHalfTouch.phase.ReadValue();
            
            // Nếu touch đã kết thúc, reset
            if (phase == UnityEngine.InputSystem.TouchPhase.Ended || 
                phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                rightHalfTouchId = -1;
                lastRightHalfTouchPosition = Vector2.zero;
                return Vector2.zero;
            }
            
            // Tính delta từ vị trí hiện tại và vị trí trước đó
            Vector2 delta = Vector2.zero;
            if (lastRightHalfTouchPosition != Vector2.zero)
            {
                delta = rightHalfTouchPosition - lastRightHalfTouchPosition;
            }
            else
            {
                // Lần đầu tiên, dùng delta từ touch
                delta = rightHalfTouch.delta.ReadValue();
            }
            
            lastRightHalfTouchPosition = rightHalfTouchPosition;
            return delta;
        }
        
        // Không có touch ở nửa phải
        rightHalfTouchId = -1;
        lastRightHalfTouchPosition = Vector2.zero;
        return Vector2.zero;
    }
    
    /// <summary>
    /// Kiểm tra xem control có phải là pointer (mouse/touch) không
    /// </summary>
    private bool IsPointerControl(InputControl control)
    {
        if (control == null) return false;
        
        // Kiểm tra device của control
        InputDevice device = control.device;
        
        // Kiểm tra mouse
        if (device is Mouse)
        {
            return true;
        }
        
        // Kiểm tra touchscreen
        if (device is Touchscreen)
        {
            return true;
        }
        
        // Kiểm tra tên control path
        string controlPath = control.path;
        if (controlPath != null)
        {
            // Kiểm tra các pattern phổ biến của pointer
            if (controlPath.Contains("<Pointer>") || 
                controlPath.Contains("/delta") ||
                controlPath.Contains("Mouse") ||
                controlPath.Contains("Touch"))
            {
                return true;
            }
        }
        
        return false;
    }
    
}