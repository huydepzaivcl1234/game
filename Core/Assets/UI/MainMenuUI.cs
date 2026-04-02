using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class MainMenuUI : MonoBehaviour
{
    [System.Serializable]
    class ButtonSfxOverride
    {
        public Button button;
        public AudioClip clip;
    }

    [SerializeField] GameObject settingsPanel;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Button applyResolutionButton;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Button playButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] Button creditsButton;
    [SerializeField] Button closeCreditsButton;
    [SerializeField] Button discordButton;
    [SerializeField] Button deleteDataButton;
    [SerializeField] GameObject supportPanel;
    [SerializeField] Button supportButton;
    [SerializeField] Button closeSupportButton;
    [SerializeField] string saveFileName = "save.json";
    [SerializeField] string discordInviteUrl = "https://discord.gg/yourinvite";
    [SerializeField] string gameplaySceneName = "GameplayScene";

    [Header("Audio")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip clickClip;
    [SerializeField] UnityEngine.Audio.AudioMixer uiMixer;
    [SerializeField] List<ButtonSfxOverride> customButtonSfx = new List<ButtonSfxOverride>();

    [Header("Audio Mixer Exposed Parameters")]
    [SerializeField] string masterVolumeExposedName = "MasterVol";
    [SerializeField] string sfxVolumeExposedName = "SfxVol";
    [SerializeField] string musicVolumeExposedName = "MusicVol";

    const string MasterKey = "masterVol";
    const string SfxKey = "sfxVol";
    const string MusicKey = "musicVol";
    const string ResWKey = "resWidth";
    const string ResHKey = "resHeight";
    const string ResFKey = "resFull";

    Resolution[] resolutions;
    int currentResIndex;

    void Awake()
    {
        resolutions = Screen.resolutions;

        if (resolutionDropdown != null)
        {
            BuildResolutionDropdown();
        }

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        LoadAudioPrefs();
        LoadFullscreenPref();

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (supportPanel != null) supportPanel.SetActive(false);

        if (applyResolutionButton != null) applyResolutionButton.onClick.AddListener(ApplySelectedResolution);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(CloseCredits);
        if (discordButton != null && discordButton.onClick.GetPersistentEventCount() == 0)
            discordButton.onClick.AddListener(OpenDiscord);
        if (deleteDataButton != null) deleteDataButton.onClick.AddListener(DeleteData);
        if (supportButton != null) supportButton.onClick.AddListener(OpenSupport);
        if (closeSupportButton != null) closeSupportButton.onClick.AddListener(CloseSupport);

        RegisterCustomButtonSfx();
    }

    void RegisterCustomButtonSfx()
    {
        if (customButtonSfx == null) return;

        for (int i = 0; i < customButtonSfx.Count; i++)
        {
            ButtonSfxOverride item = customButtonSfx[i];
            if (item == null || item.button == null || item.clip == null) continue;

            AudioClip clip = item.clip;
            item.button.onClick.AddListener(() => PlayCustomClick(clip));
        }
    }

    // Audio ------------------
    void ApplyMaster(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    void ApplySfx(float v)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(v);
    }

    void ApplyMusic(float v)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(v);
    }

    public void PlayClick()
    {
        if (clickClip != null && sfxSource != null) sfxSource.PlayOneShot(clickClip);
    }

    public void PlayCustomClick(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    void LoadAudioPrefs()
    {
        float master = PlayerPrefs.GetFloat(MasterKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);

        if (masterSlider != null) masterSlider.value = master;
        if (sfxSlider != null) sfxSlider.value = sfx;
        if (musicSlider != null) musicSlider.value = music;

        ApplyMaster(master);
        ApplySfx(sfx);
        ApplyMusic(music);
    }

    public void OnMasterChanged(float value)
    {
        ApplyMaster(value);
        PlayerPrefs.SetFloat(MasterKey, value);
    }
    public void OnSfxChanged(float value)
    {
        ApplySfx(value);
        PlayerPrefs.SetFloat(SfxKey, value);
    }
    public void OnMusicChanged(float value)
    {
        ApplyMusic(value);
        PlayerPrefs.SetFloat(MusicKey, value);
    }
    // ------------------------

    public void PlayGame()
    {
        PlayClick();
        if (Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        Debug.LogError("Cannot load scene: " + gameplaySceneName + ". Add it to Build Profiles Scene List or fix gameplaySceneName in MainMenuUI.");
    }
    public void QuitGame()
    {
        PlayClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OpenSettings() { PlayClick(); settingsPanel.SetActive(true); playButton.interactable = false; quitButton.interactable = false; }
    public void CloseSettings() { PlayClick(); settingsPanel.SetActive(false); playButton.interactable = true; quitButton.interactable = true; }
    public void OpenCredits() { PlayClick(); creditsPanel.SetActive(true); }
    public void CloseCredits() { PlayClick(); creditsPanel.SetActive(false); }
    public void OpenSupport() { PlayClick(); if (supportPanel != null) supportPanel.SetActive(true); }
    public void CloseSupport() { PlayClick(); if (supportPanel != null) supportPanel.SetActive(false); }
    public void OpenDiscord()
    {
        PlayClick();
        if (!string.IsNullOrWhiteSpace(discordInviteUrl))
        {
            Application.OpenURL(discordInviteUrl);
        }
    }

    void LoadFullscreenPref()
    {
        bool isFullscreen = PlayerPrefs.GetInt(ResFKey, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }

    void DeleteData()
    {
        PlayClick();
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save data deleted.");
        }
        else
        {
            Debug.Log("No save data found to delete.");
        }
    }

    void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int refreshRateHz = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + refreshRateHz + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void ApplySelectedResolution()
    {
        if (resolutionDropdown == null || fullscreenToggle == null || resolutions == null || resolutions.Length == 0) return;

        int selectedIndex = resolutionDropdown.value;
        Resolution selectedResolution = resolutions[selectedIndex];
        bool isFullscreen = fullscreenToggle.isOn;

        Screen.SetResolution(
            selectedResolution.width,
            selectedResolution.height,
            isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
            selectedResolution.refreshRateRatio
        );

        PlayerPrefs.SetInt(ResWKey, selectedResolution.width);
        PlayerPrefs.SetInt(ResHKey, selectedResolution.height);
        PlayerPrefs.SetInt(ResFKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}