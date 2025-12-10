using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script cho checkpoint - nơi player thả animal để tính điểm
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Parent chứa danh sách các điểm drop animal")]
    [SerializeField] private Transform parentDropPoint;
    
    [Tooltip("Effect khi thả animal thành công")]
    [SerializeField] private GameObject dropEffect;
    
    // Danh sách các điểm drop có sẵn
    private List<Transform> dropPositions = new List<Transform>();
    
    // Danh sách các điểm drop đã được sử dụng
    private List<Transform> usedDropPositions = new List<Transform>();
    
    private void Start()
    {
        // Tự động tìm parentDropPoint nếu chưa được assign
        if (parentDropPoint == null)
        {
            parentDropPoint = transform.Find("ParentDropPoint");
            if (parentDropPoint == null)
            {
                // Nếu không tìm thấy, tìm với tên khác
                parentDropPoint = transform.Find("DropArea");
                if (parentDropPoint == null)
                {
                    Debug.LogWarning($"Checkpoint: Không tìm thấy ParentDropPoint trong {gameObject.name}. Vui lòng assign hoặc tạo GameObject cha chứa các điểm drop.");
                }
            }
        }
        
        // Lấy tất cả các child của parentDropPoint làm danh sách drop positions
        if (parentDropPoint != null)
        {
            LoadDropPositions();
        }
    }
    
    /// <summary>
    /// Load tất cả các điểm drop từ parentDropPoint
    /// </summary>
    private void LoadDropPositions()
    {
        dropPositions.Clear();
        
        foreach (Transform child in parentDropPoint)
        {
            if (child.gameObject.activeInHierarchy)
            {
                dropPositions.Add(child);
            }
        }
        
        if (dropPositions.Count == 0)
        {
            Debug.LogWarning($"Checkpoint: Không tìm thấy điểm drop nào trong {parentDropPoint.name}!");
        }
        else
        {
            Debug.Log($"Checkpoint: Đã load {dropPositions.Count} điểm drop.");
        }
    }
    
    /// <summary>
    /// Được gọi khi player vào checkpoint
    /// </summary>
    public void OnPlayerEnter(PlayerController player)
    {
        if (player == null) return;
        
        // Kiểm tra xem player có đang mang animal không
        if (player.HasCarriedAnimal())
        {
            // Lấy ngẫu nhiên một điểm drop từ danh sách
            Transform dropPosition = GetRandomDropPosition();
            
            if (dropPosition == null)
            {
                Debug.LogWarning("Checkpoint: Không có điểm drop nào khả dụng!");
                return;
            }
            
            // Thả animal tại checkpoint
            player.DropAnimalAtCheckpoint(dropPosition);
            
            // Play checkpoint sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCheckpointSound();
            }
            
            // Spawn effect tại vị trí drop
            if (dropEffect != null)
            {
                Instantiate(dropEffect, dropPosition.position, Quaternion.identity);
            }
            
            Debug.Log("Đã thả animal tại checkpoint!");
        }
        else
        {
            Debug.Log("Bạn cần mang animal để thả tại checkpoint!");
        }
    }
    
    /// <summary>
    /// Lấy ngẫu nhiên một điểm drop từ danh sách chưa được sử dụng
    /// </summary>
    private Transform GetRandomDropPosition()
    {
        if (dropPositions == null || dropPositions.Count == 0)
        {
            // Nếu danh sách rỗng, thử load lại
            LoadDropPositions();
            
            if (dropPositions.Count == 0)
            {
                return null;
            }
        }
        
        // Lấy danh sách các điểm chưa được sử dụng
        List<Transform> availablePositions = new List<Transform>();
        foreach (Transform pos in dropPositions)
        {
            if (!usedDropPositions.Contains(pos))
            {
                availablePositions.Add(pos);
            }
        }
        
        // Nếu không còn điểm nào, reset danh sách đã sử dụng
        if (availablePositions.Count == 0)
        {
            Debug.Log("Checkpoint: Đã dùng hết các điểm drop, reset lại danh sách.");
            usedDropPositions.Clear();
            availablePositions = new List<Transform>(dropPositions);
        }
        
        // Chọn ngẫu nhiên một điểm từ danh sách chưa được sử dụng
        int randomIndex = Random.Range(0, availablePositions.Count);
        Transform selectedPosition = availablePositions[randomIndex];
        
        // Đánh dấu điểm này đã được sử dụng
        usedDropPositions.Add(selectedPosition);
        
        return selectedPosition;
    }
    
    /// <summary>
    /// Lấy parent drop point
    /// </summary>
    public Transform GetParentDropPoint()
    {
        return parentDropPoint;
    }
    
    /// <summary>
    /// Reload danh sách drop positions (dùng khi thêm/xóa điểm drop trong runtime)
    /// </summary>
    public void ReloadDropPositions()
    {
        LoadDropPositions();
        usedDropPositions.Clear(); // Reset danh sách đã sử dụng khi reload
    }
    
    /// <summary>
    /// Reset checkpoint (xóa tất cả animal đã thả và reset danh sách drop positions)
    /// </summary>
    public void ResetCheckpoint()
    {
        usedDropPositions.Clear();
        ReloadDropPositions();
    }
}

