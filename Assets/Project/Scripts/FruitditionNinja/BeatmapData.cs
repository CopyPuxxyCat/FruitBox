using UnityEngine;

[System.Serializable]
public class BeatNote
{
    public float time;         // giây trong bài nhạc
    public string fruitType;   // loại quả
    public float force;        // lực ném
    public float angle;        // góc lệch (trái/phải)
}

[CreateAssetMenu(fileName = "BeatmapData", menuName = "FDN/Beatmap")]
public class BeatmapData : ScriptableObject
{
    public SongData song;
    public BeatNote[] notes;
}
