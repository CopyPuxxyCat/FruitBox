using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongItemUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI songNameText;
    public Slider progressSlider;
    public Image[] starImages;
    public Sprite starEmptySprite;
    public Sprite starFullSprite;
    public TextMeshProUGUI highScoreText;
    public Image lockIcon;
    public Button playButton;

    private SongData songData;
    private System.Action<SongData> onPlayCallback;

    public void Setup(SongData data, System.Action<SongData> onPlay)
    {
        songData = data;
        onPlayCallback = onPlay;

        songNameText.text = data.songName;
        progressSlider.maxValue = data.duration;
        progressSlider.value = data.progress;

        // Màu slider
        ColorBlock colors = progressSlider.colors;
        colors.disabledColor = data.unlocked ? Color.blue : Color.gray;
        progressSlider.colors = colors;

        // High Score
        highScoreText.text = $"HighScore: {data.highScore}";

        // Stars
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].sprite = i < data.stars ? starFullSprite : starEmptySprite;
        }

        // Lock
        lockIcon.gameObject.SetActive(!data.unlocked);

        // Nút play
        playButton.interactable = data.unlocked;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => onPlayCallback?.Invoke(songData));
    }
}
