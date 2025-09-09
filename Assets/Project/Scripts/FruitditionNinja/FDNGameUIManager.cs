using UnityEngine;

public class FDNGameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject songSelectPanel;  // chọn bài
    [SerializeField] private Transform songListParent;
    [SerializeField] private SongItemUI songItemPrefab;
    [SerializeField] public SongData[] allSongs;

    private void Awake()
    {
        ShowThisPanel(songSelectPanel);
        BuildSongList();
    }

    void CloseAllPanel()
    {
        songSelectPanel.SetActive(false);
    }    

    void BuildSongList()
    {
        foreach (Transform child in songListParent) Destroy(child.gameObject);

        foreach (var song in allSongs)
        {
            FDNDataManager.Instance.LoadSongData(song);

            var item = Instantiate(songItemPrefab, songListParent);
            item.Setup(song, OnSongSelected);
        }
    }

    void OnSongSelected(SongData song)
    {
        FruitditionNinjaGameManager.Instance.StartSong(song);
        songSelectPanel.SetActive(false);
    }

    public void ShowThisPanel(GameObject panelName)
    {
        CloseAllPanel();
        panelName.SetActive(true);
    }    

}
