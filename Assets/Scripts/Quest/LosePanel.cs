using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LosePanel : MonoBehaviour
{
    public Button retryBtn;
    public Button homeBtn;

    void Start() {
        retryBtn.onClick.AddListener(OnRetryButtonClicked);   
        homeBtn.onClick.AddListener(OnHomeButtonClicked);   
    }

    void OnRetryButtonClicked()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        GameCommonUtils.LoadScene(sceneName);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnHomeButtonClicked() {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.ShowHomePanel(true);
        GameCommonUtils.LoadScene("HomeScene");
    }
}
