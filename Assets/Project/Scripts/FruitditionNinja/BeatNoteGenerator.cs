using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;


public class BeatNoteGenerator : MonoBehaviour
{
    private const int FRAME_RATE = 60;

    [Header("Physics / Timing")]
    [Tooltip("Thời gian từ lúc bắn đến khi chạm đỉnh (giây). Theo mô tả là ~1.0s.")]
    public float timeToReachPeak = 1f;
    [Tooltip("Gia tốc trọng lực dương; nội bộ sẽ tính hướng rơi xuống.")]
    public float gravity = 9.81f;

    [Header("Peak Ranges (world units)")]
    public float peakXMin = -20f;
    public float peakXMax = 20f;
    public float peakYMin = 0f;
    public float peakYMax = 33f;

    [Header("Spawn Line (world units)")]
    public float spawnY = -40f;
    public float spawnXMin = -30f;
    public float spawnXMax = 30f;

    [Header("In-Combo Gap")]
    [Tooltip("Khoảng cách thời gian giữa 2 quả trong cùng combo (giây).")]
    public float inComboGap = 0.3f;

    [Header("Fruit Sizes (for spacing)")]
    // dùng làm padding vị trí đỉnh để tránh chồng chéo
    public Vector2 watermelonSize = new(8f, 10f);
    public Vector2 appleSize = new(5.5f, 5.5f);
    public Vector2 orangeSize = new(5.5f, 5.5f);
    public Vector2 bananaSize = new(8f, 8f);
    public Vector2 grapeSize = new(9f, 6f);

    private Dictionary<FruitType, Vector2> fruitSizes;

    private BeatMapCollection beatMapCollection = new();
    private string saveFilePath;

    // Cấu trúc combo theo “frame” như yêu cầu
    private struct ComboBand
    {
        public int startFrame;
        public int endFrame;
        public int fruitCount;     // số quả / combo
        public float comboSpacing; // giây giữa 2 COMBO liên tiếp trong band

        public ComboBand(int s, int e, int count, float gap)
        { startFrame = s; endFrame = e; fruitCount = count; comboSpacing = gap; }
    }

    private ComboBand[] GetBands() => new ComboBand[]
    {
        new(  1,   9, 1, 2f),   // 1–9f: combo 1, cách 2s
        new( 11,  33, 2, 2f),   // 11–33f: combo 2, cách 2s
        new( 35,  66, 2, 2f),   // 35–66f: combo 2, cách 2s
        new( 68,  92, 3, 2.5f), // 68–92f: combo 3, cách 2.5s
        new( 94, 137, 3, 3f),   // 94–137f: combo 3, cách 3s
        new(139, 161, 4, 3f),   // 139–161f: combo 4, cách 3s
        new(163, 186, 5, 3f),   // 163–186f: combo 5, cách 3s
        new(190, 190, 1, 0f),   // 190f: 1 quả cuối
    };

    void Awake()
    {
        fruitSizes = new Dictionary<FruitType, Vector2>
        {
            { FruitType.Watermelon, watermelonSize },
            { FruitType.Apple,      appleSize },
            { FruitType.Orange,     orangeSize },
            { FruitType.Banana,     bananaSize },
            { FruitType.Grape,      grapeSize },
        };

        saveFilePath = Path.Combine(Application.persistentDataPath, "BeatMaps.json");
        LoadBeatMaps();
    }

    // ========= CORE GENERATOR =========

    public void Generate10BeatMaps()
    {
        beatMapCollection.beatMaps.Clear();
        for (int i = 0; i < 10; i++)
        {
            // seed để tái tạo chính xác
            int seed = Mathf.FloorToInt(UnityEngine.Random.value * int.MaxValue);
            beatMapCollection.beatMaps.Add(GenerateBeatMap(seed));
        }
        SaveBeatMaps();
        Debug.Log($"Generated & saved {beatMapCollection.beatMaps.Count} maps → {saveFilePath}");
    }

    public BeatMap GenerateBeatMap(int rngSeed)
    {
        var rng = new System.Random(rngSeed);
        var map = new BeatMap { mapId = beatMapCollection.beatMaps.Count, rngSeed = rngSeed };

        int comboId = 0;
        foreach (var band in GetBands())
        {
            // Các thời điểm đỉnh (glow) của combo trong band
            var glowFrames = MakeGlowFramesInBand(band, rng);

            foreach (int baseGlowFrame in glowFrames)
            {
                // Quyết định vị trí đỉnh cho combo này
                var peaks = MakeComboPeakPositions(band.fruitCount, rng);

                // Sinh các note trong combo (cách nhau inComboGap)
                for (int i = 0; i < band.fruitCount; i++)
                {
                    int glowFrame = baseGlowFrame + Mathf.RoundToInt(i * inComboGap * FRAME_RATE);
                    float glowTimeSec = glowFrame / (float)FRAME_RATE;

                    // Loại quả ngẫu nhiên
                    var fruitType = (FruitType)rng.Next(0, Enum.GetValues(typeof(FruitType)).Length);

                    // Spawn position
                    float spawnX = LerpRandom(spawnXMin, spawnXMax, rng);
                    var spawnPos = new Vector2(spawnX, spawnY);

                    // Tính vận tốc ban đầu để sau đúng timeToReachPeak tới peak
                    Vector2 peakPos = peaks[i];
                    (float speed, float angleDeg) = SolveBallistic(spawnPos, peakPos, timeToReachPeak, gravity);

                    // Thời điểm cần bắn = glowTime - timeToReachPeak
                    float spawnTimeSec = glowTimeSec - timeToReachPeak;
                    if (spawnTimeSec < 0f) spawnTimeSec = 0f; // clamp đầu bài

                    map.beatNotes.Add(new BeatNote
                    {
                        comboId = comboId,
                        glowFrame = glowFrame,
                        glowTimeSec = glowTimeSec,
                        spawnTimeSec = spawnTimeSec,
                        peakPosition = peakPos,
                        spawnPosition = spawnPos,
                        shootSpeed = speed,
                        shootAngle = angleDeg,
                        fruitType = fruitType
                    });
                }

                comboId++;
            }
        }

        // Sắp theo thời gian spawn để khi chơi chỉ việc đọc tuần tự
        map.beatNotes.Sort((a, b) => a.spawnTimeSec.CompareTo(b.spawnTimeSec));
        return map;
    }

