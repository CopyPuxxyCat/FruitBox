using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitditionNinjaGameManager : MonoBehaviour
{
    public static FruitditionNinjaGameManager Instance;

    private SongData currentSong;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        Debug.Log($"Starting song: {song.songName}");
        var spawner = FindObjectOfType<FruitSpawner>();
        spawner.LoadBeatmap(song.beatmap);
    }    

    public SongData GetCurrentSong() => currentSong;
}
