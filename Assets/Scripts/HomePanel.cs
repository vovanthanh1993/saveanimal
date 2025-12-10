using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class HomePanel : MonoBehaviour
{
    public Button upgradeBtn;
    public Button playBtn;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;

    void Start()
    {
        upgradeBtn.onClick.AddListener(OnUpgradeButtonClicked); 
        playBtn.onClick.AddListener(OnPlayButtonClicked);
        UpdateRewardDisplay();
        UpdateHealthDisplay();
        UpdateDamageDisplay();
        UpdateSpeedDisplay();
    }

    void OnUpgradeButtonClicked()
    {
        //UIManager.Instance.ShowSettingPanel(true);
    }

    void OnPlayButtonClicked()
    {
        UIManager.Instance.ShowSelectLevelPanel(true);
    }

    private void OnEnable() {
        UIManager.Instance.ShowGamePlayPanel(false);
        UpdateRewardDisplay();
        UpdateHealthDisplay();
        UpdateDamageDisplay();
        UpdateSpeedDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị reward từ PlayerData
    /// </summary>
    public void UpdateRewardDisplay()
    {
        if (coinText != null)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            {
                coinText.text = PlayerDataManager.Instance.playerData.totalReward.ToString();
            }
            else
            {
                coinText.text = "0";
            }
        }
    }

    public void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            {
                healthText.text = PlayerDataManager.Instance.playerData.health.ToString();
            }
            else
            {
                healthText.text = "0";
            }
        }
    }

    public void UpdateDamageDisplay()
    {
        if (damageText != null)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            {
                damageText.text = PlayerDataManager.Instance.playerData.damage.ToString();
            }
            else
            {
                damageText.text = "0";
            }
        }
    }

    public void UpdateSpeedDisplay()
    {
        if (speedText != null)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            {
                speedText.text = PlayerDataManager.Instance.playerData.speed.ToString();
            }
            else
            {
                speedText.text = "0";
            }
        }
    }
}
