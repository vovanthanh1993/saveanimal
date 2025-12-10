using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class NoticePanel : MonoBehaviour
{
    public TextMeshProUGUI noticeText;

    public void Init(string noticeText)
    {
        this.noticeText.text = noticeText;
        gameObject.SetActive(true);
    }
}
