using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float rotationSmoothSpeed = 6f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private bool useDamping = true;
    [SerializeField] private float dampingFactor = 0.95f;
    
    [Header("Obstacle Avoidance")]
    [SerializeField] private bool enableObstacleAvoidance = true;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    [SerializeField] private float obstacleCheckRadius = 0.5f;
    [SerializeField] private float minDistanceFromObstacle = 1f;
    
    [Header("Camera Angles")]
    [SerializeField] private Vector3 angle1Position = new Vector3(0, 1.5f, -3f);
    [SerializeField] private Vector3 angle1Rotation = new Vector3(15, 0, 0);
    
    [SerializeField] private Vector3 angle2Position = new Vector3(0, 3f, -5f);
    [SerializeField] private Vector3 angle2Rotation = new Vector3(25, 0, 0);
    
    [SerializeField] private Vector3 angle3Position = new Vector3(0, 5f, -8f);
    [SerializeField] private Vector3 angle3Rotation = new Vector3(35, 0, 0);
    
    [Header("Current Settings")]
    [SerializeField] private int currentAngle = 1;
    
    private Transform _target;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _currentVelocity;
    private Vector3 _currentAngularVelocity;
    private Vector3 _lastTargetPosition;
    private Quaternion _lastTargetRotation;
    
    public enum CameraAngle
    {
        Angle1 = 1,
        Angle2 = 2,
        Angle3 = 3
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(Transform followTarget)
    {
        _target = followTarget;
        UpdateCameraPosition();
    }
    
    public void SwitchToAngle(int angle)
    {
        currentAngle = Mathf.Clamp(angle, 1, 3);
        UpdateCameraPosition();
    }
    
    public void SwitchToAngle(CameraAngle angle)
    {
        currentAngle = (int)angle;
        UpdateCameraPosition();
    }
    
    public void NextAngle()
    {
        currentAngle = currentAngle % 3 + 1;
        UpdateCameraPosition();
    }
    
    public void PreviousAngle()
    {
        currentAngle = currentAngle == 1 ? 3 : currentAngle - 1;
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        if (_target == null) return;
        
        Vector3 basePosition = _target.position;
        
        // Validate base position
        if (float.IsNaN(basePosition.x) || float.IsNaN(basePosition.y) || float.IsNaN(basePosition.z))
        {
            Debug.LogWarning("CameraController: Target position is NaN");
            return;
        }
        
        Vector3 offsetPosition;
        Vector3 offsetRotation;
        
        switch (currentAngle)
        {
            case 1:
                offsetPosition = angle1Position;
                offsetRotation = angle1Rotation;
                break;
            case 2:
                offsetPosition = angle2Position;
                offsetRotation = angle2Rotation;
                break;
            case 3:
                offsetPosition = angle3Position;
                offsetRotation = angle3Rotation;
                break;
            default:
                offsetPosition = angle1Position;
                offsetRotation = angle1Rotation;
                break;
        }
        
        // Validate offset values
        if (float.IsNaN(offsetPosition.x) || float.IsNaN(offsetPosition.y) || float.IsNaN(offsetPosition.z))
        {
            Debug.LogWarning($"CameraController: offsetPosition for angle {currentAngle} is NaN, using default");
            offsetPosition = new Vector3(0, 1.5f, -3f);
        }
        
        // Calculate target position relative to player (không xoay theo target rotation - top-down camera)
        // Sử dụng offset trực tiếp mà không nhân với target rotation
        Vector3 rotatedOffset = offsetPosition;
        
        // Validate rotated offset
        if (float.IsNaN(rotatedOffset.x) || float.IsNaN(rotatedOffset.y) || float.IsNaN(rotatedOffset.z))
        {
            Debug.LogWarning("CameraController: Rotated offset is NaN, using fallback");
            rotatedOffset = Vector3.back * 3f + Vector3.up * 1.5f;
        }
        
        Vector3 desiredPosition = basePosition + rotatedOffset;
        
        // Validate desired position
        if (float.IsNaN(desiredPosition.x) || float.IsNaN(desiredPosition.y) || float.IsNaN(desiredPosition.z))
        {
            Debug.LogWarning("CameraController: desiredPosition is NaN, using fallback");
            desiredPosition = basePosition + Vector3.back * 3f + Vector3.up * 1.5f;
        }
        
        // Kiểm tra và tránh vật cản
        if (enableObstacleAvoidance)
        {
            _targetPosition = CheckForObstacles(basePosition, desiredPosition);
        }
        else
        {
            _targetPosition = desiredPosition;
        }
        
        // Validate final target position
        if (float.IsNaN(_targetPosition.x) || float.IsNaN(_targetPosition.y) || float.IsNaN(_targetPosition.z))
        {
            Debug.LogWarning("CameraController: _targetPosition is NaN after CheckForObstacles, using fallback");
            _targetPosition = basePosition + Vector3.back * 3f + Vector3.up * 1.5f;
        }
        
        // Camera rotation cố định cho top-down view, không xoay theo target
        _targetRotation = Quaternion.Euler(offsetRotation);
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        
        // Validate target position trước tiên
        Vector3 targetPos = _target.position;
        if (float.IsNaN(targetPos.x) || float.IsNaN(targetPos.y) || float.IsNaN(targetPos.z))
        {
            Debug.LogWarning("CameraController: Target position is NaN, skipping update");
            return;
        }
        
        // Validate current camera position
        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
        {
            Debug.LogWarning("CameraController: Current camera position is NaN, resetting");
            transform.position = targetPos + Vector3.back * 3f + Vector3.up * 1.5f;
            _currentVelocity = Vector3.zero;
        }
        
        // Store last target position for velocity calculation
        _lastTargetPosition = _targetPosition;
        _lastTargetRotation = _targetRotation;
        
        // Update camera position if target moved
        UpdateCameraPosition();
        
        // Validate _targetPosition trước khi sử dụng
        if (float.IsNaN(_targetPosition.x) || float.IsNaN(_targetPosition.y) || float.IsNaN(_targetPosition.z))
        {
            Debug.LogWarning("CameraController: _targetPosition is NaN, resetting");
            _targetPosition = targetPos + Vector3.back * 3f + Vector3.up * 1.5f;
            _currentVelocity = Vector3.zero;
        }
        
        // Validate _lastTargetPosition
        if (float.IsNaN(_lastTargetPosition.x) || float.IsNaN(_lastTargetPosition.y) || float.IsNaN(_lastTargetPosition.z))
        {
            _lastTargetPosition = _targetPosition;
        }
        
        // Calculate target velocity (tránh chia cho 0)
        Vector3 targetVelocity = Vector3.zero;
        if (Time.deltaTime > 0.0001f)
        {
            targetVelocity = (_targetPosition - _lastTargetPosition) / Time.deltaTime;
        }
        
        // Validate _currentVelocity
        if (float.IsNaN(_currentVelocity.x) || float.IsNaN(_currentVelocity.y) || float.IsNaN(_currentVelocity.z))
        {
            Debug.LogWarning("CameraController: _currentVelocity is NaN, resetting");
            _currentVelocity = Vector3.zero;
        }
        
        // Smooth movement with acceleration/deceleration
        float distance = Vector3.Distance(transform.position, _targetPosition);
        if (distance > 0.01f && !float.IsNaN(distance) && !float.IsInfinity(distance))
        {
            // Accelerate towards target
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            
            // Validate _currentVelocity sau Lerp
            if (float.IsNaN(_currentVelocity.x) || float.IsNaN(_currentVelocity.y) || float.IsNaN(_currentVelocity.z))
            {
                _currentVelocity = Vector3.zero;
            }
            
            // Validate smoothSpeed và position trước khi set
            if (smoothSpeed > 0.0001f && !float.IsNaN(smoothSpeed) && !float.IsInfinity(smoothSpeed))
            {
                Vector3 currentPos = transform.position;
                Vector3 refVelocity = _currentVelocity;
                Vector3 newPosition = Vector3.SmoothDamp(currentPos, _targetPosition, ref refVelocity, 1f / smoothSpeed);
                _currentVelocity = refVelocity;
                
                // Kiểm tra NaN trước khi assign
                if (!float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z) &&
                    !float.IsInfinity(newPosition.x) && !float.IsInfinity(newPosition.y) && !float.IsInfinity(newPosition.z))
                {
                    transform.position = newPosition;
                }
                else
                {
                    Debug.LogWarning("CameraController: SmoothDamp returned NaN/Infinity, using fallback");
                    transform.position = targetPos + Vector3.back * 3f + Vector3.up * 1.5f;
                    _currentVelocity = Vector3.zero;
                }
            }
            else
            {
                // Fallback nếu smoothSpeed không hợp lệ
                Vector3 lerpedPos = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 10f);
                if (!float.IsNaN(lerpedPos.x) && !float.IsNaN(lerpedPos.y) && !float.IsNaN(lerpedPos.z))
                {
                    transform.position = lerpedPos;
                }
                else
                {
                    transform.position = targetPos + Vector3.back * 3f + Vector3.up * 1.5f;
                }
            }
        }
        else
        {
            // Decelerate when close to target
            _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
        
        // Smooth rotation
        if (Quaternion.Angle(transform.rotation, _targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
        
        // Apply damping to reduce jitter
        if (useDamping)
        {
            _currentVelocity *= dampingFactor;
        }
    }
    
    public int GetCurrentAngle()
    {
        return currentAngle;
    }
    
    public CameraAngle GetCurrentCameraAngle()
    {
        return (CameraAngle)currentAngle;
    }
    
    public void ResetCameraSmoothness()
    {
        _currentVelocity = Vector3.zero;
        _currentAngularVelocity = Vector3.zero;
    }
    
    public void SetSmoothness(float positionSmooth, float rotationSmooth)
    {
        smoothSpeed = positionSmooth;
        rotationSmoothSpeed = rotationSmooth;
    }
    
    private Vector3 CheckForObstacles(Vector3 playerPosition, Vector3 desiredCameraPosition)
    {
        // Validate inputs
        if (float.IsNaN(playerPosition.x) || float.IsNaN(desiredCameraPosition.x))
        {
            return desiredCameraPosition;
        }
        
        Vector3 direction = desiredCameraPosition - playerPosition;
        float distance = direction.magnitude;
        
        // Nếu distance quá nhỏ hoặc không hợp lệ, trả về desired position
        if (distance < 0.001f || float.IsNaN(distance) || float.IsInfinity(distance))
        {
            return desiredCameraPosition;
        }
        
        // Raycast từ player đến vị trí camera mong muốn
        RaycastHit hit;
        if (Physics.SphereCast(playerPosition, obstacleCheckRadius, direction.normalized, out hit, distance, obstacleLayerMask))
        {
            // Nếu có vật cản, di chuyển camera gần player hơn
            float adjustedDistance = hit.distance - minDistanceFromObstacle;
            
            // Validate adjustedDistance
            if (adjustedDistance < 0.001f || float.IsNaN(adjustedDistance) || float.IsInfinity(adjustedDistance))
            {
                adjustedDistance = 1f; // Fallback distance
            }
            
            Vector3 adjustedPosition = playerPosition + direction.normalized * adjustedDistance;
            
            // Validate adjusted position
            if (float.IsNaN(adjustedPosition.x) || float.IsNaN(adjustedPosition.y) || float.IsNaN(adjustedPosition.z))
            {
                return desiredCameraPosition;
            }
            
            // Đảm bảo camera không quá gần player
            float minDistance = 1f;
            if (Vector3.Distance(adjustedPosition, playerPosition) < minDistance)
            {
                adjustedPosition = playerPosition + direction.normalized * minDistance;
            }
            
            return adjustedPosition;
        }
        
        return desiredCameraPosition;
    }
} 