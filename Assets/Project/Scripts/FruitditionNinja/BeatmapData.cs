using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeatNote
{
    public int comboId;             // ID nhóm combo
    public int glowFrame;           // frame đạt đỉnh (glow)
    public float glowTimeSec;       // giây đạt đỉnh
    public float spawnTimeSec;      // giây cần bắn để 1s sau lên đỉnh
    public Vector2 peakPosition;    // toạ độ đỉnh
    public Vector2 spawnPosition;   // toạ độ spawn
    public float shootSpeed;        // tốc độ bắn (độ lớn)
    public float shootAngle;        // góc bắn (degrees)
    public FruitType fruitType;     // loại quả
}

[Serializable]
public enum FruitType
{
    Watermelon,  // ~8x10
    Apple,       // ~5.5x5.5
    Orange,      // ~5.5x5.5
    Banana,      // ~8x8
    Grape        // ~9x6
}



[Serializable]
public class BeatMap
{
    public int mapId;
    public int rngSeed;             // để tái tạo
    public List<BeatNote> beatNotes = new();
}

[Serializable]
public class BeatMapCollection
{
    public List<BeatMap> beatMaps = new();
}
