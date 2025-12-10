using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager để quản lý QuestData, tự động tạo 50 quest mặc định nếu chưa có
/// </summary>
public class QuestDataManager : MonoBehaviour
{
    public static QuestDataManager Instance { get; private set; }

    private Dictionary<int, QuestData> questsCache = new Dictionary<int, QuestData>();
    private const int DefaultQuestCount = 50;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadOrCreateQuests();
    }

    /// <summary>
    /// Load quest từ JSON hoặc tạo 50 quest mặc định nếu chưa có
    /// </summary>
    public void LoadOrCreateQuests()
    {
        questsCache = QuestDataStorage.LoadAllQuests();
        
        // Nếu chưa có quest nào, tạo 50 quest mặc định
        if (questsCache == null || questsCache.Count == 0)
        {
            Debug.Log($"QuestDataManager: Không tìm thấy quest nào, đang tạo {DefaultQuestCount} quest mặc định...");
            CreateDefaultQuests();
            questsCache = QuestDataStorage.LoadAllQuests();
        }
        
        Debug.Log($"QuestDataManager: Đã load {questsCache.Count} quest");
    }

    /// <summary>
    /// Tạo 50 quest mặc định
    /// </summary>
    private void CreateDefaultQuests()
    {
        Dictionary<int, QuestData> defaultQuests = new Dictionary<int, QuestData>();
        
        for (int i = 1; i <= DefaultQuestCount; i++)
        {
            QuestData questData = CreateDefaultQuest(i);
            defaultQuests[i] = questData;
        }
        
        // Lưu vào JSON
        QuestDataStorage.SaveAllQuests(defaultQuests);
        Debug.Log($"QuestDataManager: Đã tạo {DefaultQuestCount} quest mặc định");
    }

    /// <summary>
    /// Tạo một quest mặc định với ID cụ thể
    /// </summary>
    private QuestData CreateDefaultQuest(int questId)
    {
        QuestData questData = ScriptableObject.CreateInstance<QuestData>();
        questData.questId = questId;
        
        // Tạo objectives cho animal collection với độ khó tăng dần theo level
        List<QuestObjective> objectives = new List<QuestObjective>();
        
        // Số lượng objectives (loại animal khác nhau) - đảm bảo ít nhất 2 loại
        int objectivesCount = 2; // Mặc định ít nhất 2 loại
        if (questId <= 10)
        {
            objectivesCount = 2; // Đảm bảo ít nhất 2 loại
        }
        else if (questId <= 20)
        {
            objectivesCount = Random.Range(2, 4); // 2-3 loại
        }
        else if (questId <= 30)
        {
            objectivesCount = Random.Range(2, 4); // 2-3 loại
        }
        else
        {
            objectivesCount = Random.Range(2, 5); // 2-4 loại
        }
        
        // Xác định phạm vi AnimalType dựa trên level (độ khó tăng dần)
        AnimalType[] allAnimalTypes = (AnimalType[])System.Enum.GetValues(typeof(AnimalType));
        List<AnimalType> availableAnimalTypes = new List<AnimalType>();
        
        if (questId == 1)
        {
            // Level 1: Các động vật đơn giản
            availableAnimalTypes.Add(AnimalType.Cow);
            availableAnimalTypes.Add(AnimalType.Chicken);
            availableAnimalTypes.Add(AnimalType.Pig);
            availableAnimalTypes.Add(AnimalType.Sheep);
        }
        else if (questId <= 10)
        {
            // Level 2-10: Thêm các động vật phổ biến
            int minIndex = 0;  // Bear
            int maxIndex = 9;  // Duck
            for (int i = minIndex; i <= maxIndex; i++)
            {
                availableAnimalTypes.Add(allAnimalTypes[i]);
            }
        }
        else if (questId <= 20)
        {
            // Level 11-20: Thêm nhiều loại hơn
            int minIndex = 0;  // Bear
            int maxIndex = 14;  // Lion
            for (int i = minIndex; i <= maxIndex; i++)
            {
                availableAnimalTypes.Add(allAnimalTypes[i]);
            }
        }
        else if (questId <= 30)
        {
            // Level 21-30: Hầu hết các loại
            int minIndex = 0;  // Bear
            int maxIndex = 19;  // Turtle
            for (int i = minIndex; i <= maxIndex; i++)
            {
                availableAnimalTypes.Add(allAnimalTypes[i]);
            }
        }
        else
        {
            // Level 31+: Tất cả các loại động vật
            availableAnimalTypes.AddRange(allAnimalTypes);
        }
        
        // Xác định requiredAmount dựa trên level
        int minRequiredAmount = 1;
        int maxRequiredAmount = 3;
        
        if (questId <= 10)
        {
            minRequiredAmount = 1;
            maxRequiredAmount = 2;
        }
        else if (questId <= 20)
        {
            minRequiredAmount = 1;
            maxRequiredAmount = 3;
        }
        else if (questId <= 30)
        {
            minRequiredAmount = 2;
            maxRequiredAmount = 3;
        }
        else
        {
            minRequiredAmount = 2;
            maxRequiredAmount = 4;
        }
        
        // Đảm bảo có đủ loại animal để chọn
        if (availableAnimalTypes.Count < objectivesCount)
        {
            Debug.LogWarning($"QuestDataManager: Quest {questId} chỉ có {availableAnimalTypes.Count} loại animal khả dụng, nhưng cần {objectivesCount}. Giảm xuống {availableAnimalTypes.Count} loại.");
            objectivesCount = availableAnimalTypes.Count;
        }
        
        // Đảm bảo ít nhất 2 loại animal
        if (objectivesCount < 2 && availableAnimalTypes.Count >= 2)
        {
            objectivesCount = 2;
        }
        
        // Tạo objectives ngẫu nhiên
        for (int i = 0; i < objectivesCount && availableAnimalTypes.Count > 0; i++)
        {
            // Chọn ngẫu nhiên một AnimalType chưa dùng
            int randomIndex = Random.Range(0, availableAnimalTypes.Count);
            AnimalType randomAnimalType = availableAnimalTypes[randomIndex];
            availableAnimalTypes.RemoveAt(randomIndex); // Xóa để không trùng lặp
            
            // requiredAmount ngẫu nhiên trong phạm vi
            int requiredAmount = Random.Range(minRequiredAmount, maxRequiredAmount + 1);
            
            QuestObjective objective = new QuestObjective
            {
                animalType = randomAnimalType,
                requiredAmount = requiredAmount
            };
            objectives.Add(objective);
        }
        
        // Validation: Đảm bảo quest có ít nhất 2 loại animal
        if (objectives.Count < 2)
        {
            Debug.LogError($"QuestDataManager: Quest {questId} chỉ có {objectives.Count} loại animal, không đủ 2 loại! Vui lòng kiểm tra lại availableAnimalTypes.");
            // Nếu không đủ, thử thêm loại animal nếu còn available
            while (objectives.Count < 2 && availableAnimalTypes.Count > 0)
            {
                int randomIndex = Random.Range(0, availableAnimalTypes.Count);
                AnimalType randomAnimalType = availableAnimalTypes[randomIndex];
                availableAnimalTypes.RemoveAt(randomIndex);
                
                int requiredAmount = Random.Range(minRequiredAmount, maxRequiredAmount + 1);
                
                QuestObjective objective = new QuestObjective
                {
                    animalType = randomAnimalType,
                    requiredAmount = requiredAmount
                };
                objectives.Add(objective);
            }
        }
        
        questData.objectives = objectives.ToArray();
        
        // Thời gian mặc định tăng dần theo level (đã giảm)
        questData.timeFor3Stars = 60f + (questId - 1) * 2f; // 60s, 65s, 70s...
        questData.timeFor2Stars = questData.timeFor3Stars * 1.5f;
        
        // Reward mặc định tăng dần theo level (đã giảm)
        int baseReward = 10 + (questId - 1) * 2;
        questData.rewardList = new List<int>
        {
            baseReward,              // 1 sao
            Mathf.RoundToInt(baseReward * 1.5f),  // 2 sao
            baseReward * 2           // 3 sao
        };
        
        return questData;
    }

    /// <summary>
    /// Lấy quest theo ID từ cache
    /// </summary>
    public QuestData GetQuest(int questId)
    {
        if (questsCache.ContainsKey(questId))
        {
            return questsCache[questId];
        }
        
        // Nếu không có trong cache, thử load từ storage
        QuestData quest = QuestDataStorage.LoadQuest(questId);
        if (quest != null)
        {
            questsCache[questId] = quest;
        }
        
        return quest;
    }

    /// <summary>
    /// Lấy tất cả quest từ cache
    /// </summary>
    public Dictionary<int, QuestData> GetAllQuests()
    {
        return questsCache;
    }

    /// <summary>
    /// Lấy số lượng quest
    /// </summary>
    public int GetQuestCount()
    {
        return questsCache != null ? questsCache.Count : 0;
    }

    /// <summary>
    /// Lấy kết quả sao của một quest
    /// </summary>
    public int GetQuestStars(int questId)
    {
        return QuestDataStorage.GetQuestStars(questId);
    }

    /// <summary>
    /// Kiểm tra quest có bị locked không
    /// </summary>
    public bool IsQuestLocked(int questId)
    {
        return QuestDataStorage.IsQuestLocked(questId);
    }

    /// <summary>
    /// Refresh cache từ JSON
    /// </summary>
    public void Refresh()
    {
        questsCache = QuestDataStorage.LoadAllQuests();
        Debug.Log($"QuestDataManager: Đã refresh, có {questsCache.Count} quest");
    }
}

