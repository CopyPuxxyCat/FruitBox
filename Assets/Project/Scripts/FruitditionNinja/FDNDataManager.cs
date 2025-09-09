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
    public SongProgressData[] songs = new SongProgressData[0]; // Khởi tạo mặc định
}

public class FDNDataManager : MonoBehaviour
{
    public static FDNDataManager Instance;
    private const string DATA_FILE = "fdn_songdata_save.json";

    public GameSaveData data;

    void Awake()
    {
        // Singleton pattern với null check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        // Khởi tạo data trước khi load
        data = new GameSaveData();
        LoadData();
    }

    void LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, DATA_FILE);

        try
        {
            if (File.Exists(path))
            {
                string encoded = File.ReadAllText(path);
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var loadedData = JsonUtility.FromJson<GameSaveData>(json);

                // Null check cho dữ liệu load được
                if (loadedData != null)
                {
                    data = loadedData;
                }

                Debug.Log($"Loaded {data.songs.Length} song progress data");
            }
            else
            {
                Debug.Log("No save file found. Creating new data.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
            data = new GameSaveData(); // Reset về default
        }
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            string path = Path.Combine(Application.persistentDataPath, DATA_FILE);
            File.WriteAllText(path, encoded);

            Debug.Log("Game data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    public void UpdateSongData(SongData song)
    {
        if (song == null)
        {
            Debug.LogWarning("Cannot update null song data");
            return;
        }

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
        if (song == null)
        {
            Debug.LogWarning("Cannot load data for null song");
            return;
        }

        var entry = Array.Find(data.songs, x => x.songName == song.songName);
        if (entry != null)
        {
            song.highScore = entry.highScore;
            song.stars = entry.stars;
            song.progress = entry.progress;
            song.unlocked = entry.unlocked;
        }
        else
        {
            // Set default values for new song
            song.highScore = 0;
            song.stars = 0;
            song.progress = 0f;
            // song.unlocked giữ nguyên giá trị mặc định trong ScriptableObject
        }
    }
}