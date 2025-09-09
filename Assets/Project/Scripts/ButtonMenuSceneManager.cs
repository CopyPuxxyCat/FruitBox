using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

public class ButtonMenuSceneManager : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] public bool isSettingPanelOpen;
    [SerializeField] private GameObject gameSelectionPanel;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource Btn_Audio;

    private void Start()
    {
        settingsPanel.transform.localScale = Vector2.zero;
        settingsPanel.SetActive(false);
        isSettingPanelOpen = false;
        gameSelectionPanel.SetActive(false);

        // Load saved values
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // Optional: add listeners directly (or via UI)
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void StartGame()
    {
        PlayBtnSound();
        SceneManager.LoadScene("Level_1");
    }

    public void BackToMainMenu()
    {
        PlayBtnSound();
        SceneManager.LoadScene("MenuScene");
    }    

    public void ExitGame()
    {
#if UNITY_EDITOR
        PlayBtnSound();
        UnityEditor.EditorApplication.isPlaying = false;
#else
        PlayBtnSound();
        Application.Quit();
#endif
    }

    public void OpenSettingsPanel()
    {
        PlayBtnSound();
        settingsPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(settingsPanel.GetComponent<RectTransform>());
        isSettingPanelOpen = true;
        settingsPanel.transform.LeanScale(Vector2.one, 0.5f);
    }

    public void CloseSettingsPanel()
    {
        PlayBtnSound();
        isSettingPanelOpen = false;
        settingsPanel.transform.LeanScale(Vector2.zero, 1f).setEaseInBack().setOnComplete(OnCloseSettingComplete);
    }

    void OnCloseSettingComplete()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenGameSelectionPanel()
    {
        PlayBtnSound();
        gameSelectionPanel.SetActive(true);
        gameSelectionPanel.transform.LeanScale(Vector2.one, 0.5f);
    }

    public void CloseGameSelectionPanel()
    {
        PlayBtnSound();
        gameSelectionPanel.transform.LeanScale(Vector2.zero, 1f).setEaseInBack().setOnComplete(OnCloseGameSelectionComplete);
    }

    void OnCloseGameSelectionComplete()
    {
        gameSelectionPanel.SetActive(false);
    }

    IEnumerator DelayAction(float time)
    {
        yield return new WaitForSeconds(time);
    }

    // Volume control function
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void PlayBtnSound()
    {
        if (Btn_Audio != null)
            Btn_Audio.Play();
    }   
    
    
}

