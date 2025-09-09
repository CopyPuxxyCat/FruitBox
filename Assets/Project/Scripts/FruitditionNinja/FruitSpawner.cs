using UnityEngine;
using System.Collections;

public class FruitSpawner : MonoBehaviour
{
    private BeatmapData beatmap;
    private FDNAudioController audioController;

    void Awake()
    {
        audioController = FindObjectOfType<FDNAudioController>();
    }

    public void LoadBeatmap(BeatmapData map)
    {
        beatmap = map;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var note in beatmap.notes)
        {
            // chờ đến đúng thời điểm trong bài hát
            yield return new WaitUntil(() => audioController.GetSongTime() >= note.time);
            SpawnFruit(note);
        }
    }

    private void SpawnFruit(BeatNote note)
    {
        Debug.Log($"Spawn fruit {note.fruitType} at {note.time}s, angle={note.angle}");

        // TODO: Instantiate prefab theo note.fruitType
        // AddForce lên rigidbody theo note.force + note.angle
    }
}
