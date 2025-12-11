using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class WinPanel : MonoBehaviour
{
    public Button homeBtn;
    public List<GameObject> starList = new List<GameObject>(); // List 3 star objects
    public TextMeshProUGUI rewardText;
    
    void Start() {
        homeBtn.onClick.AddListener(OnHomeButtonClicked);
    }

    void OnHomeButtonClicked()
    {
        GameCommonUtils.LoadScene("HomeScene");
        UIManager.Instance.ShowHomePanel(true);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        AudioManager.Instance.PlayRewardSound();
    }

    public void Init(int star, int reward)
    {
        // Hiển thị stars dựa trên số sao đạt được (1-3)
        UpdateStarsDisplay(star);
        
        if (rewardText != null)
        {
            rewardText.text = reward.ToString();
        }
    }

    private void UpdateStarsDisplay(int starCount)
    {
        // Đảm bảo có đủ 3 stars
        if (starList.Count < 3)
        {
            Debug.LogWarning("WinPanel: Cần 3 star objects trong starList!");
            return;
        }

        // Hiển thị stars: hiện star nếu index < số sao đạt được, ẩn nếu không
        for (int i = 0; i < 3; i++)
        {
            if (starList[i] != null)
            {
                starList[i].SetActive(i < starCount);
            }
        }
    }
}