    // ========= HELPERS =========

    // Sinh các frame “đỉnh” (glow) trong một band
    private List<int> MakeGlowFramesInBand(ComboBand band, System.Random rng)
    {
        var frames = new List<int>();

        if (band.startFrame == 190 && band.endFrame == 190)
        {
            frames.Add(190);
            return frames;
        }

        int gapFrames = Mathf.RoundToInt(band.comboSpacing * FRAME_RATE);
        int f = band.startFrame;

        while (f <= band.endFrame)
        {
            frames.Add(f);
            f += gapFrames;
        }

        return frames;
    }

    // Dàn đều (>=4) hoặc random có tránh đụng (<=3)
    private List<Vector2> MakeComboPeakPositions(int fruitCount, System.Random rng)
    {
        var peaks = new List<Vector2>();

        // padding theo quả to nhất để giảm overlap
        float largestWidth = Mathf.Max(watermelonSize.x, bananaSize.x, grapeSize.x, appleSize.x, orangeSize.x);
        float margin = largestWidth * 0.6f;

        if (fruitCount >= 4)
        {
            float usableMin = peakXMin + margin;
            float usableMax = peakXMax - margin;
            float span = usableMax - usableMin;
            float step = span / Mathf.Max(1, (fruitCount - 1));

            for (int i = 0; i < fruitCount; i++)
            {
                float x = usableMin + i * step;
                float y = UnityEngine.Random.Range(peakYMin, peakYMax); // chút ngẫu nhiên theo trục Y
                peaks.Add(new Vector2(x, y));
            }
        }
        else
        {
            int triesMax = 60;
            while (peaks.Count < fruitCount && triesMax-- > 0)
            {
                float x = UnityEngine.Random.Range(peakXMin + margin, peakXMax - margin);
                float y = UnityEngine.Random.Range(peakYMin, peakYMax);
                var candidate = new Vector2(x, y);

                if (!TooClose(candidate, peaks, largestWidth * 0.8f))
                    peaks.Add(candidate);
            }

            // fallback nếu quá khó tìm
            while (peaks.Count < fruitCount)
            {
                float x = Mathf.Lerp(peakXMin, peakXMax, peaks.Count / (float)fruitCount);
                float y = UnityEngine.Random.Range(peakYMin, peakYMax);
                peaks.Add(new Vector2(x, y));
            }
        }

        return peaks;
    }

    private bool TooClose(Vector2 p, List<Vector2> existing, float minDist)
    {
        foreach (var e in existing)
            if (Vector2.Distance(p, e) < minDist) return true;
        return false;
    }

    // Giải bài toán ném xiên: cho spawn→peak trong t giây, gravity dương (9.81)
    private (float speed, float angleDeg) SolveBallistic(Vector2 spawn, Vector2 peak, float t, float gPos)
    {
        float dx = peak.x - spawn.x;
        float dy = peak.y - spawn.y;

        // vy0*t + (-g)*t^2/2 = dy  => vy0 = (dy + 0.5*g*t^2)/t
        float vy0 = (dy + 0.5f * gPos * t * t) / t;
        // vx0 * t = dx
        float vx0 = dx / t;

        float speed = Mathf.Sqrt(vx0 * vx0 + vy0 * vy0);
        float angle = Mathf.Atan2(vy0, vx0) * Mathf.Rad2Deg;
        return (speed, angle);
    }

    private float LerpRandom(float a, float b, System.Random rng)
        => (float)(a + (b - a) * rng.NextDouble());

    // ========= SAVE / LOAD (Base64 JSON) =========

    private void SaveBeatMaps()
    {
        try
        {
            string json = JsonUtility.ToJson(beatMapCollection, true);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            File.WriteAllText(saveFilePath, encoded, Encoding.UTF8);
            Debug.Log("BeatMaps saved.");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveBeatMaps failed: {e.Message}");
        }
    }

    private void LoadBeatMaps()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string encoded = File.ReadAllText(saveFilePath, Encoding.UTF8);
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                beatMapCollection = JsonUtility.FromJson<BeatMapCollection>(json) ?? new BeatMapCollection();
                Debug.Log($"Loaded {beatMapCollection.beatMaps.Count} beatmaps.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadBeatMaps failed: {e.Message}");
            beatMapCollection = new BeatMapCollection();
        }
    }

    public BeatMap GetRandomBeatMap()
    {
        if (beatMapCollection.beatMaps.Count == 0) return null;
        int idx = UnityEngine.Random.Range(0, beatMapCollection.beatMaps.Count);
        return beatMapCollection.beatMaps[idx];
    }

    // ========= UI hooks =========
    [ContextMenu("Generate 10 BeatMaps")]
    public void Editor_Generate10() => Generate10BeatMaps();

    public void OnGenerateButtonClick() => Generate10BeatMaps();

    public void OnPlayButtonClick()
    {
        var map = GetRandomBeatMap();
        if (map == null) { Debug.LogWarning("No maps. Generate first!"); return; }

        Debug.Log($"Play BeatMap id={map.mapId} notes={map.beatNotes.Count}");
    }
}
