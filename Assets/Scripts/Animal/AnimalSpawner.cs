using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawner để spawn animal theo số lượng trong quest objectives
/// </summary>
public class AnimalSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Tự động load prefabs từ Resources/Prefabs (nếu bật, sẽ bỏ qua animalPrefabs list)")]
    [SerializeField] private bool autoLoadFromResources = true;
    
    [Tooltip("Đường dẫn thư mục trong Resources chứa animal prefabs (ví dụ: 'Prefabs' hoặc 'Prefabs/Animals')")]
    [SerializeField] private string prefabsFolderPath = "Prefabs";
    
    [Tooltip("Dictionary để map AnimalType với prefab tương ứng (chỉ dùng khi autoLoadFromResources = false)")]
    [SerializeField] private List<AnimalPrefabData> animalPrefabs = new List<AnimalPrefabData>();
    
    [Header("Spawn Points")]
    [Tooltip("GameObject cha chứa tất cả các spawn points (sẽ tự động lấy tất cả các con)")]
    [SerializeField] private Transform spawnPointsParent;
    
    private List<Transform> spawnPoints = new List<Transform>();
    
    
    private Dictionary<AnimalType, GameObject> animalPrefabDict = new Dictionary<AnimalType, GameObject>();
    private List<GameObject> spawnedAnimals = new List<GameObject>();
    private List<int> usedSpawnPointIndices = new List<int>(); // Lưu các spawn point đã dùng
    
    void Start()
    {
        InitializeSpawnPoints();
        InitializePrefabDictionary();
        // Delay spawning để đảm bảo QuestManager đã load quest
        StartCoroutine(WaitForQuestManagerAndSpawn());
    }
    
    /// <summary>
    /// Đợi QuestManager khởi tạo và load quest trước khi spawn animals
    /// </summary>
    System.Collections.IEnumerator WaitForQuestManagerAndSpawn()
    {
        // Đợi một frame để đảm bảo tất cả Start() methods đã chạy
        yield return null;
        
        // Đợi cho đến khi QuestManager.Instance và currentQuest đã sẵn sàng
        int maxWaitFrames = 60; // Tối đa đợi 60 frames (khoảng 1 giây ở 60fps)
        int waitFrames = 0;
        
        while ((QuestManager.Instance == null || QuestManager.Instance.currentQuest == null) && waitFrames < maxWaitFrames)
        {
            yield return null;
            waitFrames++;
        }
        
        // Kiểm tra lại sau khi đợi
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null)
        {
            Debug.LogError("AnimalSpawner: Không thể tìm thấy QuestManager hoặc currentQuest sau khi đợi!");
            yield break;
        }
        
        // Spawn animals khi QuestManager đã sẵn sàng
        SpawnAnimalsFromQuest();
    }
    
    /// <summary>
    /// Khởi tạo danh sách spawn points từ các con của spawnPointsParent
    /// </summary>
    private void InitializeSpawnPoints()
    {
        spawnPoints.Clear();
        
        if (spawnPointsParent == null)
        {
            Debug.LogError("AnimalSpawner: spawnPointsParent không được set! Vui lòng gán GameObject cha chứa spawn points.");
            return;
        }
        
        // Lấy tất cả các con của spawnPointsParent
        for (int i = 0; i < spawnPointsParent.childCount; i++)
        {
            Transform child = spawnPointsParent.GetChild(i);
            if (child != null)
            {
                spawnPoints.Add(child);
            }
        }
        
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"AnimalSpawner: Không tìm thấy spawn point nào trong {spawnPointsParent.name}!");
        }
        else
        {
            Debug.Log($"AnimalSpawner: Đã tìm thấy {spawnPoints.Count} spawn points");
        }
    }
    
    /// <summary>
    /// Khởi tạo dictionary từ list animalPrefabs hoặc load từ Resources
    /// </summary>
    private void InitializePrefabDictionary()
    {
        animalPrefabDict.Clear();
        
        if (autoLoadFromResources)
        {
            LoadPrefabsFromResources();
        }
        else
        {
            LoadPrefabsFromList();
        }
        
        Debug.Log($"AnimalSpawner: Đã load {animalPrefabDict.Count} animal prefabs");
    }
    
    /// <summary>
    /// Load prefabs từ Resources folder
    /// </summary>
    private void LoadPrefabsFromResources()
    {
        if (string.IsNullOrEmpty(prefabsFolderPath))
        {
            Debug.LogWarning("AnimalSpawner: prefabsFolderPath không được set!");
            return;
        }
        
        // Load tất cả GameObject từ Resources/Prefabs
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(prefabsFolderPath);
        
        if (loadedPrefabs == null || loadedPrefabs.Length == 0)
        {
            Debug.LogWarning($"AnimalSpawner: Không tìm thấy prefab nào trong Resources/{prefabsFolderPath}!");
            return;
        }
        
        // Map mỗi prefab với AnimalType dựa trên tên hoặc AnimalItem component
        foreach (GameObject prefab in loadedPrefabs)
        {
            if (prefab == null) continue;
            
            AnimalType animalType = GetAnimalTypeFromPrefab(prefab);
            
            if (animalPrefabDict.ContainsKey(animalType))
            {
                Debug.LogWarning($"AnimalSpawner: Prefab cho {animalType} đã tồn tại, bỏ qua prefab: {prefab.name}");
                continue;
            }
            
            animalPrefabDict[animalType] = prefab;
            Debug.Log($"AnimalSpawner: Đã load prefab '{prefab.name}' cho {animalType}");
        }
    }
    
    /// <summary>
    /// Load prefabs từ list animalPrefabs (manual)
    /// </summary>
    private void LoadPrefabsFromList()
    {
        foreach (var data in animalPrefabs)
        {
            if (data.prefab != null)
            {
                animalPrefabDict[data.animalType] = data.prefab;
            }
        }
    }
    
    /// <summary>
    /// Xác định AnimalType từ prefab (dựa trên AnimalItem component hoặc tên file)
    /// </summary>
    private AnimalType GetAnimalTypeFromPrefab(GameObject prefab)
    {
        // Thử lấy từ AnimalItem component trước
        AnimalItem animalItem = prefab.GetComponent<AnimalItem>();
        if (animalItem != null)
        {
            return animalItem.AnimalType;
        }
        
        // Nếu không có component, thử parse từ tên file
        string prefabName = prefab.name;
        
        // Loại bỏ các ký tự đặc biệt và chuyển về PascalCase
        prefabName = prefabName.Replace("_", "").Replace("-", "").Trim();
        
        // Thử parse tên thành AnimalType enum
        if (System.Enum.TryParse<AnimalType>(prefabName, true, out AnimalType parsedType))
        {
            return parsedType;
        }
        
        // Nếu không parse được, thử tìm match không phân biệt hoa thường
        AnimalType[] allTypes = (AnimalType[])System.Enum.GetValues(typeof(AnimalType));
        foreach (AnimalType type in allTypes)
        {
            if (prefabName.Equals(type.ToString(), System.StringComparison.OrdinalIgnoreCase))
            {
                return type;
            }
        }
        
        // Fallback: nếu không tìm thấy, dùng Cow làm mặc định và cảnh báo
        Debug.LogWarning($"AnimalSpawner: Không thể xác định AnimalType cho prefab '{prefab.name}', sử dụng Cow làm mặc định");
        return AnimalType.Cow;
    }
    
    /// <summary>
    /// Spawn animals dựa trên quest objectives
    /// </summary>
    public void SpawnAnimalsFromQuest()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null)
        {
            Debug.LogError("AnimalSpawner: QuestManager hoặc currentQuest là null!");
            return;
        }
        
        QuestData quest = QuestManager.Instance.currentQuest;
        
        if (quest.objectives == null || quest.objectives.Length == 0)
        {
            Debug.LogWarning("AnimalSpawner: Quest không có objectives!");
            return;
        }
        
        // Reset danh sách spawn points đã dùng
        usedSpawnPointIndices.Clear();
        
        // Spawn animals cho mỗi objective
        foreach (var objective in quest.objectives)
        {
            SpawnAnimalsForObjective(objective);
        }
        
        Debug.Log($"AnimalSpawner: Đã spawn {spawnedAnimals.Count} animals (chỉ spawn các loại có trong quest)");
    }
    
    /// <summary>
    /// Spawn animals cho một objective cụ thể
    /// </summary>
    private void SpawnAnimalsForObjective(QuestObjective objective)
    {
        if (!animalPrefabDict.ContainsKey(objective.animalType))
        {
            Debug.LogWarning($"AnimalSpawner: Không tìm thấy prefab cho {objective.animalType}!");
            return;
        }
        
        GameObject prefab = animalPrefabDict[objective.animalType];
        
        // Spawn số lượng = requiredAmount + 1 tại các spawn point chưa dùng
        int spawnAmount = objective.requiredAmount + 1;
        for (int i = 0; i < spawnAmount; i++)
        {
            int spawnIndex = GetUnusedSpawnPointIndex();
            if (spawnIndex == -1)
            {
                Debug.LogWarning($"AnimalSpawner: Không còn spawn point trống để spawn {objective.animalType}!");
                break;
            }
            
            Vector3 spawnPosition = spawnPoints[spawnIndex].position;
            // Set rotation y = 180 khi spawn
            Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);
            GameObject animal = Instantiate(prefab, spawnPosition, spawnRotation);
            
            // Đảm bảo animal có AnimalItem component với đúng animalType
            AnimalItem animalItem = animal.GetComponent<AnimalItem>();
            if (animalItem == null)
            {
                // Nếu prefab chưa có AnimalItem, thêm vào
                animalItem = animal.AddComponent<AnimalItem>();
            }
            // Set animalType cho animal
            animalItem.SetAnimalType(objective.animalType);
            
            spawnedAnimals.Add(animal);
            usedSpawnPointIndices.Add(spawnIndex);
        }
        
        Debug.Log($"AnimalSpawner: Đã spawn {spawnAmount} {objective.animalType} (yêu cầu: {objective.requiredAmount}, spawn thêm 1)");
    }
    
    /// <summary>
    /// Lấy index của một spawn point chưa được sử dụng
    /// </summary>
    private int GetUnusedSpawnPointIndex()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return -1;
        }
        
        // Tạo danh sách các index chưa dùng
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!usedSpawnPointIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }
        
        if (availableIndices.Count == 0)
        {
            return -1; // Không còn spawn point trống
        }
        
        // Chọn ngẫu nhiên một index từ danh sách chưa dùng
        int randomIndex = Random.Range(0, availableIndices.Count);
        return availableIndices[randomIndex];
    }
    
    
    /// <summary>
    /// Xóa tất cả animals đã spawn
    /// </summary>
    public void ClearSpawnedAnimals()
    {
        foreach (var animal in spawnedAnimals)
        {
            if (animal != null)
            {
                Destroy(animal);
            }
        }
        spawnedAnimals.Clear();
        usedSpawnPointIndices.Clear();
    }
    
    /// <summary>
    /// Respawn animals từ quest (xóa cũ và spawn lại)
    /// </summary>
    public void RespawnAnimals()
    {
        ClearSpawnedAnimals();
        SpawnAnimalsFromQuest();
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ spawn points
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 1f);
                }
            }
        }
    }
    
}

/// <summary>
/// Class để map AnimalType với prefab
/// </summary>
[System.Serializable]
public class AnimalPrefabData
{
    public AnimalType animalType;
    public GameObject prefab;
}

