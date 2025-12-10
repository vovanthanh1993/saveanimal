using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject introPanel;
    public HomePanel homePanel;

    public GameObject selectLevelPanel;

    public StartPanel startPanel;

    public GamePlayPanel gamePlayPanel;

    public GameObject loadingPanel;

    public NoticePanel noticePanel;

    public SettingPanel settingPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoadingPanel(bool isShow) {
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(isShow);
        }
    }

    public void ShowSelectLevelPanel(bool isShow) {
        if (selectLevelPanel != null)
        {
            selectLevelPanel.SetActive(isShow);
            
            // Refresh LevelController khi panel được hiển thị
            if (isShow)
            {
                LevelController levelController = selectLevelPanel.GetComponentInChildren<LevelController>();
                if (levelController != null)
                {
                    levelController.Refresh();
                }
            }
        }
    }

    public void ShowGamePlayPanel(bool isShow) {
        if (gamePlayPanel != null)
        {
            gamePlayPanel.gameObject.SetActive(isShow);
        }
    }

    public void ShowIntroPanel(bool isShow) {
        if (introPanel != null)
        {
            introPanel.SetActive(isShow);
        }
    }

    public void ShowHomePanel(bool isShow) {
        if (homePanel != null)
        {
            homePanel.gameObject.SetActive(isShow);
        }
    }

    private void OnEnable() {
        selectLevelPanel.gameObject.SetActive(false);
        gamePlayPanel.gameObject.SetActive(false);
        introPanel.SetActive(true);
        homePanel.gameObject.SetActive(false);
        noticePanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
    }

    public void ShowSettingPanel(bool isShow) {
        if (settingPanel != null)
        {
            settingPanel.gameObject.SetActive(isShow);
        }
    }
}
