using System;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class SongProgressData
{
    public string songName;
    public int highScore;
    public int stars;
    public float progress;
    public bool unlocked;
}

[Serializable]
public class GameSaveData
{
    public SongProgressData[] songs;
}

public class FDNDataManager : MonoBehaviour
{
    public static FDNDataManager Instance;
    private const string DATA_FILE = "fdn_save.json";

    public GameSaveData data;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);

        LoadData();
    }

    void LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, DATA_FILE);
        if (File.Exists(path))
        {
            string encoded = File.ReadAllText(path);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            data = JsonUtility.FromJson<GameSaveData>(json);
        }
        else
        {
            data = new GameSaveData { songs = new SongProgressData[0] };
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(data);
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        string path = Path.Combine(Application.persistentDataPath, DATA_FILE);
        File.WriteAllText(path, encoded);
    }

    public void UpdateSongData(SongData song)
    {
        var entry = Array.Find(data.songs, x => x.songName == song.songName);
        if (entry == null)
        {
            entry = new SongProgressData { songName = song.songName };
            Array.Resize(ref data.songs, data.songs.Length + 1);
            data.songs[data.songs.Length - 1] = entry;
        }

        entry.highScore = Mathf.Max(entry.highScore, song.highScore);
        entry.stars = Mathf.Max(entry.stars, song.stars);
        entry.progress = Mathf.Max(entry.progress, song.progress);
        entry.unlocked = song.unlocked;

        SaveData();
    }

    public void LoadSongData(SongData song)
    {
        var entry = Array.Find(data.songs, x => x.songName == song.songName);
        if (entry != null)
        {
            song.highScore = entry.highScore;
            song.stars = entry.stars;
            song.progress = entry.progress;
            song.unlocked = entry.unlocked;
        }
    }
}
