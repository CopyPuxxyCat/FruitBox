using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FDNAudioController : MonoBehaviour
{
    public static FDNAudioController Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource BGmusicSource;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Slider musicTimeline;

    private SongData currentSong;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);

        countdownText.gameObject.SetActive(false);
    }

    private void Start()
    {
        musicTimeline.gameObject.SetActive(false);
    }

    public void PlaySong(SongData song)
    {
        currentSong = song;
        StartCoroutine(CountdownAndPlay());

        Invoke(nameof(OnSongEnd), song.songClip.length); // đổi thành gắn 1 cái tag vào cuối bản nhạc bool SongEndTag để có thể mở rộng dùng skill các thứ
    }

    private IEnumerator CountdownAndPlay()
    {
        countdownText.gameObject.SetActive(true);
        musicTimeline.gameObject.SetActive(true);
        BGmusicSource.gameObject.SetActive(false);

        countdownText.text = "1";
        TweenText(1f);
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        TweenText(1f);
        yield return new WaitForSeconds(1f);
        countdownText.text = "3";
        TweenText(1f);
        yield return new WaitForSeconds(1f);
        countdownText.text = "START!";
        TweenText(0.5f);
        yield return new WaitForSeconds(0.5f);

        countdownText.gameObject.SetActive(false);

        musicSource.clip = currentSong.songClip;
        musicSource.Play();

        // Reset timeline
        musicTimeline.maxValue = currentSong.duration;
        musicTimeline.value = 0f;
    }

    private void TweenText(float time)
    {
        countdownText.rectTransform.localScale = Vector3.zero;
        LeanTween.scale(countdownText.rectTransform, Vector3.one, time).setEaseOutElastic();
    }    

    private void OnSongEnd()
    {
        Debug.Log($"Song ended: {currentSong.songName}");
        BGmusicSource.gameObject.SetActive(true);
        // Cập nhật dữ liệu
        FDNDataManager.Instance.UpdateSongData(currentSong);

        // TODO: mở panel kết thúc game
        
    }

    private void Update()
    {
        if (musicSource.isPlaying)
        {
            musicTimeline.value = musicSource.time;
        }
    }

    public float GetSongTime() => musicSource.time;
}
