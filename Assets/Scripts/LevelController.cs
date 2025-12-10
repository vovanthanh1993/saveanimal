using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelController : MonoBehaviour
{
    [Header("Level Setup")]
    public GameObject levelPrefab;
    public Transform contentRoot;

    [Range(1, 200)]
    public int totalLevels = 50;
    [Range(1, 20)]
    public int itemsPerPage = 10;

    [Header("Pagination UI")]
    public Button previousButton;
    public Button nextButton;

    private readonly List<Level> spawnedLevels = new List<Level>();
    private int currentPage;
    private PlayerData playerData;

    private void Start()
    {
        LoadPlayerData();
        BuildPage(0);

        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextPage);
    }
    
    /// <summary>
    /// Refresh lại UI với dữ liệu mới nhất từ PlayerData
    /// </summary>
    public void Refresh()
    {
        LoadPlayerData();
        // Luôn rebuild page để hiển thị số sao mới nhất
        BuildPage(currentPage);
    }

    private void LoadPlayerData()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }
        else
        {
            // Fallback nếu PlayerDataManager chưa được khởi tạo
            playerData = PlayerDataStorage.LoadOrCreateDefault(totalLevels);
        }
        
        // Lấy số lượng quest từ QuestDataManager
        if (QuestDataManager.Instance != null)
        {
            totalLevels = QuestDataManager.Instance.GetQuestCount();
        }
        else
        {
            // Fallback: load trực tiếp từ storage
            var allQuests = QuestDataStorage.LoadAllQuests();
            if (allQuests != null && allQuests.Count > 0)
            {
                totalLevels = allQuests.Count;
            }
        }
    }

    private void BuildPage(int pageIndex)
    {
        ClearExistingLevels();

        currentPage = Mathf.Clamp(pageIndex, 0, MaxPageIndex());

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, totalLevels);

        for (int i = startIndex; i < endIndex; i++)
        {
            Level levelComponent = Instantiate(levelPrefab, contentRoot).GetComponent<Level>();
            PlayerLevelData levelInfo = GetLevelInfo(i);
            if (levelInfo == null)
            {
                levelInfo = new PlayerLevelData
                {
                    level = i + 1,
                    star = 0,
                    isLocked = i != 0
                };
            }

            levelComponent.Init(levelInfo);
            spawnedLevels.Add(levelComponent);
        }

        UpdatePaginationButtons();
    }

    private void ClearExistingLevels()
    {
        if (contentRoot != null)
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }
        }

        spawnedLevels.Clear();
    }

    private void UpdatePaginationButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = currentPage < MaxPageIndex();
    }

    private int MaxPageIndex()
    {
        if (totalLevels == 0) return 0;
        return Mathf.Max(0, Mathf.CeilToInt((float)totalLevels / itemsPerPage) - 1);
    }

    private PlayerLevelData GetLevelInfo(int index)
    {
        int levelNumber = index + 1;
        
        // Lấy thông tin từ QuestDataManager
        int stars = 0;
        bool isLocked = true;
        
        if (QuestDataManager.Instance != null)
        {
            stars = QuestDataManager.Instance.GetQuestStars(levelNumber);
            isLocked = QuestDataManager.Instance.IsQuestLocked(levelNumber);
        }
        else
        {
            // Fallback: load trực tiếp từ storage
            stars = QuestDataStorage.GetQuestStars(levelNumber);
            isLocked = QuestDataStorage.IsQuestLocked(levelNumber);
        }
        
        return new PlayerLevelData
        {
            level = levelNumber,
            star = stars,
            isLocked = isLocked
        };
    }

    public void ShowNextPage()
    {
        if (currentPage >= MaxPageIndex()) return;
        BuildPage(currentPage + 1);
    }

    public void ShowPreviousPage()
    {
        if (currentPage <= 0) return;
        BuildPage(currentPage - 1);
    }
}
