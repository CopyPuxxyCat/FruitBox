using System.Collections;
using UnityEngine;

public class FruitditionNinjaGameManager : MonoBehaviour
{
    public static FruitditionNinjaGameManager Instance;

    private SongData currentSong;
    private BeatNoteGenerator beatNoteGenerator;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tìm BeatNoteGenerator trong scene
        beatNoteGenerator = FindObjectOfType<BeatNoteGenerator>();
        if (beatNoteGenerator == null)
        {
            Debug.LogWarning("BeatNoteGenerator not found! Generated beatmaps won't work.");
        }
    }

    public void StartSong(SongData song)
    {
        currentSong = song;

        FDNAudioController.Instance.PlaySong(song);

        StartCoroutine(WaitForCountDown(song));
    }

    private IEnumerator WaitForCountDown(SongData song)
    {
        yield return new WaitForSeconds(3.5f);
        
        var spawner = FindObjectOfType<FruitSpawner>();

        // Quyết định dùng beatmap nào
        BeatMap beatmapToUse = GetBeatmapForSong(song);

        if (beatmapToUse != null)
        {
            spawner.LoadBeatmap(beatmapToUse);
        }
        else
        {
            Debug.LogError($"No beatmap available for song: {song.songName}");
        }
    }

    private BeatMap GetBeatmapForSong(SongData song)
    {
        if (song.useGeneratedBeatmap)
        {
            // Dùng generated beatmap
            if (beatNoteGenerator != null)
            {
                var generatedMap = beatNoteGenerator.GetRandomBeatMap();
                if (generatedMap != null)
                {
                    Debug.Log($"Using generated beatmap ID: {generatedMap.mapId}");
                    return generatedMap;
                }
                else
                {
                    Debug.LogWarning("No generated beatmaps available! Generate some first.");
                }
            }
        }

        // Fallback về beatmap cố định
        if (song.beatmap != null)
        {
            Debug.Log("Using fixed beatmap from SongData");
            return song.beatmap;
        }

        return null;
    }

    public SongData GetCurrentSong() => currentSong;

    [Header("Input")]
    [SerializeField] private KeyCode submitComboKey = KeyCode.Space;

    private void Update()
    {
        // Handle manual combo submission
        if (Input.GetKeyDown(submitComboKey))
        {
            SubmitCurrentCombos();
        }
    }

    public void SubmitCurrentCombos()
    {
        if (ComboPanelManager.Instance != null)
        {
            ComboPanelManager.Instance.SubmitComboManually();
        }
    }
}