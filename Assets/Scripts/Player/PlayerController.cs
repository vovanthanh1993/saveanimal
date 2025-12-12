using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject model;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;

    [Header("Animal Collection")]
    [Tooltip("Điểm để hiển thị animal khi đã nhặt")]
    [SerializeField] private Transform animalPoint;
    
    // Animal đang được mang theo (chỉ 1 con)
    private AnimalItem carriedAnimal = null;

    [Header("Spawn Settings")]
    [Tooltip("Vị trí spawn point (nếu null sẽ dùng vị trí ban đầu của player)")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    [SerializeField] private bool canReceiveInput = true;
    [SerializeField] private bool isDisable = false;
    
    // Components
    public PlayerAnimation playerAnimation;
    
    private Vector3 initialSpawnPosition;
    
    // Flag để tránh trừ mạng nhiều lần khi va chạm với nhiều ResetTag cùng lúc
    private bool isReturningToSpawn = false;
    private float lastSpawnReturnTime = 0f;
    [Header("Spawn Return Settings")]
    [Tooltip("Thời gian cooldown sau khi về spawn point (giây)")]
    [SerializeField] private float spawnReturnCooldown = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Get components
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
        playerAnimation = GetComponent<PlayerAnimation>();
        
        // Tự động tìm AnimalPoint nếu chưa được assign
        if (animalPoint == null)
        {
            animalPoint = transform.Find("AnimalPoint");
            if (animalPoint == null)
            {
                // Tìm trong tất cả các con
                foreach (Transform child in transform)
                {
                    if (child.name == "AnimalPoint")
                    {
                        animalPoint = child;
                        break;
                    }
                }
            }
        }
        
        // Đảm bảo CharacterController tồn tại
        if (characterController == null)
        {
            Debug.LogError("PlayerController: CharacterController component is missing! Please add CharacterController to the player GameObject.");
        }
    }

    private void Start()
    {
        // Load move speed from PlayerDataManager if available
        if (PlayerDataManager.Instance != null)
        {
            moveSpeed = PlayerDataManager.Instance.playerData.speed / 10f;
        }
        
        // Lưu vị trí spawn point
        if (spawnPoint != null)
        {
            initialSpawnPosition = spawnPoint.position;
        }
        else
        {
            initialSpawnPosition = transform.position;
        }
        
        // Reset flag khi bắt đầu level mới
        isReturningToSpawn = false;
        lastSpawnReturnTime = 0f;
        
        // Camera sẽ được quản lý bởi CameraFollowController tự động
    }

    private void Update()
    {
        if (isDisable || !canReceiveInput)
        {
            // Nếu không cho phép input, dừng animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        HandleInput();
    }

    private void LateUpdate()
    {
        if (!isDisable)
        {
            UpdateCameraTarget();
        }
    }

    #region Initialization
    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (InputManager.Instance == null) return;

        HandleMovement();
        HandleCheatInput();
    }

    private void HandleMovement()
    {
        if (characterController == null || InputManager.Instance == null)
        {
            return;
        }

        Vector2 moveInput = InputManager.Instance.InputMoveVector();
        
        if (moveInput.magnitude < 0.1f)
        {
            // Không có input - chỉ áp dụng gravity
            characterController.Move(Physics.gravity * Time.deltaTime);
            
            // Cập nhật animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        // Tính toán hướng di chuyển tương đối với camera rotation
        Vector3 worldDirection = GetWorldDirection(moveInput);
        
        // Xoay player theo hướng di chuyển
        if (worldDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Di chuyển player
        Vector3 velocity = worldDirection * moveSpeed + Physics.gravity;
        characterController.Move(velocity * Time.deltaTime);

        // Cập nhật animation walk
        if (playerAnimation != null)
        {
            float moveSpeedValue = moveInput.magnitude;
            playerAnimation.SetMovement(true, moveSpeedValue);
        }
    }

    /// <summary>
    /// Chuyển đổi input direction sang world direction dựa trên camera rotation
    /// </summary>
    private Vector3 GetWorldDirection(Vector2 inputDirection)
    {
        // Lấy rotation từ camera hoặc từ player rotation
        Quaternion rotation = Quaternion.identity;
        
        if (Camera.main != null)
        {
            // Dùng camera yaw để tính hướng di chuyển
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }
        else
        {
            // Nếu không có camera, dùng player rotation
            rotation = transform.rotation;
        }
        
        // Chuyển đổi input direction sang world direction
        Vector3 direction = new Vector3(inputDirection.x, 0f, inputDirection.y);
        return rotation * direction;
    }

    /// <summary>
    /// Xử lý input cheat/debug (F1 để thêm 1000 gold)
    /// </summary>
    private void HandleCheatInput()
    {
        // Nhấn F1 để thêm 1000 gold (sử dụng Input System)
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            AddGold(1000);
        }
    }

    /// <summary>
    /// Thêm gold vào PlayerData
    /// </summary>
    /// <param name="amount">Số lượng gold cần thêm</param>
    private void AddGold(int amount)
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            PlayerDataManager.Instance.playerData.totalReward += amount;
            PlayerDataManager.Instance.Save();
            
            Debug.Log($"Đã thêm {amount} gold. Tổng gold hiện tại: {PlayerDataManager.Instance.playerData.totalReward}");
            
            // Cập nhật UI nếu đang ở HomePanel
            if (UIManager.Instance != null && UIManager.Instance.homePanel != null)
            {
                UIManager.Instance.homePanel.UpdateRewardDisplay();
            }
        }
        else
        {
            Debug.LogWarning("PlayerController: Không thể thêm gold vì PlayerDataManager không tồn tại!");
        }
    }

    #endregion

    #region Visual & Camera

    private void UpdateCameraTarget()
    {
        // Không cần xoay camTarget nữa vì camera top-down chỉ follow position
        // Giữ hàm này để không phá vỡ code khác nhưng không làm gì
    }

    #endregion

    #region Collision Detection

    /// <summary>
    /// Xử lý va chạm với trigger (vật thể có tag "end")
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResetTag"))
        {
            // Kiểm tra cooldown để tránh trừ mạng nhiều lần khi va chạm với nhiều ResetTag cùng lúc
            if (!isReturningToSpawn && Time.time - lastSpawnReturnTime >= spawnReturnCooldown)
            {
                ReturnToSpawnPoint();
            }
        }

        if (other.CompareTag("EndTag"))
        {
            ShowVictory();
        }
        
        // Xử lý checkpoint
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null)
        {
            checkpoint.OnPlayerEnter(this);
        }
    }
    
    /// <summary>
    /// Xử lý va chạm với collider khi dùng Character Controller
    /// Character Controller không trigger OnCollisionEnter, cần dùng OnControllerColliderHit
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Kiểm tra va chạm với animal
        AnimalItem animalItem = hit.gameObject.GetComponent<AnimalItem>();
        if (animalItem != null && carriedAnimal == null && !animalItem.IsPickedUp && !animalItem.IsCollected)
        {
            // Chỉ lượm được nếu chưa có animal nào đang mang
            // Lấy AnimalPoint
            Transform animalPointTransform = GetAnimalPoint();
            
            // Lượm animal (chưa tính điểm)
            animalItem.PickupAnimal(animalPointTransform);
            
            // Thông báo cho PlayerController
            PickupAnimal(animalItem);
        }
    }
    
    /// <summary>
    /// Lượm animal (chỉ lượm được 1 con) - được gọi từ AnimalItem
    /// </summary>
    public void PickupAnimal(AnimalItem animalItem)
    {
        if (carriedAnimal != null)
        {
            Debug.Log("Đã có animal đang mang theo, không thể lượm thêm!");
            return;
        }
        
        carriedAnimal = animalItem;
    }
    
    /// <summary>
    /// Thả animal tại checkpoint
    /// </summary>
    public void DropAnimalAtCheckpoint(Transform checkpointPosition)
    {
        if (carriedAnimal == null)
        {
            Debug.Log("Không có animal để thả!");
            return;
        }
        
        // Lưu animal type trước khi thả
        AnimalType animalType = carriedAnimal.AnimalType;
        
        // Thả animal tại checkpoint với callback để tính điểm sau khi animation hoàn thành
        carriedAnimal.DropAnimalAtCheckpoint(checkpointPosition, () =>
        {
            // Tính điểm khi animation thả animal hoàn thành
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnAnimalCollected(animalType);
            }
        });
        
        // Reset carried animal
        carriedAnimal = null;
    }
    
    /// <summary>
    /// Kiểm tra xem có đang mang animal không
    /// </summary>
    public bool HasCarriedAnimal()
    {
        return carriedAnimal != null;
    }
    
    /// <summary>
    /// Lấy animal đang mang theo
    /// </summary>
    public AnimalItem GetCarriedAnimal()
    {
        return carriedAnimal;
    } 
    
    /// <summary>
    /// Quay về spawn point
    /// </summary>
    private void ReturnToSpawnPoint()
    {
        // Đánh dấu đang trong quá trình về spawn để tránh xử lý nhiều lần
        if (isReturningToSpawn)
        {
            return;
        }
        
        isReturningToSpawn = true;
        lastSpawnReturnTime = Time.time;
        
        AudioManager.Instance.PlayFallSound();
        
        // Trừ 1 mạng khi về spawn point
        if (HealthPanel.Instance != null)
        {
            bool stillHasLives = HealthPanel.Instance.LoseLife();
            
            // Nếu hết mạng, không cần teleport nữa vì đã hiển thị lose panel
            if (!stillHasLives)
            {
                Debug.Log("Player đã hết mạng! Không thể tiếp tục.");
                isReturningToSpawn = false; // Reset flag
                return;
            }
        }
        
        // Tắt CharacterController tạm thời để teleport
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Teleport về spawn point
        Vector3 targetPosition = spawnPoint != null ? spawnPoint.position : initialSpawnPosition;
        transform.position = targetPosition;
        
        // Bật lại CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        Debug.Log("Player đã quay về spawn point!");
        
        // Reset flag sau một khoảng thời gian ngắn để cho phép xử lý lần tiếp theo
        StartCoroutine(ResetSpawnReturnFlag());
    }
    
    /// <summary>
    /// Reset flag sau khi hoàn thành quá trình về spawn point
    /// </summary>
    private System.Collections.IEnumerator ResetSpawnReturnFlag()
    {
        yield return new WaitForSeconds(spawnReturnCooldown);
        isReturningToSpawn = false;
    }
    
    /// <summary>
    /// Hiển thị victory panel khi đến end gate
    /// </summary>
    private void ShowVictory()
    {
        // Kiểm tra xem đã collect đủ animal và đến endgate chưa
        if (QuestManager.Instance != null)
        {
            // Kiểm tra và hoàn thành quest nếu đã collect đủ
            QuestManager.Instance.CheckAndCompleteQuest();
        }
        else
        {
            Debug.LogWarning("QuestManager không tồn tại!");
        }
    }
    
    /// <summary>
    /// Set spawn point mới
    /// </summary>
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        if (spawnPoint != null)
        {
            initialSpawnPosition = spawnPoint.position;
        }
    }

    #endregion

    #region Public Methods

    public void SetDisable(bool disable)
    {
        isDisable = disable;
        
        if (characterController != null)
        {
            characterController.enabled = !disable;
        }
        
        if (disable)
        {
            SetIdleAnimation();
        }
    }

    public void SetIdleAnimation()
    {
        // Set movement to idle (speed = 0)
        playerAnimation?.SetMovement(false, 0f);
    }

    public GameObject GetModel()
    {
        return model;
    }

    /// <summary>
    /// Lấy AnimalPoint transform
    /// </summary>
    public Transform GetAnimalPoint()
    {
        return animalPoint;
    }

    #endregion
}
