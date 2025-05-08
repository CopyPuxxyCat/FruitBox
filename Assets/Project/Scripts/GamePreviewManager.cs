using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System;
using TMPro;

/// <summary>
/// Manages game preview playback and transition UI.
/// </summary>
public class GamePreviewManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private GameObject playButton;
    [SerializeField] private CanvasGroup playButtonCanvasGroup;
    [SerializeField] private TextMeshProUGUI placeholderText;

    [Header("Game Video Clips")]
    [SerializeField] private VideoClip sumTenClip;
    [SerializeField] private VideoClip dropPuzzleClip;
    [SerializeField] private VideoClip sliceMergeClip;

    private string selectedSceneName;

    private void Awake()
    {
        ValidateSerializedFields();
        InitializeUI();

        videoPlayer.started += OnVideoStarted;
    }

    /// <summary>
    /// Called from UI button to preview a selected game.
    /// </summary>
    /// 

    private void OnDestroy()
    {
        videoPlayer.started -= OnVideoStarted; 
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        videoDisplay.gameObject.SetActive(true); 
    }

    public void ShowPreview(string gameName)
    {
        placeholderText.gameObject.SetActive(false);
        playButtonCanvasGroup.alpha = 0f;
        playButton.SetActive(false);
        videoDisplay.gameObject.SetActive(false);

        if (!TrySetVideoClip(gameName))
        {
            Debug.LogWarning($"Invalid game name: {gameName}");
            return;
        }

        videoPlayer.Play();

        // Delay then fade in play button
        playButton.SetActive(true);
        LeanTween.alphaCanvas(playButtonCanvasGroup, 1f, 0.5f)
                 .setDelay(2f);
    }

    /// <summary>
    /// Loads the selected game's scene.
    /// </summary>
    public void PlaySelectedGame()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            SceneManager.LoadScene(selectedSceneName);
        }
        else
        {
            Debug.LogWarning("No game selected to play.");
        }
    }

    /// <summary>
    /// Sets the correct video clip and target scene.
    /// </summary>
    private bool TrySetVideoClip(string gameName)
    {
        switch (gameName)
        {
            case "SumTen":
                videoPlayer.clip = sumTenClip;
                selectedSceneName = "Level_1";
                return true;
            case "DropPuzzle":
                videoPlayer.clip = dropPuzzleClip;
                selectedSceneName = "Level_1";
                return true;
            case "SliceMerge":
                videoPlayer.clip = sliceMergeClip;
                selectedSceneName = "Level_1";
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Ensures all required references are assigned.
    /// </summary>
    private void ValidateSerializedFields()
    {
        if (!videoPlayer) Debug.LogError("VideoPlayer not assigned.");
        if (!videoDisplay) Debug.LogError("VideoDisplay not assigned.");
        if (!playButton) Debug.LogError("PlayButton not assigned.");
        if (!playButtonCanvasGroup) Debug.LogError("PlayButtonCanvasGroup not assigned.");
        if (!placeholderText) Debug.LogError("PlaceholderText not assigned.");
    }

    /// <summary>
    /// Initializes UI state at startup.
    /// </summary>
    private void InitializeUI()
    {
        playButton.SetActive(false);
        playButtonCanvasGroup.alpha = 0f;
        placeholderText.gameObject.SetActive(true);
        placeholderText.text = "Select Game";
        videoDisplay.gameObject.SetActive(false);
    }
}


