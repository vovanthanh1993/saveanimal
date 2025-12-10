using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UpgradePanel : MonoBehaviour
{
    public Button healthBtn;
    public Button damageBtn;
    public Button speedBtn;

    public int healthAddValue = 5;
    public int damageAddValue = 5;
    public int speedAddValue = 2;

    public int healthPrice;
    public int damagePrice;
    public int speedPrice;

    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI damagePriceText;
    public TextMeshProUGUI speedPriceText;

    public TextMeshProUGUI healthValueText;
    public TextMeshProUGUI damageValueText;
    public TextMeshProUGUI speedValueText;

    void Start()
    {
        if (healthBtn != null)
        {
            healthBtn.onClick.AddListener(OnHealthButtonClicked);
        }
        if (damageBtn != null)
        {
            damageBtn.onClick.AddListener(OnDamageButtonClicked);
        }
        if (speedBtn != null)
        {
            speedBtn.onClick.AddListener(OnSpeedButtonClicked);
        }
        if (healthPriceText != null)
        {
            healthPriceText.text = healthPrice.ToString();
        }
        if (damagePriceText != null)
        {
            damagePriceText.text = damagePrice.ToString();
        }
        if (speedPriceText != null)
        {
            speedPriceText.text = speedPrice.ToString();
        }
        if(healthAddValue != 0)
        {
            healthValueText.text = "+ " + healthAddValue.ToString() + " health";
        }
        if(damageAddValue != 0)
        {
            damageValueText.text = "+ " + damageAddValue.ToString() + " damage";
        }
        if(speedAddValue != 0)
        {
            speedValueText.text = "+ " + speedAddValue.ToString() + " speed";
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút upgrade health
    /// </summary>
    void OnHealthButtonClicked()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null)
            return;

        PlayerData playerData = PlayerDataManager.Instance.playerData;

        // Kiểm tra xem có đủ reward để mua không
        if (playerData.totalReward >= healthPrice)
        {
            // Trừ reward
            playerData.totalReward -= healthPrice;
            
            // Cộng health
            playerData.health += healthAddValue;
            
            // Lưu PlayerData
            PlayerDataManager.Instance.Save();
            
            Debug.Log($"Đã upgrade health +{healthAddValue}. Health hiện tại: {playerData.health}, Reward còn lại: {playerData.totalReward}");
            
            // Cập nhật hiển thị trong HomePanel nếu có
            UpdateHomePanelDisplay();
            UIManager.Instance.noticePanel.Init("Health upgraded successfully! \nCurrent health: " + playerData.health);
            AudioManager.Instance.PlaySuccessSound();
        }
        else
        {
            Debug.Log($"Không đủ reward! Cần {healthPrice}, hiện có {playerData.totalReward}");
            UIManager.Instance.noticePanel.Init("Insufficient resources!");
            AudioManager.Instance.PlayFailSound();
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút upgrade damage
    /// </summary>
    void OnDamageButtonClicked()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null)
            return;

        PlayerData playerData = PlayerDataManager.Instance.playerData;

        // Kiểm tra xem có đủ reward để mua không
        if (playerData.totalReward >= damagePrice)
        {
            // Trừ reward
            playerData.totalReward -= damagePrice;
            
            // Cộng damage
            playerData.damage += damageAddValue;
            
            // Lưu PlayerData
            PlayerDataManager.Instance.Save();
            
            Debug.Log($"Đã upgrade damage +{damageAddValue}. Damage hiện tại: {playerData.damage}, Reward còn lại: {playerData.totalReward}");
            UIManager.Instance.noticePanel.Init("Damage upgraded successfully! \nCurrent damage: " + playerData.damage);
            // Cập nhật hiển thị trong HomePanel nếu có
            UpdateHomePanelDisplay();
            AudioManager.Instance.PlaySuccessSound();
        }
        else
        {
            Debug.Log($"Không đủ reward! Cần {damagePrice}, hiện có {playerData.totalReward}");
            UIManager.Instance.noticePanel.Init("Insufficient resources!");
            AudioManager.Instance.PlayFailSound();
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút upgrade speed
    /// </summary>
    void OnSpeedButtonClicked()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null)
            return;

        PlayerData playerData = PlayerDataManager.Instance.playerData;

        // Kiểm tra xem có đủ reward để mua không
        if (playerData.totalReward >= speedPrice)
        {
            // Trừ reward
            playerData.totalReward -= speedPrice;
            
            // Cộng speed
            playerData.speed += speedAddValue;
            
            // Lưu PlayerData
            PlayerDataManager.Instance.Save();
            
            Debug.Log($"Đã upgrade speed +{speedAddValue}. Speed hiện tại: {playerData.speed}, Reward còn lại: {playerData.totalReward}");
            
            // Cập nhật hiển thị trong HomePanel nếu có
            UpdateHomePanelDisplay();
            UIManager.Instance.noticePanel.Init("Speed upgraded successfully! \nCurrent speed: " + playerData.speed);
            AudioManager.Instance.PlaySuccessSound();
        }
        else
        {
            Debug.Log($"Không đủ reward! Cần {speedPrice}, hiện có {playerData.totalReward}");
            UIManager.Instance.noticePanel.Init("Insufficient resources!");
            AudioManager.Instance.PlayFailSound();
        }
    }

    /// <summary>
    /// Cập nhật hiển thị trong HomePanel
    /// </summary>
    void UpdateHomePanelDisplay()
    {
        HomePanel homePanel = FindFirstObjectByType<HomePanel>();
        if (homePanel != null)
        {
            homePanel.UpdateRewardDisplay();
            homePanel.UpdateHealthDisplay();
            homePanel.UpdateDamageDisplay();
            homePanel.UpdateSpeedDisplay();
        }
    }
}
