using UnityEngine;

public class GamePlayPanel : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;

    private void OnEnable()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }

    public void ShowWinPanel(bool isShow, int star, int reward){
        AudioManager.Instance.PlayWinSound();
        winPanel.SetActive(isShow);
        winPanel.GetComponent<WinPanel>().Init(star, reward);
    }

    public void ShowLosePanel(bool isShow){
        AudioManager.Instance.PlayLoseSound();
        losePanel.SetActive(isShow);
    }
}
