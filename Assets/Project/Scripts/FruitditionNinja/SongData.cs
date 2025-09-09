using UnityEngine;

[CreateAssetMenu(fileName = "SongData", menuName = "FDN/SongData")]
public class SongData : ScriptableObject
{
    public string songName;
    public AudioClip songClip;
    public float duration;
    [Header("UI Info")]
    public Sprite songIcon;

    [Header("Gameplay Data")]
    public BeatMap beatmap; // Beatmap cố định (optional)

    [Header("Generated Beatmap")]
    [Tooltip("Nếu true, sẽ dùng generated beatmap thay vì beatmap cố định")]
    public bool useGeneratedBeatmap = true;

    [Header("Progress Data")]
    public int highScore;
    public int stars;      // 0-3
    public float progress; // second
    public bool unlocked;
}
