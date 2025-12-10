using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WinPanel : MonoBehaviour
{
    public Button nextLevelBtn;
    public Button homeBtn;
    public Image starImage;
    public TextMeshProUGUI rewardText;
    void Start() {
        homeBtn.onClick.AddListener(OnHomeButtonClicked);   
        nextLevelBtn.onClick.AddListener(OnNextLevelButtonClicked);
    }

    void OnHomeButtonClicked()
    {
        GameCommonUtils.LoadScene("HomeScene");
        UIManager.Instance.ShowHomePanel(true);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    void OnNextLevelButtonClicked()
    {
        GameCommonUtils.LoadScene("HomeScene");
        UIManager.Instance.ShowHomePanel(true);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void Init(int star, int reward)
    {
        if (starImage != null)
        {
            starImage.sprite = Resources.Load<Sprite>("Stars/star_" + star.ToString());
        }
        if (rewardText != null)
        {
            rewardText.text = reward.ToString();
        }
    }
}
