using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputSystem_Actions InputSystem;
    
    [Header("Touch Joystick Settings")]
    [Tooltip("Joystick UI động - xuất hiện tại vị trí touch")]
    public DynamicJoystickUI dynamicJoystickUI;
    
    [Tooltip("Bán kính tối đa joystick có thể di chuyển (pixel)")]
    public float joystickRange = 150f;
    
    [Tooltip("Dead zone - vùng không nhạy ở giữa joystick (0-1)")]
    [Range(0f, 0.5f)]
    public float deadZone = 0.1f;
    
    // Touch tracking cho joystick động
    private int activeTouchId = -1;
    private Vector2 touchStartPosition = Vector2.zero;
    private Vector2 currentTouchPosition = Vector2.zero;
    private bool isTouchActive = false;

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

    private void Update()
    {
        HandleTouchInput();
    }

    /// <summary>
    /// Xử lý touch input để tạo joystick động
    /// </summary>
    private void HandleTouchInput()
    {
        // Kiểm tra touch input
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            
            // Tìm touch đang được sử dụng
            TouchControl activeTouch = null;
            
            if (activeTouchId >= 0)
            {
                // Tìm touch đã được track
                for (int i = 0; i < touches.Count; i++)
                {
                    var touch = touches[i];
                    if (touch.touchId.ReadValue() == activeTouchId)
                    {
                        var phase = touch.phase.ReadValue();
                        
                        if (phase == UnityEngine.InputSystem.TouchPhase.Ended || 
                            phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                        {
                            // Touch đã kết thúc - reset
                            ResetTouchJoystick();
                        }
                        else
                        {
                            // Touch vẫn đang hoạt động
                            activeTouch = touch;
                            currentTouchPosition = touch.position.ReadValue();
                        }
                        break;
                    }
                }
            }
            
            // Nếu không có touch đang track, tìm touch mới
            if (activeTouch == null && activeTouchId < 0)
            {
                for (int i = 0; i < touches.Count; i++)
                {
                    var touch = touches[i];
                    var phase = touch.phase.ReadValue();
                    
                    // Tìm touch mới bắt đầu
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        Vector2 touchPos = touch.position.ReadValue();
                        
                        // Kiểm tra xem touch có phải là UI touch không
                        if (IsPointerOverUI(touchPos))
                        {
                            // Touch vào UI, không xử lý như joystick
                            continue;
                        }
                        
                        activeTouch = touch;
                        activeTouchId = touch.touchId.ReadValue();
                        touchStartPosition = touchPos;
                        currentTouchPosition = touchStartPosition;
                        isTouchActive = true;
                        
                        // Hiển thị joystick UI tại vị trí touch
                        if (dynamicJoystickUI != null)
                        {
                            dynamicJoystickUI.SetPosition(touchStartPosition);
                            // Cập nhật joystickRange từ UI nếu có
                            if (dynamicJoystickUI.joystickRange > 0)
                            {
                                joystickRange = dynamicJoystickUI.joystickRange;
                            }
                        }
                        break;
                    }
                }
            }
            
            // Cập nhật vị trí nếu có touch đang hoạt động
            if (activeTouch != null)
            {
                currentTouchPosition = activeTouch.position.ReadValue();
                
                // Cập nhật joystick UI
                if (dynamicJoystickUI != null)
                {
                    dynamicJoystickUI.UpdateHandlePosition(currentTouchPosition, touchStartPosition);
                }
            }
        }
        else
        {
            // Kiểm tra mouse input (để test trên editor)
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    
                    // Kiểm tra xem mouse click có phải là UI click không
                    if (IsPointerOverUI(mousePos))
                    {
                        // Click vào UI, không xử lý như joystick
                        // Reset nếu đang có joystick active
                        if (activeTouchId >= 0)
                        {
                            ResetTouchJoystick();
                        }
                    }
                    else
                    {
                        // Mouse click - bắt đầu joystick
                        touchStartPosition = mousePos;
                        currentTouchPosition = touchStartPosition;
                        activeTouchId = 0; // Dùng ID 0 cho mouse
                        isTouchActive = true;
                        
                        // Hiển thị joystick UI tại vị trí mouse
                        if (dynamicJoystickUI != null)
                        {
                            dynamicJoystickUI.SetPosition(touchStartPosition);
                            // Cập nhật joystickRange từ UI nếu có
                            if (dynamicJoystickUI.joystickRange > 0)
                            {
                                joystickRange = dynamicJoystickUI.joystickRange;
                            }
                        }
                    }
                }
                else if (Mouse.current.leftButton.isPressed && activeTouchId >= 0)
                {
                    // Mouse đang giữ - cập nhật vị trí
                    currentTouchPosition = Mouse.current.position.ReadValue();
                    
                    // Cập nhật joystick UI
                    if (dynamicJoystickUI != null)
                    {
                        dynamicJoystickUI.UpdateHandlePosition(currentTouchPosition, touchStartPosition);
                    }
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame && activeTouchId >= 0)
                {
                    // Mouse thả - reset
                    ResetTouchJoystick();
                }
            }
        }
    }
    
    /// <summary>
    /// Reset touch joystick
    /// </summary>
    private void ResetTouchJoystick()
    {
        activeTouchId = -1;
        touchStartPosition = Vector2.zero;
        currentTouchPosition = Vector2.zero;
        isTouchActive = false;
        
        // Reset joystick UI
        if (dynamicJoystickUI != null)
        {
            dynamicJoystickUI.ResetJoystick();
        }
    }

    public void DisablePlayerInput()
    {
        InputSystem.Player.Disable();
    }

    public Vector2 InputMoveVector()
    {
        // Ưu tiên input từ joystick UI nếu đang hoạt động
        if (dynamicJoystickUI != null && dynamicJoystickUI.IsActive())
        {
            Vector2 input = dynamicJoystickUI.GetInputVector();
            
            // Áp dụng dead zone
            float magnitude = input.magnitude;
            if (magnitude < deadZone)
            {
                return Vector2.zero;
            }
            
            // Scale lại sau khi loại bỏ dead zone
            float scaledMagnitude = (magnitude - deadZone) / (1f - deadZone);
            input = input.normalized * scaledMagnitude;
            
            return input;
        }
        
        // Nếu không có joystick UI, tính từ touch input trực tiếp
        if (isTouchActive && activeTouchId >= 0)
        {
            // Tính vector từ vị trí bắt đầu đến vị trí hiện tại
            Vector2 delta = currentTouchPosition - touchStartPosition;
            
            // Giới hạn trong phạm vi joystickRange
            float distance = delta.magnitude;
            if (distance > joystickRange)
            {
                delta = delta.normalized * joystickRange;
            }
            
            // Chuẩn hóa về [-1, 1]
            Vector2 normalizedInput = delta / joystickRange;
            
            // Áp dụng dead zone
            float magnitude = normalizedInput.magnitude;
            if (magnitude < deadZone)
            {
                return Vector2.zero;
            }
            
            // Scale lại sau khi loại bỏ dead zone
            float scaledMagnitude = (magnitude - deadZone) / (1f - deadZone);
            normalizedInput = normalizedInput.normalized * scaledMagnitude;
            
            return normalizedInput;
        }
        
        // Nếu không có touch joystick, dùng input từ Input System
        return InputSystem.Player.Move.ReadValue<Vector2>();
    }
    
    /// <summary>
    /// Kiểm tra xem pointer (touch/mouse) có đang ở trên UI element không
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        // Kiểm tra EventSystem
        if (EventSystem.current == null)
        {
            return false;
        }
        
        // Tạo PointerEventData
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = screenPosition;
        
        // Raycast vào UI
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        
        // Lọc bỏ các kết quả là joystick UI (nếu có)
        foreach (var result in results)
        {
            // Nếu không phải là joystick UI, thì là UI element khác
            if (dynamicJoystickUI != null)
            {
                // Kiểm tra xem có phải là joystick UI không
                if (result.gameObject != dynamicJoystickUI.gameObject &&
                    result.gameObject.transform != dynamicJoystickUI.background &&
                    result.gameObject.transform != dynamicJoystickUI.handle &&
                    (dynamicJoystickUI.background != null && !result.gameObject.transform.IsChildOf(dynamicJoystickUI.background)) &&
                    (dynamicJoystickUI.handle != null && !result.gameObject.transform.IsChildOf(dynamicJoystickUI.handle)))
                {
                    // Là UI element khác, không phải joystick
                    return true;
                }
            }
            else
            {
                // Không có joystick UI, bất kỳ UI nào cũng được tính
                return true;
            }
        }
        
        return false;
    }
    
}