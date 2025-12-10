using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingPanel : MonoBehaviour
{
    public Button homeBtn;
    public Button closeBtn;
    
    [Header("SFX Settings")]
    [Tooltip("Nút tăng SFX volume")]
    public Button sfxIncreaseBtn;
    [Tooltip("Nút giảm SFX volume")]
    public Button sfxDecreaseBtn;
    [Tooltip("Image để hiển thị SFX volume (fillAmount)")]
    public Image sfxFillImage;
    
    [Header("Music Settings")]
    [Tooltip("Nút tăng Music volume")]
    public Button musicIncreaseBtn;
    [Tooltip("Nút giảm Music volume")]
    public Button musicDecreaseBtn;
    [Tooltip("Image để hiển thị Music volume (fillAmount)")]
    public Image musicFillImage;
    
    [Header("Volume Settings")]
    [Tooltip("Bước tăng/giảm volume mỗi lần nhấn nút (0-1)")]
    [Range(0.01f, 0.2f)]
    public float volumeStep = 0.1f;
    
    private float currentSFXVolume = 1f;
    private float currentMusicVolume = 1f;

    private void OnEnable() {
        if(SceneManager.GetActiveScene().name == "HomeScene") 
            homeBtn.gameObject.SetActive(false);
        else homeBtn.gameObject.SetActive(true);
        
        // Load volume từ PlayerPrefs hoặc dùng giá trị mặc định
        LoadVolumeSettings();
        UpdateUI();
    }

    void Start() {
        homeBtn.onClick.AddListener(OnHomeButtonClicked);   
        closeBtn.onClick.AddListener(OnCloseButtonClicked);
        
        // SFX buttons
        if (sfxIncreaseBtn != null)
            sfxIncreaseBtn.onClick.AddListener(OnSFXIncreaseClicked);
        if (sfxDecreaseBtn != null)
            sfxDecreaseBtn.onClick.AddListener(OnSFXDecreaseClicked);
        
        // Music buttons
        if (musicIncreaseBtn != null)
            musicIncreaseBtn.onClick.AddListener(OnMusicIncreaseClicked);
        if (musicDecreaseBtn != null)
            musicDecreaseBtn.onClick.AddListener(OnMusicDecreaseClicked);
    }

    public void OnHomeButtonClicked(){
        GameCommonUtils.LoadScene("HomeScene");
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.ShowHomePanel(true);
    }

    public void OnCloseButtonClicked(){
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    
    #region SFX Volume Control
    
    private void OnSFXIncreaseClicked()
    {
        currentSFXVolume = Mathf.Clamp01(currentSFXVolume + volumeStep);
        ApplySFXVolume();
        UpdateUI();
        SaveVolumeSettings();
        
        // Phát sound khi điều chỉnh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayChangeSound();
        }
    }
    
    private void OnSFXDecreaseClicked()
    {
        currentSFXVolume = Mathf.Clamp01(currentSFXVolume - volumeStep);
        ApplySFXVolume();
        UpdateUI();
        SaveVolumeSettings();
        
        // Phát sound khi điều chỉnh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayChangeSound();
        }
    }
    
    private void ApplySFXVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(currentSFXVolume);
        }
    }
    
    #endregion
    
    #region Music Volume Control
    
    private void OnMusicIncreaseClicked()
    {
        currentMusicVolume = Mathf.Clamp01(currentMusicVolume + volumeStep);
        ApplyMusicVolume();
        UpdateUI();
        SaveVolumeSettings();
        
        // Phát sound khi điều chỉnh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayChangeSound();
        }
    }
    
    private void OnMusicDecreaseClicked()
    {
        currentMusicVolume = Mathf.Clamp01(currentMusicVolume - volumeStep);
        ApplyMusicVolume();
        UpdateUI();
        SaveVolumeSettings();
        
        // Phát sound khi điều chỉnh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayChangeSound();
        }
    }
    
    private void ApplyMusicVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(currentMusicVolume);
        }
    }
    
    #endregion
    
    #region UI Update
    
    private void UpdateUI()
    {
        // Update SFX UI
        if (sfxFillImage != null)
        {
            sfxFillImage.fillAmount = currentSFXVolume;
        }
        
        // Update Music UI
        if (musicFillImage != null)
        {
            musicFillImage.fillAmount = currentMusicVolume;
        }
    }
    
    #endregion
    
    #region Save/Load Settings
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("SFXVolume", currentSFXVolume);
        PlayerPrefs.SetFloat("MusicVolume", currentMusicVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        currentSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        currentMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        
        // Áp dụng volume ngay khi load
        ApplySFXVolume();
        ApplyMusicVolume();
    }
    
    #endregion
}
