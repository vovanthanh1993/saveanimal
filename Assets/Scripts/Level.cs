using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Level : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Image starImage;
    public bool isLocked = true;
    public Image lockImage;
    private PlayerLevelData levelData;

    public void Init(PlayerLevelData data) {
        levelData = data;
        isLocked = data.isLocked;
        levelText.text = data.level.ToString();
        starImage.sprite = Resources.Load<Sprite>("Stars/star_" + data.star.ToString());
        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(isLocked);
        }
    }

    public void OnClick()
    {
        Debug.Log("OnClick: " + levelData.level);
        if (isLocked) return;
        if (UIManager.Instance.startPanel != null)
        {
            UIManager.Instance.startPanel.ShowForLevel(levelData);
            UIManager.Instance.ShowSelectLevelPanel(false);
            AudioManager.Instance.PlayPopupSound();
        }
    }
}
